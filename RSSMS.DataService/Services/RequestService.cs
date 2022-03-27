﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Constants;
using RSSMS.DataService.Enums;
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
        Task<RequestByIdViewModel> GetById(Guid id);
        Task<RequestCreateViewModel> Create(RequestCreateViewModel model, string accessToken);
        Task<RequestUpdateViewModel> Update(Guid id, RequestUpdateViewModel model, string accessToken);
        Task<RequestViewModel> Delete(Guid id);
        Task<RequestByIdViewModel> AssignStorage(RequestAssignStorageViewModel model, string accessToken);
        Task<RequestByIdViewModel> Cancel(Guid id, RequestCancelViewModel model, string accessToken);
    }


    public class RequestService : BaseService<Request>, IRequestService
    {
        private readonly IMapper _mapper;
        private readonly IScheduleService _scheduleService;
        private readonly IFirebaseService _firebaseService;
        private readonly IStaffAssignStoragesService _staffAssignStoragesService;
        private readonly IOrderHistoryExtensionService _orderHistoryExtensionService;
        public RequestService(IUnitOfWork unitOfWork, IRequestRepository repository, IMapper mapper
            , IScheduleService scheduleService
            , IFirebaseService firebaseService, IStaffAssignStoragesService staffAssignStoragesService
            , IOrderHistoryExtensionService orderHistoryExtensionService
            ) : base(unitOfWork, repository)
        {
            _mapper = mapper;
            _scheduleService = scheduleService;
            _firebaseService = firebaseService;
            _staffAssignStoragesService = staffAssignStoragesService;
            _orderHistoryExtensionService = orderHistoryExtensionService;
        }

        public async Task<RequestViewModel> Delete(Guid id)
        {
            var entity = await GetAsync(id);
            if (entity == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Request id not found");
            entity.IsActive = false;
            await UpdateAsync(entity);
            return _mapper.Map<RequestViewModel>(entity);
        }

        public async Task<DynamicModelResponse<RequestViewModel>> GetAll(RequestViewModel model, IList<int> RequestTypes, string[] fields, int page, int size, string accessToken)
        {
            var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
            var userId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);
            var role = secureToken.Claims.First(claim => claim.Type.Contains("role")).Value;

            var requests = Get(x => x.IsActive == true).Include(a => a.Schedules).Include(a => a.CreatedByNavigation).ThenInclude(b => b.StaffAssignStorages)
                .Include(x => x.Storage)
                .Include(x => x.Order)
                .ThenInclude(order => order.Storage);
            if (model.FromDate != null && model.ToDate != null)
            {
                requests = Get(x => x.IsActive == true && x.DeliveryDate.Value.Date >= model.FromDate.Value.Date && x.DeliveryDate <= model.ToDate.Value.Date)
                    .Include(a => a.Schedules).Include(a => a.CreatedByNavigation).ThenInclude(b => b.StaffAssignStorages)
                    .Include(x => x.Storage)
                    .Include(x => x.Order)
                    .ThenInclude(order => order.Storage);
            }

            if (RequestTypes != null)
            {
                if (RequestTypes.Count > 0)
                {
                    requests = Get(x => x.IsActive == true).Where(x => RequestTypes.Contains((int)x.Type)).Include(a => a.Schedules).Include(a => a.CreatedByNavigation).ThenInclude(b => b.StaffAssignStorages)
                    .Include(x => x.Storage)
                     .Include(x => x.Order)
                    .ThenInclude(order => order.Storage);
                }
            }
            if (role == "Manager")
            {
                var storageIds = _staffAssignStoragesService.Get(x => x.StaffId == userId).Select(a => a.StorageId).ToList();
                var staff = _staffAssignStoragesService.Get(x => storageIds.Contains(x.StorageId)).Select(a => a.StaffId).ToList();
                requests = requests.Where(x => staff.Contains((Guid)x.CreatedBy) || x.CreatedBy == userId || x.CreatedByNavigation.Role.Name == "Customer").Include(a => a.Schedules).Include(a => a.CreatedByNavigation).ThenInclude(b => b.StaffAssignStorages)
                    .Include(x => x.Storage)
                    .Include(x => x.Order)
                    .ThenInclude(order => order.Storage);
            }

            if (role == "Delivery Staff")
            {
                requests = requests.Where(x => x.CreatedBy == userId).Include(a => a.CreatedByNavigation).ThenInclude(b => b.StaffAssignStorages).Include(x => x.Storage).Include(x => x.Order)
                .ThenInclude(x => x.Storage);
            }

            if (role == "Customer")
            {
                requests = requests.Where(x => x.CreatedBy == userId).Include(a => a.CreatedByNavigation).ThenInclude(b => b.StaffAssignStorages)
                    .Include(x => x.Storage)
                    .Include(x => x.Order)
                    .ThenInclude(x => x.Storage);
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

        public async Task<RequestByIdViewModel> GetById(Guid id)
        {
            var result = await Get(x => x.Id == id && x.IsActive)
                .Include(request => request.RequestDetails).ThenInclude(requestDetail => requestDetail.Service)
                .Include(request => request.CreatedByNavigation).ThenInclude(createdBy => createdBy.Role)
               .ProjectTo<RequestByIdViewModel>(_mapper.ConfigurationProvider)
               .FirstOrDefaultAsync();
            if (result == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Request id not found");
            return result;
        }

        public async Task<RequestCreateViewModel> Create(RequestCreateViewModel model, string accessToken)
        {
            var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
            var userId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);
            var role = secureToken.Claims.First(claim => claim.Type.Contains("role")).Value;

            Request request = null;
            if (role == "Delivery Staff" && model.Type == (int)RequestType.Cancel_Order) // huy lich giao hang
            {
                var schedules = _scheduleService.Get(x => x.ScheduleDay.Date == model.CancelDay.Value.Date && x.IsActive == true && x.UserId == userId).Include(a => a.User).ToList();
                if (schedules.Count < 1) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Schedule not found");
                foreach (var schedule in schedules)
                {
                    request = _mapper.Map<Request>(model);
                    request.OrderId = schedule.Request.OrderId;
                    request.CreatedBy = userId;
                    request.Status = 0;
                    await CreateAsync(request);

                    schedule.IsActive = false;
                    //schedule.Status = 1;
                    schedule.RequestId = request.Id;
                    await _scheduleService.UpdateAsync(schedule);
                }
                var user = schedules.Select(x => x.User).Where(x => x.Id == userId).FirstOrDefault();

                await _firebaseService.PushCancelRequestNoti("Delivery staff " + user.Name + " canceled schedule on " + model.CancelDay, user.Id);

                return model;
            }

            if (model.Type == (int)RequestType.Extend_Order) // gia han don
            {
                request = _mapper.Map<Request>(model);
                request.CreatedBy = userId;

                await CreateAsync(request);

                OrderHistoryExtension orderExtend = _mapper.Map<OrderHistoryExtension>(model);
                orderExtend.ModifiedBy = userId;
                orderExtend.RequestId = request.Id;
                orderExtend.OrderId = (Guid)model.OrderId;
                await _orderHistoryExtensionService.CreateAsync(orderExtend);
                var staffAssignInStorage = _orderHistoryExtensionService.Get(x => x.OrderId == model.OrderId).Include(x => x.Order).ThenInclude(order => order.Storage).ThenInclude(storage => storage.StaffAssignStorages.Where(staff => staff.RoleName == "Manager" && staff.IsActive == true)).ThenInclude(staffAssignInStorage => staffAssignInStorage.Staff).Select(x => x.Order.Storage.StaffAssignStorages.FirstOrDefault()).FirstOrDefault();
                
                await _firebaseService.SendNoti("Customer " + userId + " expand the order: " + model.OrderId, userId, staffAssignInStorage.Staff.DeviceTokenId, request.Id, new
                {
                    Content = "Customer " + userId + " expand the order: " + model.OrderId,
                    OrderId = model.OrderId,
                    RequestId = request.Id
                });
                return model;
            }
            Order order = null;
            Request newRequest = null;
            if (model.Type == (int)RequestType.Return_Order) // rut do ve
            {
                request = _mapper.Map<Request>(model);
                request.CreatedBy = userId;
                request.Status = 1;
                await CreateAsync(request);


                newRequest = Get(x => x.Id == request.Id && x.IsActive == true).Include(request => request.Order)
                    .Include(order => order.Storage).ThenInclude(storage => storage.StaffAssignStorages.Where(staff => staff.RoleName == "Manager" && staff.IsActive == true)).ThenInclude(taffAssignStorage => taffAssignStorage.Staff).FirstOrDefault();
                if (newRequest.Order == null)
                    throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Order not found");
                if (newRequest.Order.Storage == null)
                    throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Order not assigned yet");

                var staffAssignInStorage = newRequest.Order.Storage.StaffAssignStorages.Where(x => x.RoleName == "Manager" && x.IsActive == true).FirstOrDefault();
                if (staffAssignInStorage == null) return model;
                await _firebaseService.SendNoti("Customer " + userId + " take back the order: " + model.OrderId, userId, staffAssignInStorage.Staff.DeviceTokenId, request.Id, new
                {
                    Content = "Customer " + userId + " take back the order: " + model.OrderId,
                    OrderId = model.OrderId,
                    RequestId = request.Id
                });
                return model;
            }
            if(model.Type == (int)RequestType.Create_Order)
            {
                request = _mapper.Map<Request>(model);
                request.CreatedBy = userId;
                if (role == "Customer") request.CustomerId = userId;
                await CreateAsync(request);
                

                await _firebaseService.PushOrderNoti("New request arrive!", null, request.Id);
                return model;
            }

            // customer huy don
            
            request = _mapper.Map<Request>(model);
            request.CreatedBy = userId;
            await CreateAsync(request);
            newRequest = Get(x => x.Id == request.Id && x.IsActive == true).Include(request => request.Order)
                    .Include(order => order.Storage).ThenInclude(storage => storage.StaffAssignStorages.Where(staff => staff.RoleName == "Manager" && staff.IsActive == true)).ThenInclude(taffAssignStorage => taffAssignStorage.Staff).FirstOrDefault();
            if (newRequest.Order == null)
                throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Order not found");
            if (newRequest.Order.Storage == null)
                throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Order not assigned yet");
            newRequest.Order.RejectedReason = model.Note;
            newRequest.Order.Status = 0;
            await UpdateAsync(newRequest);
            

            var manager = order.Storage.StaffAssignStorages.Where(x => x.Staff.Role.Name == "Manager" && x.IsActive == true).FirstOrDefault();
            await _firebaseService.SendNoti("Customer " + userId + " cancel the order: " + model.OrderId, userId, manager.Staff.DeviceTokenId, request.Id, new
            {
                Content = "Customer " + userId + " cancel the order: " + model.OrderId,
                OrderId = model.OrderId,
                RequestId = request.Id
            });

            return model;
        }

        public async Task<RequestUpdateViewModel> Update(Guid id, RequestUpdateViewModel model, string accessToken)
        {
            var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
            var userId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);

            if (id != model.Id) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Request Id not matched");

            var request = await Get(x => x.Id == id && x.IsActive == true).Include(x => x.Order).ThenInclude(order => order.OrderHistoryExtensions).FirstOrDefaultAsync();
            if (request == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Request not found");

            request.Status = 2;
            request.IsPaid = model.IsPaid;
            request.ModifiedBy = userId;
            await UpdateAsync(request);

            var orderHistoryExtend = request.Order.OrderHistoryExtensions.Where(x => x.RequestId == id).FirstOrDefault();
            if (orderHistoryExtend == null)
                throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Order extend not found");
            orderHistoryExtend.PaidDate = DateTime.Now;
            orderHistoryExtend.ModifiedBy = userId;
            await _orderHistoryExtensionService.UpdateAsync(orderHistoryExtend);

            var order = orderHistoryExtend.Order;
            order.ReturnDate = orderHistoryExtend.ReturnDate;
            order.IsPaid = model.IsPaid;
            order.ModifiedBy = userId;
            order.ModifiedDate = DateTime.Now;
            orderHistoryExtend.Order = order;
            await _orderHistoryExtensionService.UpdateAsync(orderHistoryExtend);

            return _mapper.Map<RequestUpdateViewModel>(model);
        }

        public async Task<RequestByIdViewModel> AssignStorage(RequestAssignStorageViewModel model, string accessToken)
        {
            try
            {
                var request = await Get(request => request.Id == model.RequestId && request.IsActive).FirstOrDefaultAsync();
                if (request == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Request not found");

                var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
                var userId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);

                request.StorageId = model.StorageId;
                request.ModifiedBy = userId;
                request.Status = 2;
                await UpdateAsync(request);
                return _mapper.Map<RequestByIdViewModel>(request);
            }
            catch(Exception e)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, e.Message);
            }
        }

        public async Task<RequestByIdViewModel> Cancel(Guid id, RequestCancelViewModel model, string accessToken)
        {
            if (id != model.Id) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Request Id not matched");

            var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
            var userId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);

            var entity = await Get(x => x.Id == id && x.IsActive == true).FirstOrDefaultAsync();
            if (entity == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Request not found");

            entity.Status = 0;
            entity.CancelReason = model.CancelReason;
            await UpdateAsync(entity);

            return await GetById(id);
        }
    }
}
