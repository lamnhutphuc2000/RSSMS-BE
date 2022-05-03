using _3DBinPacking.Model;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Constants;
using RSSMS.DataService.Enums;
using RSSMS.DataService.Models;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.Responses;
using RSSMS.DataService.UnitOfWorks;
using RSSMS.DataService.Utilities;
using RSSMS.DataService.ViewModels.Notifications;
using RSSMS.DataService.ViewModels.Requests;
using RSSMS.DataService.ViewModels.Storages;
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
        Task<DynamicModelResponse<RequestViewModel>> GetAll(RequestViewModel model, IList<int> RequestTypes, IList<int> RequestStatus, string[] fields, int page, int size, string accessToken);
        Task<RequestByIdViewModel> GetById(Guid id, string accessToken);
        Task<RequestByIdViewModel> Create(RequestCreateViewModel model, string accessToken);
        Task<RequestUpdateViewModel> Update(Guid id, RequestUpdateViewModel model, string accessToken);
        Task<RequestViewModel> Delete(Guid id);
        Task<RequestByIdViewModel> AssignStorage(RequestAssignStorageViewModel model, string accessToken);
        Task<RequestByIdViewModel> Cancel(Guid id, RequestCancelViewModel model, string accessToken);
        Task<RequestByIdViewModel> DeliverRequest(Guid id, string accessToken);
        Task<RequestByIdViewModel> DeliverySendRequestNotification(Guid id, NotificationDeliverySendRequestNotiViewModel model, string accessToken);
        Task<bool> CheckStorageAvailable(int spaceType, bool isMany, int orderType, DateTime dateFrom, DateTime dateTo, List<Cuboid> cuboids, Guid? storageId, bool isCustomerDelivery, Guid? requestId, string accessToken, List<string> deliveryTimes, bool isCreateOrder);
        Task<List<StorageViewModel>> GetStorageAvailable(RequestCreateViewModel model, string accessToken);
    }


    public class RequestService : BaseService<Request>, IRequestService
    {
        private readonly IMapper _mapper;
        private readonly IScheduleService _scheduleService;
        private readonly IFirebaseService _firebaseService;
        private readonly IStaffAssignStorageService _staffAssignStoragesService;
        private readonly IOrderHistoryExtensionService _orderHistoryExtensionService;
        private readonly IRequestTimelineService _requestTimelineService;
        private readonly IAccountService _accountService;
        private readonly IStorageService _storageService;
        private readonly IServiceService _serviceService;
        private readonly IOrderTimelineService _orderTimelineService;
        private readonly IUtilService _utilService;
        public RequestService(IUnitOfWork unitOfWork, IRequestRepository repository, IMapper mapper
            , IScheduleService scheduleService
            , IFirebaseService firebaseService, IStaffAssignStorageService staffAssignStoragesService
            , IOrderHistoryExtensionService orderHistoryExtensionService
            , IRequestTimelineService requestTimelineService
            , IAccountService accountService
            , IStorageService storageService
            , IServiceService serviceService
            , IOrderTimelineService orderTimelineService
            , IUtilService utilService
            ) : base(unitOfWork, repository)
        {
            _mapper = mapper;
            _scheduleService = scheduleService;
            _firebaseService = firebaseService;
            _staffAssignStoragesService = staffAssignStoragesService;
            _orderHistoryExtensionService = orderHistoryExtensionService;
            _requestTimelineService = requestTimelineService;
            _accountService = accountService;
            _orderTimelineService = orderTimelineService;
            _storageService = storageService;
            _serviceService = serviceService;
            _utilService = utilService;
        }

        public async Task<RequestViewModel> Delete(Guid id)
        {
            try
            {
                var entity = await Get(request => request.Id == id && request.IsActive).FirstOrDefaultAsync();
                if (entity == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Request id not found");
                entity.IsActive = false;
                await UpdateAsync(entity);
                return _mapper.Map<RequestViewModel>(entity);
            }
            catch (ErrorResponse e)
            {
                throw new ErrorResponse((int)e.Error.Code, e.Error.Message);
            }
            catch (Exception ex)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<DynamicModelResponse<RequestViewModel>> GetAll(RequestViewModel model, IList<int> RequestTypes, IList<int> RequestStatus, string[] fields, int page, int size, string accessToken)
        {
            try
            {
                var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
                var userId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);
                var role = secureToken.Claims.First(claim => claim.Type.Contains("role")).Value;
                var account = _accountService.Get(account => account.Id == userId && account.IsActive).Include(account => account.StaffAssignStorages).Include(account => account.Role).FirstOrDefault();
                if (account == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Không tìm thấy tài khoản");
                var staffStorage = account.StaffAssignStorages.Where(staffAssign => staffAssign.IsActive).Select(staffAssign => staffAssign.StorageId).FirstOrDefault();
                if (account.Role.Name == "Office Staff" && staffStorage == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Thủ kho chưa được phân công vào kho");
                var requests = Get(request => request.IsActive)
                    .Include(request => request.Schedules)
                    .Include(request => request.CreatedByNavigation).ThenInclude(createdBy => createdBy.StaffAssignStorages)
                    .Include(request => request.Storage)
                    .Include(request => request.Order).ThenInclude(order => order.Storage);
                if (model.FromDate != null && model.ToDate != null)
                {
                    requests = Get(request => request.IsActive && request.DeliveryDate.Value.Date >= model.FromDate.Value.Date && request.DeliveryDate <= model.ToDate.Value.Date)
                        .Include(request => request.Schedules)
                        .Include(request => request.CreatedByNavigation).ThenInclude(createdBy => createdBy.StaffAssignStorages)
                        .Include(request => request.Storage)
                        .Include(request => request.Order).ThenInclude(order => order.Storage);
                }

                if (RequestTypes != null)
                {
                    if (RequestTypes.Count > 0)
                    {
                        requests = requests.Where(request => RequestTypes.Contains(request.Type))
                            .Include(request => request.Schedules)
                            .Include(request => request.CreatedByNavigation).ThenInclude(createdBy => createdBy.StaffAssignStorages)
                            .Include(request => request.Storage)
                            .Include(request => request.Order).ThenInclude(order => order.Storage);
                    }
                }
                if (RequestStatus != null)
                {
                    if (RequestStatus.Count > 0)
                    {
                        requests = requests.Where(request => RequestStatus.Contains((int)request.Status))
                            .Include(request => request.Schedules)
                            .Include(request => request.CreatedByNavigation).ThenInclude(createdBy => createdBy.StaffAssignStorages)
                            .Include(request => request.Storage)
                            .Include(request => request.Order).ThenInclude(order => order.Storage);
                    }
                }
                if (role == "Admin")
                {
                    requests = requests.Where(request => request.CreatedByNavigation.Role.Name == "Customer")
                        .Include(request => request.Schedules)
                        .Include(request => request.CreatedByNavigation).ThenInclude(createdBy => createdBy.StaffAssignStorages)
                        .Include(request => request.Storage)
                        .Include(request => request.Order).ThenInclude(order => order.Storage);
                }
                if (role == "Manager")
                {
                    var storageIds = account.StaffAssignStorages.Where(staffAssign => staffAssign.IsActive).Select(staffAssign => staffAssign.StorageId).ToList();
                    var staff = _staffAssignStoragesService.Get(staffAssignStorage => storageIds.Contains(staffAssignStorage.StorageId)).Select(a => a.StaffId).ToList();

                    requests = requests.Where(request => storageIds.Contains((Guid)request.StorageId) || storageIds.Contains((Guid)request.Order.StorageId) || request.CreatedByNavigation.StaffAssignStorages.Where(staff => staff.IsActive && storageIds.Contains((Guid)staff.StorageId)).FirstOrDefault() != null)
                        .Include(request => request.Schedules)
                        .Include(request => request.CreatedByNavigation).ThenInclude(createdBy => createdBy.StaffAssignStorages)
                        .Include(request => request.Storage)
                        .Include(request => request.Order).ThenInclude(order => order.Storage);

                }
                if (role == "Office Staff")
                {
                    requests = requests.Where(request => request.StorageId == staffStorage || request.Order.StorageId == staffStorage)
                                    .Include(request => request.Schedules)
                                    .Include(request => request.CreatedByNavigation).ThenInclude(createdBy => createdBy.StaffAssignStorages)
                                    .Include(request => request.Storage)
                                    .Include(request => request.Order).ThenInclude(order => order.Storage);
                }

                if (role == "Delivery Staff")
                {
                    requests = requests.Where(request => request.CreatedBy == userId)
                        .Include(request => request.Schedules)
                        .Include(request => request.CreatedByNavigation).ThenInclude(createdBy => createdBy.StaffAssignStorages)
                        .Include(request => request.Storage)
                        .Include(request => request.Order).ThenInclude(order => order.Storage);
                }

                if (role == "Customer")
                {
                    requests = requests.Where(x => x.CreatedBy == userId)
                        .Include(request => request.Schedules)
                        .Include(request => request.CreatedByNavigation).ThenInclude(createdBy => createdBy.StaffAssignStorages)
                        .Include(request => request.Storage)
                        .Include(request => request.Order).ThenInclude(order => order.Storage);
                }

                var result = requests.OrderByDescending(request => request.CreatedDate).ProjectTo<RequestViewModel>(_mapper.ConfigurationProvider)
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
                    Data = await result.Item2.ToListAsync()
                };
                return rs;
            }
            catch (ErrorResponse e)
            {
                throw new ErrorResponse((int)e.Error.Code, e.Error.Message);
            }
            catch (Exception ex)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<RequestByIdViewModel> GetById(Guid id, string accessToken)
        {
            try
            {
                var request = Get(request => request.Id == id && request.IsActive)
                    .Include(request => request.Schedules)
                    .Include(request => request.Order)
                    .Include(request => request.Storage)
                    .Include(request => request.RequestDetails).ThenInclude(requestDetail => requestDetail.Service)
                    .Include(request => request.CreatedByNavigation).ThenInclude(createdBy => createdBy.Role);

                var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
                var userId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);
                var role = secureToken.Claims.First(claim => claim.Type.Contains("role")).Value;


                var acc = _accountService.Get(account => account.IsActive && account.Id == userId)
                        .Include(acc => acc.StaffAssignStorages.Where(staffAssign => staffAssign.IsActive)).FirstOrDefault();

                Guid? storageId = null;
                if (acc != null) storageId = acc.StaffAssignStorages.FirstOrDefault()?.StorageId;
                if (role == "Delivery Staff" && storageId != null)
                    request = request.Where(request => (request.StorageId == storageId || request.Order.StorageId == storageId) && request.Schedules.Where(schedule => schedule.IsActive && schedule.StaffId == userId).Count() > 0)
                                .Include(request => request.Schedules)
                                .Include(request => request.Order)
                                .Include(request => request.Storage)
                                .Include(request => request.RequestDetails).ThenInclude(requestDetail => requestDetail.Service)
                                .Include(request => request.CreatedByNavigation).ThenInclude(createdBy => createdBy.Role);

                if (role == "Office Staff" && storageId != null)
                    request = request.Where(request => request.StorageId == storageId || request.Order.StorageId == storageId)
                                .Include(request => request.Schedules)
                                .Include(request => request.Order)
                                .Include(request => request.Storage)
                                .Include(request => request.RequestDetails).ThenInclude(requestDetail => requestDetail.Service)
                                .Include(request => request.CreatedByNavigation).ThenInclude(createdBy => createdBy.Role);

                if ((role == "Delivery Staff" || role == "Office Staff") && storageId == null)
                    throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Nhân viên chưa vào kho");
                var result = await request.ProjectTo<RequestByIdViewModel>(_mapper.ConfigurationProvider).FirstOrDefaultAsync();
                if (result == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Không tìm được yêu cầu");
                if (!string.IsNullOrWhiteSpace(result.Note))
                {
                    var maxServiceDeliveryFee = result.RequestDetails.Select(requestDetail => requestDetail.ServiceDeliveryFee).Max();
                    result.DeliveryFee = maxServiceDeliveryFee * Math.Ceiling(Convert.ToDecimal(result.Note.Split(' ')[0]));
                }

                if (result.Type != 2) return result;
                var orderHistoryExtension = _orderHistoryExtensionService.Get(orderHistory => orderHistory.RequestId == result.Id).FirstOrDefault();
                if (orderHistoryExtension != null) result.TotalPrice = orderHistoryExtension.TotalPrice;
                return result;
            }
            catch (ErrorResponse e)
            {
                throw new ErrorResponse((int)e.Error.Code, e.Error.Message);
            }
            catch (Exception ex)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, ex.Message);
            }

        }

        public async Task<RequestByIdViewModel> Create(RequestCreateViewModel model, string accessToken)
        {
            try
            {
                var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
                var userId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);
                var role = secureToken.Claims.First(claim => claim.Type.Contains("role")).Value;
                Guid? storageId = null;
                if (role == "Office Staff") storageId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "storage_id").Value);
                Request request = null;
                Order order = null;
                Request newRequest = null;

                if (model.Type == (int)RequestType.Tao_don) // customer tao yeu cau tao don
                {


                    // check xem yêu cầu tạo đơn giữ đồ thuê hay kho tự quản => dẫn tới cần sử dụng space gì
                    int spaceType = 0;
                    if (model.TypeOrder == (int)OrderType.Kho_tu_quan) spaceType = 1;

                    // check xem là giữ ít hay nhiều 
                    bool isMany = false;

                    // service list chứa list service người dùng đặt
                    List<Cuboid> cuboid = new List<Cuboid>();
                    var services = model.RequestDetails.Select(requestDetail => new
                    {
                        ServiceId = requestDetail.ServiceId,
                        Amount = requestDetail.Amount
                    }).ToList();
                    decimal totalPrice = 0;
                    decimal month = Math.Ceiling((decimal)model.ReturnDate.Value.Date.Subtract(model.DeliveryDate.Value.Date).Days / 30);

                    if (spaceType == 1) month = (model.ReturnDate.Value.Date.Subtract(model.DeliveryDate.Value.Date).Days / 30);

                    int serviceType = -1;
                    decimal serviceDeliveryFee = 0;
                    for (int i = 0; i < services.Count; i++)
                    {
                        for (int j = 0; j < services[i].Amount; j++)
                        {
                            var service = _serviceService.Get(service => service.Id == services[i].ServiceId).FirstOrDefault();
                            if (!service.IsActive) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Dịch vụ không tồn tại");
                            if (service.Type == (int)ServiceType.Gui_theo_dien_tich) isMany = true;
                            if (service.Type != (int)ServiceType.Phu_kien)
                            {
                                if (serviceDeliveryFee < service.DeliveryFee) serviceDeliveryFee = (decimal)service.DeliveryFee;
                                totalPrice += service.Price * month;
                                if (serviceType == -1) serviceType = (int)service.Type;
                                if (serviceType != -1 && serviceType != service.Type) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Không thể đặt 1 đơn 2 loại dịch vụ chính");
                                cuboid.Add(new Cuboid((decimal)service.Width, (decimal)service.Height, (decimal)service.Length, 0, service.Id + i.ToString()));
                            }
                            if (service.Type == (int)ServiceType.Phu_kien) totalPrice += service.Price;
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(model.Note))
                        totalPrice += serviceDeliveryFee * Math.Ceiling(Convert.ToDecimal(model.Note.Split(' ')[0]));

                    if (totalPrice < model.TotalPrice) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Tổng tiền lỗi");
                    if (model.AdvanceMoney == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Tiền cọc không được trống");
                    if (model.AdvanceMoney != (totalPrice * 50 / 100)) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Tiền cọc không đúng");
                    var checkResult = await CheckStorageAvailable(spaceType, isMany, (int)model.TypeOrder, (DateTime)model.DeliveryDate, (DateTime)model.ReturnDate, cuboid, model.StorageId, (bool)model.IsCustomerDelivery, null, accessToken, new List<string> { model.DeliveryTime }, false);
                    if (!checkResult) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Kho không còn chỗ chứa");


                    request = _mapper.Map<Request>(model);
                    request.CreatedBy = userId;
                    if (role == "Office Staff") request.StorageId = storageId;
                    await CreateAsync(request);

                    await _requestTimelineService.CreateAsync(new RequestTimeline
                    {
                        RequestId = request.Id,
                        CreatedDate = DateTime.Now,
                        CreatedBy = userId,
                        Datetime = DateTime.Now,
                        Name = "Yêu cầu tạo đơn đã tạo thành công"
                    });

                    //await _firebaseService.PushOrderNoti("Yêu cầu moi đã den!", null, request.Id);
                    return await GetById(request.Id, accessToken);
                }


                if (role == "Delivery Staff" && model.Type == (int)RequestType.Huy_lich_giao_hang) // huy lich giao hang
                {
                    var schedules = _scheduleService.Get(x => x.ScheduleDay.Date == model.CancelDay.Value.Date && x.IsActive == true && x.StaffId == userId).Include(a => a.Staff).Include(x => x.Request).ToList();
                    if (schedules.Count < 1) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Schedule not found");
                    foreach (var schedule in schedules)
                    {
                        request = _mapper.Map<Request>(model);
                        request.OrderId = schedule.Request.OrderId;
                        request.CreatedBy = userId;
                        request.Status = 1;
                        request.Type = (int)RequestType.Huy_lich_giao_hang;
                        request.CancelDate = model.CancelDay;
                        await CreateAsync(request);

                        var oldRequest = schedule.Request;
                        oldRequest.Status = 6;

                        schedule.IsActive = false;
                        schedule.Request = oldRequest;
                        await _scheduleService.UpdateAsync(schedule);
                    }
                    var user = schedules.Select(x => x.Staff).Where(x => x.Id == userId).FirstOrDefault();

                    await _firebaseService.PushCancelRequestNoti("Delivery staff " + user.Name + " canceled schedule on " + model.CancelDay, user.Id);

                    return await GetById(request.Id, accessToken);
                }


                // check if customer has request of the order that are not completed
                request = Get(request => request.IsActive && request.CreatedBy == userId && (request.Status == (int)RequestStatus.Dang_xu_ly || request.Status == (int)RequestStatus.Dang_van_chuyen) && request.OrderId == model.OrderId).FirstOrDefault();
                if (request != null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Còn yêu cầu đang được xử lý");


                if (model.Type == (int)RequestType.Gia_han_don) // gia han don
                {
                    var orderToExtend = Get(request => request.OrderId == model.OrderId)
                        .Include(request => request.Order).ThenInclude(order => order.OrderDetails).ThenInclude(orderDetail => orderDetail.OrderDetailServiceMaps).Select(request => request.Order).FirstOrDefault();
                    if (orderToExtend == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Không tìm thấy đơn cần gia hạn");

                    if (model.IsPaid == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Chưa thanh toán");
                    if (model.IsPaid == false) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Chưa thanh toán");
                    var days = (model.ReturnDate.Value.Date.Subtract(model.OldReturnDate.Value.Date).Days);
                    if (days < 1) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Phải gia hạn thêm ít nhất 1 ngày");
                    var orderDetails = orderToExtend.OrderDetails.ToList();
                    int typeService = 1;
                    int spaceType = 0;
                    if (orderToExtend.Type == (int)OrderType.Kho_tu_quan) spaceType = 1;
                    bool isMany = false;
                    bool isMainService = false;

                    decimal totalPrice = 0;
                    decimal month = Math.Ceiling((decimal)model.ReturnDate.Value.Date.Subtract(model.OldReturnDate.Value.Date).Days / 30);
                    if (orderToExtend.Type == (int)OrderType.Kho_tu_quan)
                        month = ((model.ReturnDate.Value.Year - model.OldReturnDate.Value.Year) * 12) + model.ReturnDate.Value.Month - model.OldReturnDate.Value.Month;

                    // service list chứa list service người dùng đặt
                    List<Cuboid> cuboid = new List<Cuboid>();

                    // check order detail customer order
                    foreach (var orderDetail in orderDetails)
                    {
                        var servicesIds = orderDetail.OrderDetailServiceMaps.Select(service => new { ServiceId = service.ServiceId, Amount = service.Amount });

                        foreach (var serviceId in servicesIds)
                        {
                            var service = await _serviceService.GetById((Guid)serviceId.ServiceId);
                            if (service.Type == (int)ServiceType.Gui_theo_dien_tich) isMany = true;
                            if (service.Type != (int)ServiceType.Phu_kien)
                            {
                                isMainService = true;
                                totalPrice += (decimal)service.Price * month * (int)serviceId.Amount;
                                if (typeService == 1) typeService = (int)service.Type;
                                if (typeService != 1 && typeService != service.Type) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Không thể đặt 1 đơn 2 loại dịch vụ chính");
                            }

                        }
                        if (!((decimal)orderDetail.Height == 0 && (decimal)orderDetail.Width == 0 && (decimal)orderDetail.Length == 0))
                            cuboid.Add(new Cuboid((decimal)orderDetail.Width, (decimal)orderDetail.Height, (decimal)orderDetail.Length, 0, Guid.NewGuid()));
                    }

                    if (!isMainService) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Chưa chọn dịch vụ chính");
                    if (totalPrice != model.TotalPrice) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Tổng tiền lỗi");
                    var checkResult = await CheckStorageAvailable(spaceType, isMany, (int)orderToExtend.Type, (DateTime)model.ReturnDate, (DateTime)model.OldReturnDate, cuboid, orderToExtend.StorageId, false, null, accessToken, new List<string> { model.DeliveryTime }, true);
                    if (!checkResult) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Kho không còn chỗ chứa");


                    request = _mapper.Map<Request>(model);
                    request.CreatedBy = userId;
                    request.Status = (int)RequestStatus.Hoan_thanh;

                    await CreateAsync(request);

                    order = request.Order;
                    await _orderTimelineService.CreateAsync(new OrderTimeline
                    {
                        OrderId = order.Id,
                        CreatedDate = DateTime.Now,
                        CreatedBy = userId,
                        Datetime = DateTime.Now,
                        Name = "Đơn gia hạn thành công hạn trả về " + request.ReturnDate
                    });

                    OrderHistoryExtension orderExtend = new OrderHistoryExtension();
                    orderExtend.OldReturnDate = (DateTime)request.Order.ReturnDate;
                    orderExtend.PaidDate = DateTime.Now;
                    orderExtend.TotalPrice = (decimal)request.TotalPrice;
                    orderExtend.ModifiedBy = userId;
                    orderExtend.RequestId = request.Id;
                    orderExtend.OrderId = order.Id;
                    orderExtend.ReturnDate = (DateTime)request.ReturnDate;
                    await _orderHistoryExtensionService.CreateAsync(orderExtend);

                    List<OrderHistoryExtensionServiceMap> orderExtendService = new List<OrderHistoryExtensionServiceMap>();
                    orderDetails = request.Order.OrderDetails.ToList();
                    foreach (var orderDetail in orderDetails)
                    {
                        foreach (var service in orderDetail.OrderDetailServiceMaps.ToList())
                        {
                            var serviceExtend = new OrderHistoryExtensionServiceMap()
                            {
                                Amount = service.Amount,
                                Price = service.Service.Price,
                                Serviceid = service.ServiceId,
                                OrderHistoryExtensionId = orderExtend.Id
                            };
                            orderExtendService.Add(serviceExtend);
                        }
                    }
                    if (orderExtendService.Count > 0)
                    {
                        orderExtend.OrderHistoryExtensionServiceMaps = orderExtendService;
                        order.ReturnDate = orderExtend.ReturnDate;
                        orderExtend.Order = order;
                        await _orderHistoryExtensionService.UpdateAsync(orderExtend);
                    }

                    order.ReturnDate = model.ReturnDate;
                    order.IsPaid = (bool)model.IsPaid;
                    order.ModifiedBy = userId;
                    order.ModifiedDate = DateTime.Now;
                    orderExtend.Order = order;
                    await _orderHistoryExtensionService.UpdateAsync(orderExtend);


                    var requestTmp = Get(x => x.Id == request.Id).Include(x => x.Order).ThenInclude(order => order.Storage).ThenInclude(storage => storage.StaffAssignStorages.Where(staffAssign => staffAssign.Staff.Role.Name == "Manager" && staffAssign.IsActive)).ThenInclude(staffAssignInStorage => staffAssignInStorage.Staff).Select(x => x.Order.Storage.StaffAssignStorages.FirstOrDefault()).FirstOrDefault();

                    var managerToNoti = _staffAssignStoragesService.Get(staffAssign => staffAssign.IsActive && staffAssign.Staff.Role.Name == "Manager" && staffAssign.StorageId == orderToExtend.StorageId).Include(staffAssign => staffAssign.Staff).FirstOrDefault();

                    var customer = await _accountService.Get(account => account.Id == userId).FirstOrDefaultAsync();

                    await _firebaseService.SendNoti("Khách " + customer.Name + " gia hạn đơn: " + order.Name, userId, managerToNoti.Staff.DeviceTokenId, request.Id, new
                    {
                        Content = "Khách " + userId + " gia hạn đơn: " + order.Name,
                        model.OrderId,
                        RequestId = request.Id
                    });
                    return await GetById(request.Id, accessToken);
                }

                if (model.Type == (int)RequestType.Tra_don) // rut do ve
                {
                    if (model.IsCustomerDelivery != null)
                    {
                        if (!(bool)model.IsCustomerDelivery)
                        {
                            var oldRequest = Get(request => request.OrderId == model.OrderId).FirstOrDefault();
                            var staffs = await _accountService.GetStaff(oldRequest.StorageId, accessToken, new List<string> { "Delivery Staff" }, model.DeliveryDate, new List<string> { model.DeliveryTime }, false);
                            if (staffs.Count < 1) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Không còn đủ nhân viên vào thời điểm trả đơn");
                        }
                    }



                    request = _mapper.Map<Request>(model);
                    request.CreatedBy = userId;
                    request.Status = 1;
                    await CreateAsync(request);
                    var requestCreated = Get(x => x.Id == request.Id).Include(request => request.Order).FirstOrDefault();

                    await _requestTimelineService.CreateAsync(new RequestTimeline
                    {
                        RequestId = request.Id,
                        CreatedDate = DateTime.Now,
                        CreatedBy = userId,
                        Datetime = DateTime.Now,
                        Name = "Yêu cầu rút đồ về chờ xác nhận"
                    });


                    newRequest = Get(x => x.Id == request.Id && x.IsActive == true).Include(request => request.Order)
                        .ThenInclude(order => order.Storage).ThenInclude(storage => storage.StaffAssignStorages.Where(staffAssign => staffAssign.Staff.Role.Name == "Manager" && staffAssign.IsActive)).ThenInclude(taffAssignStorage => taffAssignStorage.Staff).ThenInclude(staff => staff.Role).FirstOrDefault();

                    if (newRequest.Order == null)
                        throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Order not found");
                    if (newRequest.Order.Storage == null)
                        throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Order not assigned yet");

                    var staffAssignInStorage = newRequest.Order.Storage.StaffAssignStorages.Where(x => x.Staff.Role.Name == "Manager" && x.IsActive).FirstOrDefault();
                    if (staffAssignInStorage == null) return await GetById(request.Id, accessToken);
                    await _firebaseService.SendNoti("Customer " + userId + " take back the order: " + model.OrderId, userId, staffAssignInStorage.Staff.DeviceTokenId, request.Id, new
                    {
                        Content = "Customer " + userId + " take back the order: " + model.OrderId,
                        model.OrderId,
                        RequestId = request.Id
                    });
                    return await GetById(request.Id, accessToken);
                }



                // customer huy don

                request = _mapper.Map<Request>(model);
                request.CreatedBy = userId;
                await CreateAsync(request);
                newRequest = Get(x => x.Id == request.Id && x.IsActive == true).Include(request => request.Order)
                        .Include(order => order.Storage).ThenInclude(storage => storage.StaffAssignStorages.Where(staff => staff.Staff.Role.Name == "Manager" && staff.IsActive)).ThenInclude(taffAssignStorage => taffAssignStorage.Staff).FirstOrDefault();
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
                    model.OrderId,
                    RequestId = request.Id
                });

                return await GetById(request.Id, accessToken);
            }
            catch (ErrorResponse e)
            {
                throw new ErrorResponse((int)e.Error.Code, e.Error.Message);
            }
            catch (Exception ex)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<RequestUpdateViewModel> Update(Guid id, RequestUpdateViewModel model, string accessToken)
        {
            try
            {
                var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
                var userId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);

                if (id != model.Id) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Request Id not matched");

                var request = await Get(x => x.Id == id && x.IsActive == true)
                    .Include(x => x.Order).ThenInclude(order => order.OrderDetails).ThenInclude(orderDetail => orderDetail.OrderDetailServiceMaps)
                    .Include(x => x.Order).ThenInclude(order => order.OrderHistoryExtensions).FirstOrDefaultAsync();
                if (request == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Request not found");

                request.Status = model.Status;




                if (model.IsPaid == null)
                {
                    await UpdateAsync(request);

                    string name = null;
                    if (request.Type == (int)RequestType.Tao_don) name = "Nhân viên đang tới lấy hàng";
                    if (request.Type == (int)RequestType.Tra_don) name = "Nhân viên đang tới trả hàng";
                    if (model.Description.Length > 0) name = model.Description;
                    await _requestTimelineService.CreateAsync(new RequestTimeline
                    {
                        RequestId = request.Id,
                        CreatedDate = DateTime.Now,
                        CreatedBy = userId,
                        Datetime = DateTime.Now,
                        Name = name
                    });

                }
                else
                {
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
                    order.IsPaid = (bool)model.IsPaid;
                    order.ModifiedBy = userId;
                    order.ModifiedDate = DateTime.Now;
                    orderHistoryExtend.Order = order;
                    await _orderHistoryExtensionService.UpdateAsync(orderHistoryExtend);

                }
                return model;
            }
            catch (ErrorResponse e)
            {
                throw new ErrorResponse((int)e.Error.Code, e.Error.Message);
            }
            catch (Exception ex)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, ex.Message);
            }

        }

        public async Task<RequestByIdViewModel> AssignStorage(RequestAssignStorageViewModel model, string accessToken)
        {
            try
            {
                var request = await Get(request => request.Id == model.RequestId && request.IsActive)
                                        .Include(request => request.RequestDetails)
                                        .ThenInclude(requestDetail => requestDetail.Service).FirstOrDefaultAsync();
                if (request == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Request not found");



                // check xem yêu cầu tạo đơn giữ đồ thuê hay kho tự quản => dẫn tới cần sử dụng space gì
                int spaceType = 0;
                if (request.TypeOrder == (int)OrderType.Kho_tu_quan) spaceType = 1;

                // check xem là giữ ít hay nhiều 
                bool isMany = false;

                // service list chứa list service người dùng đặt
                List<Cuboid> cuboid = new List<Cuboid>();
                var services = request.RequestDetails.Select(requestDetail => new
                {
                    ServiceId = requestDetail.ServiceId,
                    Amount = requestDetail.Amount
                }).ToList();
                for (int i = 0; i < services.Count; i++)
                {
                    for (int j = 0; j < services[i].Amount; j++)
                    {
                        var service = _serviceService.Get(service => service.Id == services[i].ServiceId).FirstOrDefault();
                        if (service.Type == (int)ServiceType.Gui_theo_dien_tich) isMany = true;
                        if (service.Type != (int)ServiceType.Phu_kien)
                        {
                            cuboid.Add(new Cuboid((decimal)service.Width, (decimal)service.Height, (decimal)service.Length, 0, service.Id + i.ToString()));
                        }
                    }
                }
                if (request.DeliveryTime != null)
                {
                    var checkResult = await CheckStorageAvailable(spaceType, isMany, (int)request.TypeOrder, (DateTime)request.DeliveryDate, (DateTime)request.ReturnDate, cuboid, model.StorageId, (bool)request.IsCustomerDelivery, request.Id, accessToken, new List<string> { _utilService.TimeToString((TimeSpan)request.DeliveryTime) }, false);
                    if (!checkResult) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Kho không còn chỗ chứa");
                }
                else
                {
                    var checkResult = await CheckStorageAvailable(spaceType, isMany, (int)request.TypeOrder, (DateTime)request.DeliveryDate, (DateTime)request.ReturnDate, cuboid, model.StorageId, (bool)request.IsCustomerDelivery, request.Id, accessToken, new List<string>(), false);
                    if (!checkResult) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Kho không còn chỗ chứa");
                }




                var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
                var userId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);



                request.StorageId = model.StorageId;
                request.ModifiedBy = userId;
                request.Status = 2;
                await UpdateAsync(request);

                string name = null;
                if (request.Type == (int)RequestType.Tao_don) name = "Yêu cầu tạo đơn đã xử lý";
                if (request.Type == (int)RequestType.Gia_han_don) name = "Yêu cầu gia hạn đơn đã được xử lý";
                if (request.Type == (int)RequestType.Tra_don) name = "Yêu cầu rút đồ về đă được xử lý";
                await _requestTimelineService.CreateAsync(new RequestTimeline
                {
                    RequestId = request.Id,
                    CreatedDate = DateTime.Now,
                    CreatedBy = userId,
                    Datetime = DateTime.Now,
                    Name = name
                });

                return _mapper.Map<RequestByIdViewModel>(request);
            }
            catch (ErrorResponse e)
            {
                throw new ErrorResponse((int)e.Error.Code, e.Error.Message);
            }
            catch (Exception e)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, e.Message);
            }
        }

        public async Task<RequestByIdViewModel> Cancel(Guid id, RequestCancelViewModel model, string accessToken)
        {
            try
            {
                if (id != model.Id) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Id không khớp");

                var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
                var userId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);
                var account = _accountService.Get(account => account.Id == userId && account.IsActive).FirstOrDefault();
                if (account == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Không tìm thấy tài khoản");
                var entity = await Get(x => x.Id == id && x.IsActive && x.Status != 0).Include(entity => entity.Schedules).FirstOrDefaultAsync();
                if (entity == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Không tìm thấy yêu cầu");

                var schedules = entity.Schedules;
                if (schedules != null)
                    if (schedules.Count > 0)
                        foreach (var schedule in schedules)
                            schedule.IsActive = false;
                entity.Schedules = schedules;
                entity.ModifiedBy = userId;
                entity.Status = 0;
                entity.CancelReason = model.CancelReason;
                await UpdateAsync(entity);

                return await GetById(id, accessToken);
            }
            catch (ErrorResponse e)
            {
                throw new ErrorResponse((int)e.Error.Code, e.Error.Message);
            }
            catch (Exception e)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, e.Message);
            }

        }

        public async Task<RequestByIdViewModel> DeliverRequest(Guid id, string accessToken)
        {
            try
            {
                var request = await Get(x => x.Id == id && x.IsActive).Include(x => x.CreatedByNavigation).FirstOrDefaultAsync();
                if (request == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Order not found");
                request.Status = (int)RequestStatus.Dang_van_chuyen;

                await UpdateAsync(request);
                var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
                var userId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);

                string name = "đến kho";
                string notiName = "lấy hàng về kho";
                if (request.Type == 4)
                {
                    name = "đến khách hàng";
                    notiName = "trả hàng";
                }

                await _requestTimelineService.CreateAsync(new RequestTimeline
                {
                    RequestId = request.Id,
                    CreatedDate = DateTime.Now,
                    CreatedBy = userId,
                    Datetime = DateTime.Now,
                    Name = "Đơn đang vận chuyển " + name
                });
                var customer = request.CreatedByNavigation;

                await _firebaseService.SendNoti("Nhân viên đang di chuyển đến để " + notiName, customer.Id, customer.DeviceTokenId, request.Id, new
                {
                    Content = "Nhân viên đang di chuyển đến để " + notiName,
                    request.OrderId,
                    RequestId = request.Id
                });

                return await GetById(id, accessToken);
            }
            catch (ErrorResponse e)
            {
                throw new ErrorResponse((int)e.Error.Code, e.Error.Message);
            }
            catch (Exception e)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, e.Message);
            }

        }

        public async Task<RequestByIdViewModel> DeliverySendRequestNotification(Guid id, NotificationDeliverySendRequestNotiViewModel model, string accessToken)
        {
            try
            {
                var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
                var userId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);

                var request = await Get(x => x.Id == id && x.IsActive).Include(x => x.Order).ThenInclude(order => order.Storage).ThenInclude(storage => storage.StaffAssignStorages).ThenInclude(staffAssign => staffAssign.Staff)
                    .Include(x => x.Storage).ThenInclude(storage => storage.StaffAssignStorages).ThenInclude(staffAssign => staffAssign.Staff).FirstOrDefaultAsync();
                if (request == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Order not found");

                var storage = request.Storage;
                if (storage == null)
                {
                    if (request.Order == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Order not found");
                    if (request.Order.Storage == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Storage not found");
                    storage = request.Order.Storage;
                }

                var staffList = storage.StaffAssignStorages.Where(x => x.Staff.Role.Name != "Delivery Staff" && x.IsActive).Select(x => x.Staff).ToList();

                foreach (var staff in staffList)
                {
                    if (staff.DeviceTokenId != null)
                    {
                        await _firebaseService.SendNoti(model.Message, userId, staff.DeviceTokenId, request.Id, new
                        {
                            Content = model.Message,
                            request.OrderId,
                            RequestId = request.Id
                        });
                    }
                }
                return await GetById(id, accessToken);
            }
            catch (ErrorResponse e)
            {
                throw new ErrorResponse((int)e.Error.Code, e.Error.Message);
            }
            catch (Exception e)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, e.Message);
            }

        }

        public async Task<bool> CheckStorageAvailable(int spaceType, bool isMany, int orderType, DateTime dateFrom, DateTime dateTo, List<Cuboid> cuboids, Guid? storageId, bool isCustomerDelivery, Guid? requestId, string accessToken, List<string> deliveryTimes, bool isCreateOrder)
        {
            // spaceType để check là kho tự quản hay giữ đồ thuê
            // isMany để check là giữ ít hay giữ nhiều
            // serviceList để chứa service người dùng đặt
            bool result = false;
            var requestsAssignStorage = await Get(request => request.IsActive && request.TypeOrder == orderType && (request.Type == (int)RequestType.Tao_don || request.Type == (int)RequestType.Tra_don || request.Type == (int)RequestType.Gia_han_don) && (request.Status == (int)RequestStatus.Da_xu_ly || request.Status == (int)RequestStatus.Dang_van_chuyen || request.Status == (int)RequestStatus.Dang_xu_ly))
                                           .Include(request => request.Order).ThenInclude(order => order.OrderDetails)
                                           .Include(request => request.RequestDetails).ThenInclude(requestDetail => requestDetail.Service).ToListAsync();
            if (requestId != null) requestsAssignStorage = requestsAssignStorage.Where(request => request.Id != requestId).ToList();
            requestsAssignStorage = requestsAssignStorage.Where(request => (request.DeliveryDate <= dateFrom && request.ReturnDate >= dateFrom) || (dateFrom <= request.DeliveryDate && dateTo >= request.DeliveryDate)).ToList();

            // Check xem storage còn đủ chỗ không
            result = await _storageService.CheckStorageAvailable(storageId, spaceType, dateFrom, dateTo, isMany, cuboids, requestsAssignStorage, isCustomerDelivery, accessToken, deliveryTimes, isCreateOrder);
            return result;
        }

        public async Task<List<StorageViewModel>> GetStorageAvailable(RequestCreateViewModel model, string accessToken)
        {
            try
            {
                List<StorageViewModel> result = new List<StorageViewModel>();
                // check xem yêu cầu tạo đơn giữ đồ thuê hay kho tự quản => dẫn tới cần sử dụng space gì
                int spaceType = (int)SpaceType.Ke;
                // check xem là giữ ít hay nhiều 
                bool isMany = false;
                decimal maxServiceDeliveryFee = 0;
                List<Cuboid> cuboid = new List<Cuboid>();


                var services = model.RequestDetails.Select(requestDetail => new
                {
                    ServiceId = requestDetail.ServiceId,
                    Amount = requestDetail.Amount
                }).ToList();
                if (services.Count == 0) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Chưa đặt bất kì sản phẩm nào.");
                // service list chứa list service người dùng đặt

                for (int i = 0; i < services.Count; i++)
                {
                    for (int j = 0; j < services[i].Amount; j++)
                    {
                        var service = _serviceService.Get(service => service.Id == services[i].ServiceId && service.IsActive).FirstOrDefault();
                        if (service == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Không tìm thấy dịch vụ muốn đặt");
                        if (service.Type == (int)ServiceType.Gui_theo_dien_tich) isMany = true;
                        if (service.Type != (int)ServiceType.Phu_kien)
                        {
                            if (maxServiceDeliveryFee < service.DeliveryFee) maxServiceDeliveryFee = (decimal)service.DeliveryFee;
                            cuboid.Add(new Cuboid(service.Width, service.Height, service.Length, 0, service.Id + i.ToString()));
                        }

                        if (service.Type == (int)ServiceType.Kho) spaceType = (int)SpaceType.Dien_tich;
                    }
                }

                if (model.Type == (int)RequestType.Tra_don)
                {
                    var request = Get(request => request.OrderId == model.OrderId)
                        .Include(request => request.Order).ThenInclude(order => order.OrderDetails).ThenInclude(orderDetail => orderDetail.OrderDetailServiceMaps)
                        .Include(request => request.Order).ThenInclude(order => order.Storage).FirstOrDefault();
                    if (request == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Không tìm thấy đơn cần rút đồ về");

                    var storage = request.Order.Storage;
                    var customerGeo = await _storageService.GetGeometry(model.ReturnAddress);
                    string customerAddress = customerGeo.results.FirstOrDefault().geometry.location.lat + "," + customerGeo.results.FirstOrDefault().geometry.location.lng;
                    string storageAddress = storage.Lat + "," + storage.Lng;
                    var storageReturn = _storageService.Get(storage => storage.Id == storage.Id).ProjectTo<StorageViewModel>(_mapper.ConfigurationProvider).FirstOrDefault();
                    var distance = await _storageService.GetDistanceFromCustomerToStorage(customerAddress, storageAddress);
                    storageReturn.DeliveryDistance = distance.rows[0].elements[0].distance.text;
                    if (!storageReturn.DeliveryDistance.Contains('k'))
                        storageReturn.DeliveryFee = maxServiceDeliveryFee * 1;
                    else
                        storageReturn.DeliveryFee = maxServiceDeliveryFee * Math.Ceiling(Convert.ToDecimal(storageReturn.DeliveryDistance.Split(' ')[0]));
                    result.Add(storageReturn);
                    return result;
                }

                var requestsAssignStorage = await Get(request => request.IsActive && (request.Type == (int)RequestType.Tao_don || request.Type == (int)RequestType.Tra_don || request.Type == (int)RequestType.Gia_han_don) && (request.Status == (int)RequestStatus.Da_xu_ly || request.Status == (int)RequestStatus.Dang_van_chuyen || request.Status == (int)RequestStatus.Dang_xu_ly))
                                           .Include(request => request.Order).ThenInclude(order => order.OrderDetails)
                                           .Include(request => request.RequestDetails).ThenInclude(requestDetail => requestDetail.Service).ToListAsync();
                requestsAssignStorage = requestsAssignStorage.Where(request => (request.DeliveryDate <= model.DeliveryDate && request.ReturnDate >= model.DeliveryDate) || (model.DeliveryDate <= request.DeliveryDate && model.ReturnDate >= request.DeliveryDate)).ToList();



                result = await _storageService.GetStorageAvailable(null, spaceType, (DateTime)model.DeliveryDate, (DateTime)model.ReturnDate, isMany, cuboid, requestsAssignStorage, (bool)model.IsCustomerDelivery, accessToken, new List<string> { model.DeliveryTime }, model.DeliveryAddress, maxServiceDeliveryFee);


                if (result.Count == 0) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Không có kho còn chỗ trống.");
                return result;
            }
            catch (ErrorResponse e)
            {
                throw new ErrorResponse((int)e.Error.Code, e.Error.Message);
            }
            catch (Exception ex)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }


    }
}
