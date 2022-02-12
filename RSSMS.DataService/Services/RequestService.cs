using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Constants;
using RSSMS.DataService.Models;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.Responses;
using RSSMS.DataService.UnitOfWorks;
using RSSMS.DataService.Utilities;
using RSSMS.DataService.ViewModels.Requests;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RSSMS.DataService.Services
{
    public interface IRequestService : IBaseService<Request>
    {
        Task<DynamicModelResponse<RequestViewModel>> GetAll(RequestViewModel model, IList<int> RequestTypes, string[] fields, int page, int size, string accessToken);
        Task<RequestByIdViewModel> GetById(int id);
        Task<RequestCreateViewModel> Create(RequestCreateViewModel model, string accessToken);
        Task<RequestUpdateViewModel> Update(int id, RequestUpdateViewModel model, string accessToken);
        Task<RequestViewModel> Delete(int id);
    }


    public class RequestService : BaseService<Request>, IRequestService
    {
        private readonly IMapper _mapper;
        private readonly IScheduleService _scheduleService;
        private readonly INotificationService _notificationService;
        private readonly INotificationDetailService _notificationDetailService;
        private readonly IStaffManageStorageService _staffManageStorageService;
        private readonly IOrderHistoryExtensionService _orderHistoryExtensionService;
        private readonly IOrderService _orderService;
        public RequestService(IUnitOfWork unitOfWork, IRequestRepository repository, IMapper mapper
            , IScheduleService scheduleService, INotificationService notificationService
            , INotificationDetailService notificationDetailService, IStaffManageStorageService staffManageStorageService
            , IOrderHistoryExtensionService orderHistoryExtensionService
            , IOrderService orderService
            ) : base(unitOfWork, repository)
        {
            _mapper = mapper;
            _scheduleService = scheduleService;
            _notificationService = notificationService;
            _notificationDetailService = notificationDetailService;
            _staffManageStorageService = staffManageStorageService;
            _orderHistoryExtensionService = orderHistoryExtensionService;
            _orderService = orderService;
        }

        public async Task<RequestViewModel> Delete(int id)
        {
            var entity = await GetAsync<int>(id);
            if (entity == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Request id not found");
            entity.IsActive = false;
            await UpdateAsync(entity);
            return _mapper.Map<RequestViewModel>(entity);
        }

        public async Task<DynamicModelResponse<RequestViewModel>> GetAll(RequestViewModel model, IList<int> RequestTypes, string[] fields, int page, int size, string accessToken)
        {
            var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
            var userId = Int32.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);
            var role = secureToken.Claims.First(claim => claim.Type.Contains("role")).Value;

            var requests = Get(x => x.IsActive == true).Include(a => a.Schedules).Include(a => a.User).ThenInclude(b => b.StaffManageStorages);

            if (RequestTypes != null)
            {
                if (RequestTypes.Count > 0)
                {
                    requests = Get(x => x.IsActive == true).Where(x => RequestTypes.Contains((int)x.Type)).Include(a => a.Schedules).Include(a => a.User).ThenInclude(b => b.StaffManageStorages);
                }
            }
            if (role == "Manager")
            {
                var storageIds = _staffManageStorageService.Get(x => x.UserId == userId).Select(a => a.StorageId).ToList();
                var staff = _staffManageStorageService.Get(x => storageIds.Contains(x.StorageId)).Select(a => a.UserId).ToList();
                requests = requests.Where(x => staff.Contains((int)x.UserId) || x.UserId == userId).Include(a => a.Schedules).Include(a => a.User).ThenInclude(b => b.StaffManageStorages);
            }

            if (role == "Delivery Staff")
            {
                requests = requests.Where(x => x.UserId == userId).Include(a => a.User).ThenInclude(b => b.StaffManageStorages);
            }

            if (role == "Customer")
            {
                requests = requests.Where(x => x.UserId == userId).Include(a => a.User).ThenInclude(b => b.StaffManageStorages);
            }
            var result = requests.OrderByDescending(x => x.CreatedDate).ProjectTo<RequestViewModel>(_mapper.ConfigurationProvider)
                    .PagingIQueryable(page, size, CommonConstant.LimitPaging, CommonConstant.DefaultPaging);
            if (result.Item2.ToList().Count < 1) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Can not found");
            var rs = new DynamicModelResponse<RequestViewModel>
            {
                Metadata = new PagingMetaData
                {
                    Page = page,
                    Size = size,
                    Total = result.Item1,
                    TotalPage = (int)Math.Ceiling((double)result.Item1 / size)
                },
                Data = result.Item2.ToList()
            };
            return rs;
        }

        public async Task<RequestByIdViewModel> GetById(int id)
        {
            var result = await Get(x => x.Id == id && x.IsActive == true)
                .Include(x => x.Order)
                .ThenInclude(order => order.OrderHistoryExtensions.Where(a => a.IsActive == true).OrderByDescending(orderHistory => orderHistory.CreateDate))
               .ProjectTo<RequestByIdViewModel>(_mapper.ConfigurationProvider)
               .FirstOrDefaultAsync();
            if (result == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Request id not found");
            return result;
        }

        public async Task<RequestViewModel> Update(int id, RequestViewModel model)
        {
            if (id != model.Id) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Request Id not matched");

            var entity = await GetAsync(id);
            if (entity == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Request not found");

            var updateEntity = _mapper.Map(model, entity);
            await UpdateAsync(updateEntity);

            return _mapper.Map<RequestViewModel>(updateEntity);
        }

        public async Task<RequestCreateViewModel> Create(RequestCreateViewModel model, string accessToken)
        {
            var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
            var userId = Int32.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);
            var role = secureToken.Claims.First(claim => claim.Type.Contains("role")).Value;

            Request request = null;
            Notification noti = null;
            if (role == "Delivery Staff" && model.Type == 0) // huy lich giao hang
            {
                var schedules = _scheduleService.Get(x => x.SheduleDay.Value.Date == model.CancelDay.Value.Date && x.IsActive == true && x.UserId == userId).Include(a => a.User).ToList();
                if(schedules.Count < 1) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Request not found");
                foreach (var schedule in schedules)
                {
                    request = _mapper.Map<Request>(model);
                    request.OrderId = schedule.OrderId;
                    request.UserId = userId;
                    await CreateAsync(request);

                    schedule.IsActive = false;
                    schedule.Status = 1;
                    schedule.RequestId = request.Id;
                    await _scheduleService.UpdateAsync(schedule);
                }
                var user = schedules.Select(x => x.User).Where(x => x.Id == userId).FirstOrDefault();
                noti = new Notification
                {
                    Description = "Delivery staff " + user.Name + " canceled schedule on " + model.CancelDay,
                    CreateDate = DateTime.Now,
                    IsActive = true,
                    Type = 0
                };
                await _notificationService.CreateAsync(noti);

                await _notificationDetailService.PushCancelRequestNoti("Delivery staff " + user.Name + " canceled schedule on " + model.CancelDay, userId, noti.Id);

                return model;
            }

            if(model.Type == 1) // gia han don
            {
                request = _mapper.Map<Request>(model);
                request.UserId = userId;

                await CreateAsync(request);

                noti = new Notification
                {
                    Description = "Customer " + userId + " expand the order: " + model.OrderId,
                    CreateDate = DateTime.Now,
                    IsActive = true,
                    Type = 0,
                    OrderId = model.OrderId
                };
                OrderHistoryExtension orderExtend = _mapper.Map<OrderHistoryExtension>(model);
                orderExtend.ModifiedBy = userId;
                orderExtend.RequestId = request.Id;
                await _orderHistoryExtensionService.CreateAsync(orderExtend);
                var manager = _orderHistoryExtensionService.Get(x => x.OrderId == model.OrderId).Include(x => x.Order).ThenInclude(x => x.Manager).Select(x => x.Order.Manager).FirstOrDefault();
                await _notificationService.CreateAsync(noti);
                await _notificationDetailService.SendNoti("Customer " + userId + " expand the order: " + model.OrderId, userId, manager.Id, manager.DeviceTokenId, noti.Id, (int)model.OrderId, request.Id, new
                {
                    Content = "Customer " + userId + " expand the order: " + model.OrderId,
                    OrderId = model.OrderId,
                    RequestId = request.Id
                });
                return model;
            }
            Order order = null;
            if(model.Type == 2) // rut do ve
            {
                order = _orderService.Get(x => x.Id == model.OrderId && x.IsActive == true && x.CustomerId == userId).Include(x => x.Manager).FirstOrDefault();
                if (order == null)
                    throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Order not found");
                if (order.Manager == null)
                    throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Manager not found");
                request = _mapper.Map<Request>(model);
                request.UserId = userId;
                await CreateAsync(request);



                noti = new Notification
                {
                    Description = "Customer " + userId + " take back the order: " + model.OrderId,
                    CreateDate = DateTime.Now,
                    IsActive = true,
                    Type = 3,
                    OrderId = model.OrderId
                };
                await _notificationService.CreateAsync(noti);
                await _notificationDetailService.SendNoti("Customer " + userId + " take back the order: " + model.OrderId, userId, order.Manager.Id, order.Manager.DeviceTokenId, noti.Id, (int)model.OrderId, request.Id, new
                {
                    Content = "Customer " + userId + " take back the order: " + model.OrderId,
                    OrderId = model.OrderId,
                    RequestId = request.Id
                });
                return model;
            }

            // customer huy don
            order = _orderService.Get(x => x.Id == model.OrderId && x.IsActive == true && x.CustomerId == userId).Include(x => x.Manager).FirstOrDefault();
            if (order == null)
                throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Order not found");
            if (order.Manager == null)
                throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Manager not found");
            request = _mapper.Map<Request>(model);
            request.UserId = userId;
            await CreateAsync(request);
            order.RejectedReason = model.Note;
            order.Status = 0;
            await _orderService.UpdateAsync(order);
            noti = new Notification
            {
                Description = "Customer " + userId + " cancel the order: " + model.OrderId,
                CreateDate = DateTime.Now,
                IsActive = true,
                Type = 4,
                OrderId = model.OrderId
            };
            await _notificationService.CreateAsync(noti);
            await _notificationDetailService.SendNoti("Customer " + userId + " cancel the order: " + model.OrderId, userId, order.Manager.Id, order.Manager.DeviceTokenId, noti.Id, (int)model.OrderId, request.Id, new
            {
                Content = "Customer " + userId + " cancel the order: " + model.OrderId,
                OrderId = model.OrderId,
                RequestId = request.Id
            });

            return model;
        }

        public async Task<RequestUpdateViewModel> Update(int id, RequestUpdateViewModel model, string accessToken)
        {
            var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
            var userId = Int32.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);

            if (id != model.Id) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Request Id not matched");

            var entity = await Get(x => x.Id == id && x.IsActive == true).FirstOrDefaultAsync();
            if (entity == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Request not found");

            var orderHistoryExtend = entity.OrderHistoryExtensions.FirstOrDefault();
            if(orderHistoryExtend == null)
                throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Order extend not found");
            orderHistoryExtend.PaidDate = DateTime.Now;
            await _orderHistoryExtensionService.UpdateAsync(orderHistoryExtend);

            var order = _orderService.Get(x => x.Id == orderHistoryExtend.OrderId).FirstOrDefault();
            order.ReturnDate = orderHistoryExtend.ReturnDate;
            order.ModifiedBy = userId;
            order.ModifiedDate = DateTime.Now;
            await _orderService.UpdateAsync(order);

            return _mapper.Map<RequestUpdateViewModel>(model);
        }
    }
}
