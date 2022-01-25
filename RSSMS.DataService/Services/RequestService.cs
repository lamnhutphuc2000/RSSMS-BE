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
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RSSMS.DataService.Services
{
    public interface IRequestService : IBaseService<Request>
    {
        Task<DynamicModelResponse<RequestViewModel>> GetAll(RequestViewModel model, string[] fields, int page, int size, string accessToken);
        Task<RequestViewModel> GetById(int id);
        Task<RequestCreateViewModel> Create(RequestCreateViewModel model, string accessToken);
        Task<RequestUpdateViewModel> Update(int id, RequestUpdateViewModel model);
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
        public RequestService(IUnitOfWork unitOfWork, IRequestRepository repository, IMapper mapper
            , IScheduleService scheduleService, INotificationService notificationService
            , INotificationDetailService notificationDetailService, IStaffManageStorageService staffManageStorageService
            , IOrderHistoryExtensionService orderHistoryExtensionService
            ) : base(unitOfWork, repository)
        {
            _mapper = mapper;
            _scheduleService = scheduleService;
            _notificationService = notificationService;
            _notificationDetailService = notificationDetailService;
            _staffManageStorageService = staffManageStorageService;
            _orderHistoryExtensionService = orderHistoryExtensionService;
        }

        public async Task<RequestViewModel> Delete(int id)
        {
            var entity = await GetAsync<int>(id);
            if (entity == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Request id not found");
            entity.IsActive = false;
            await UpdateAsync(entity);
            return _mapper.Map<RequestViewModel>(entity);
        }

        public async Task<DynamicModelResponse<RequestViewModel>> GetAll(RequestViewModel model, string[] fields, int page, int size, string accessToken)
        {
            var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
            var userId = Int32.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);
            var role = secureToken.Claims.First(claim => claim.Type.Contains("role")).Value;

            var requests = Get(x => x.IsActive == true).Include(a => a.User).ThenInclude(b => b.StaffManageStorages);

            if (role == "Manager")
            {
                var storageIds = _staffManageStorageService.Get(x => x.UserId == userId).Select(a => a.StorageId).ToList();
                var staff = _staffManageStorageService.Get(x => storageIds.Contains(x.StorageId)).Select(a => a.UserId).ToList();
                requests = requests.Where(x => staff.Contains((int)x.UserId) || x.UserId == userId).Include(a => a.User).ThenInclude(b => b.StaffManageStorages);
            }

            if (role == "Delivery Staff")
            {
                requests = requests.Where(x => x.UserId == userId).Include(a => a.User).ThenInclude(b => b.StaffManageStorages);
            }

            if (role == "Customer")
            {
                requests = requests.Where(x => x.UserId == userId).Include(a => a.User).ThenInclude(b => b.StaffManageStorages);
            }
            var result = requests.ProjectTo<RequestViewModel>(_mapper.ConfigurationProvider)
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

        public async Task<RequestViewModel> GetById(int id)
        {
            var result = await Get(x => x.Id == id && x.IsActive == true)
               .ProjectTo<RequestViewModel>(_mapper.ConfigurationProvider)
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
            if (role == "Delivery Staff")
            {
                var schedules = _scheduleService.Get(x => x.SheduleDay.Value.Date == model.CancelDay.Date && x.IsActive == true && x.UserId == userId).Include(a => a.User).ToList();
                if(schedules.Count < 1) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Request not found");
                foreach (var schedule in schedules)
                {
                    schedule.IsActive = false;
                    schedule.Status = 1;
                    await _scheduleService.UpdateAsync(schedule);

                    request = _mapper.Map<Request>(model);
                    request.OrderId = schedule.OrderId;
                    request.UserId = userId;
                    await CreateAsync(request);
                }
                var listOrder = schedules.Select(x => x.OrderId).ToList();

                noti = new Notification
                {
                    Description = "Delivery staff " + userId + " cancel delivery of order " + listOrder.ToString(),
                    CreateDate = DateTime.Now,
                    IsActive = true,
                    Type = 0,
                    
                };
                await _notificationService.CreateAsync(noti);

                await _notificationDetailService.PushCancelRequestNoti("Delivery staff " + userId + " cancel delivery of order " + listOrder.ToString(), userId, noti.Id);

                return model;
            }


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

        public async Task<RequestUpdateViewModel> Update(int id, RequestUpdateViewModel model)
        {
            if (id != model.Id) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Request Id not matched");

            var entity = await GetAsync(id);
            if (entity == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Request not found");

            var updateEntity = _mapper.Map(model, entity);
            await UpdateAsync(updateEntity);

            return _mapper.Map<RequestUpdateViewModel>(updateEntity);
        }
    }
}
