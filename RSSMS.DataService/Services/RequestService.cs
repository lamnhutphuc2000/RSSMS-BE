using _3DBinPacking.Enum;
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
using RSSMS.DataService.ViewModels.Floors;
using RSSMS.DataService.ViewModels.OrderDetails;
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
        Task<RequestByIdViewModel> GetById(Guid id, string accessToken);
        Task<RequestCreateViewModel> Create(RequestCreateViewModel model, string accessToken);
        Task<RequestUpdateViewModel> Update(Guid id, RequestUpdateViewModel model, string accessToken);
        Task<RequestViewModel> Delete(Guid id);
        Task<RequestByIdViewModel> AssignStorage(RequestAssignStorageViewModel model, string accessToken);
        Task<RequestByIdViewModel> Cancel(Guid id, RequestCancelViewModel model, string accessToken);
        Task<RequestByIdViewModel> DeliverRequest(Guid id, string accessToken);
        Task<RequestByIdViewModel> DeliverySendRequestNotification(Guid id, string message, string accessToken);
    }


    public class RequestService : BaseService<Request>, IRequestService
    {
        private readonly IMapper _mapper;
        private readonly IScheduleService _scheduleService;
        private readonly IFirebaseService _firebaseService;
        private readonly IStaffAssignStorageService _staffAssignStoragesService;
        private readonly IOrderHistoryExtensionService _orderHistoryExtensionService;
        private readonly IOrderTimelineService _orderTimelineService;
        private readonly IAccountService _accountService;
        private readonly IStorageService _storageService;
        private readonly IServiceService _serviceService;
        public RequestService(IUnitOfWork unitOfWork, IRequestRepository repository, IMapper mapper
            , IScheduleService scheduleService
            , IFirebaseService firebaseService, IStaffAssignStorageService staffAssignStoragesService
            , IOrderHistoryExtensionService orderHistoryExtensionService
            , IOrderTimelineService orderTimelineService
            , IAccountService accountService
            , IStorageService storageService
            , IServiceService serviceService
            ) : base(unitOfWork, repository)
        {
            _mapper = mapper;
            _scheduleService = scheduleService;
            _firebaseService = firebaseService;
            _staffAssignStoragesService = staffAssignStoragesService;
            _orderHistoryExtensionService = orderHistoryExtensionService;
            _orderTimelineService = orderTimelineService;
            _accountService = accountService;
            _storageService = storageService;
            _serviceService = serviceService;
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

        public async Task<DynamicModelResponse<RequestViewModel>> GetAll(RequestViewModel model, IList<int> RequestTypes, string[] fields, int page, int size, string accessToken)
        {
            try
            {
                var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
                var userId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);
                var role = secureToken.Claims.First(claim => claim.Type.Contains("role")).Value;

                var requests = Get(request => request.IsActive)
                    .Include(request => request.Customer)
                    .Include(request => request.Schedules)
                    .Include(request => request.CreatedByNavigation).ThenInclude(createdBy => createdBy.StaffAssignStorages)
                    .Include(request => request.Storage)
                    .Include(request => request.Order).ThenInclude(order => order.Storage);
                if (model.FromDate != null && model.ToDate != null)
                {
                    requests = Get(request => request.IsActive && request.DeliveryDate.Value.Date >= model.FromDate.Value.Date && request.DeliveryDate <= model.ToDate.Value.Date)
                        .Include(request => request.Customer)
                        .Include(request => request.Schedules)
                        .Include(request => request.CreatedByNavigation).ThenInclude(createdBy => createdBy.StaffAssignStorages)
                        .Include(request => request.Storage)
                        .Include(request => request.Order).ThenInclude(order => order.Storage);
                }

                if (RequestTypes != null)
                {
                    if (RequestTypes.Count > 0)
                    {
                        requests = requests.Where(request => RequestTypes.Contains((int)request.Type))
                            .Include(request => request.Customer)
                            .Include(request => request.Schedules)
                            .Include(request => request.CreatedByNavigation).ThenInclude(createdBy => createdBy.StaffAssignStorages)
                            .Include(request => request.Storage)
                            .Include(request => request.Order).ThenInclude(order => order.Storage);
                    }
                }
                if (role == "Manager")
                {
                    var storageIds = _staffAssignStoragesService.Get(staffAssignStorage => staffAssignStorage.StaffId == userId).Select(a => a.StorageId).ToList();
                    var staff = _staffAssignStoragesService.Get(staffAssignStorage => storageIds.Contains(staffAssignStorage.StorageId)).Select(a => a.StaffId).ToList();
                    requests = requests.Where(request => staff.Contains((Guid)request.CreatedBy) || request.CreatedBy == userId || request.CreatedByNavigation.Role.Name == "Customer" || storageIds.Contains((Guid)request.StorageId))
                        .Include(request => request.Customer)
                        .Include(request => request.Schedules)
                        .Include(request => request.CreatedByNavigation).ThenInclude(createdBy => createdBy.StaffAssignStorages)
                        .Include(request => request.Storage)
                        .Include(request => request.Order).ThenInclude(order => order.Storage);
                }

                if (role == "Delivery Staff")
                {
                    requests = requests.Where(request => request.CreatedBy == userId)
                        .Include(request => request.Customer)
                        .Include(request => request.Schedules)
                        .Include(request => request.CreatedByNavigation).ThenInclude(createdBy => createdBy.StaffAssignStorages)
                        .Include(request => request.Storage)
                        .Include(request => request.Order).ThenInclude(order => order.Storage);
                }

                if (role == "Customer")
                {
                    requests = requests.Where(x => x.CreatedBy == userId || x.Customer.Id == userId)
                        .Include(request => request.Customer)
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

                if (role == "Delivery Staff")
                {
                    var storageId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "storage_id").Value);
                    request = request.Where(request => request.StorageId == storageId && request.Schedules.Where(schedule => schedule.IsActive && schedule.UserId == userId).Count() > 0)
                                    .Include(request => request.Schedules)
                                    .Include(request => request.Order)
                                    .Include(request => request.Storage)
                                    .Include(request => request.RequestDetails).ThenInclude(requestDetail => requestDetail.Service)
                                    .Include(request => request.CreatedByNavigation).ThenInclude(createdBy => createdBy.Role);
                }

                var result = await request.ProjectTo<RequestByIdViewModel>(_mapper.ConfigurationProvider).FirstOrDefaultAsync();
                if (result == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Request id not found");
                if (result.Type != 2) return result;
                var orderHistoryExtension = _orderHistoryExtensionService.Get(orderHistory => orderHistory.RequestId == result.Id).First();
                result.TotalPrice = orderHistoryExtension.TotalPrice;
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

        public async Task<RequestCreateViewModel> Create(RequestCreateViewModel model, string accessToken)
        {
            try
            {
                var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
                var userId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);
                var role = secureToken.Claims.First(claim => claim.Type.Contains("role")).Value;
                Guid? storageId = null;
                if (role == "Office Staff") storageId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "storage_id").Value);
                Request request = null;
                if (role == "Delivery Staff" && model.Type == (int)RequestType.Delivery_Cancel_Schedule) // huy lich giao hang
                {
                    var schedules = _scheduleService.Get(x => x.ScheduleDay.Date == model.CancelDay.Value.Date && x.IsActive == true && x.UserId == userId).Include(a => a.User).Include(x => x.Request).ToList();
                    if (schedules.Count < 1) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Schedule not found");
                    foreach (var schedule in schedules)
                    {
                        request = _mapper.Map<Request>(model);
                        request.OrderId = schedule.Request.OrderId;
                        request.CreatedBy = userId;
                        request.Status = 1;
                        request.Type = (int)RequestType.Delivery_Cancel_Schedule;
                        request.CancelDate = model.CancelDay;
                        await CreateAsync(request);

                        var oldRequest = schedule.Request;
                        oldRequest.Status = 6;

                        schedule.IsActive = false;
                        schedule.Request = oldRequest;
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

                    request = Get(x => x.Id == request.Id).Include(x => x.Order).ThenInclude(order => order.OrderDetails).ThenInclude(orderDetail => orderDetail.OrderDetailServiceMaps).FirstOrDefault();



                    int spaceType = 0;
                    bool isMany = false;
                    if (request.Order.Type == (int)OrderType.Kho_tu_quan) spaceType = 1;
                    if (request.Order.Type == (int)OrderType.Giu_do_thue) spaceType = 0;
                    var orderDetails = request.Order.OrderDetails;
                    foreach(var orderDetail in orderDetails)
                    {
                        var servicesIds = orderDetail.OrderDetailServiceMaps.Select(service => new { ServiceId = service.ServiceId, Amount = service.Amount });

                        foreach (var serviceId in servicesIds)
                        {
                            var service = await _serviceService.GetById((Guid)serviceId.ServiceId);
                            if (service.Type == (int)ServiceType.Gui_theo_dien_tich) isMany = true;
                        }
                    }


                    decimal serviceMaxHeight = 0;
                    decimal serviceMaxWidth = 0;
                    decimal serviceMaxLength = 0;

                    List<Cuboid> cuboids = new List<Cuboid>();
                    List<Guid> serviceList = new List<Guid>();



                    // Check xem storage còn đủ chỗ không
                    var floorInStorages = await _storageService.GetFloorWithStorage(request.Order.StorageId, spaceType, (DateTime)model.DeliveryDate, isMany);

                    bool flag = false;
                    if (floorInStorages == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Not enough space in storages");

                    foreach (var floorInStorage in floorInStorages)
                    {
                        int serviceNum = 0;
                        if (!flag)
                        {
                            // Lay kich thuoc cac service khach hang dat 
                            var orderDetailListTmp = request.Order.OrderDetails.Select(orderDetail => new
                            {
                                OrderDetailId = Guid.NewGuid(),
                                Height = (decimal)orderDetail.Height,
                                Width = (decimal)orderDetail.Width,
                                Length = (decimal)orderDetail.Length,
                            }).ToList();
                            for (int i = 1; i <= orderDetailListTmp.Count; i++)
                            {
                                if (orderDetailListTmp[i - 1].Width == 0 && orderDetailListTmp[i - 1].Height == 0 && orderDetailListTmp[i - 1].Length == 0)
                                {
                                    if (serviceMaxHeight < orderDetailListTmp[i - 1].Height) serviceMaxHeight = orderDetailListTmp[i - 1].Height;
                                    if (serviceMaxWidth < orderDetailListTmp[i - 1].Width) serviceMaxWidth = orderDetailListTmp[i - 1].Width;
                                    if (serviceMaxLength < orderDetailListTmp[i - 1].Length) serviceMaxLength = orderDetailListTmp[i - 1].Length;
                                    if (isMany) cuboids.Add(new Cuboid(orderDetailListTmp[i - 1].Width, 1, orderDetailListTmp[i - 1].Length, 0, orderDetailListTmp[i - 1].OrderDetailId));
                                    if (!isMany) cuboids.Add(new Cuboid(orderDetailListTmp[i - 1].Width, orderDetailListTmp[i - 1].Height, orderDetailListTmp[i - 1].Length, 0, orderDetailListTmp[i - 1].OrderDetailId));


                                    serviceList.Add(orderDetailListTmp[i - 1].OrderDetailId);
                                    serviceNum++;
                                }

                            }



                            var floors = floorInStorage.Value.ToList();
                            List<OrderDetailInFloorViewModel> orderDetailList = new List<OrderDetailInFloorViewModel>();
                            List<FloorGetByIdViewModel> floorList = new List<FloorGetByIdViewModel>();
                            foreach (var floor in floors)
                            {
                                if (floor.Height >= serviceMaxHeight && floor.Width >= serviceMaxWidth && floor.Length >= serviceMaxLength)
                                {
                                    floorList.Add(floor);
                                    orderDetailList.AddRange(floor.OrderDetails);
                                }
                            }
                            if (!(floorList.Count < serviceNum && model.Type == (int)OrderType.Kho_tu_quan))
                            {
                                foreach (var floorInList in floorList)
                                {
                                    if (!flag)
                                    {
                                        // get request đã được assign vào storage
                                        var requestsAssignStorage = await Get(requests => requests.IsActive && requests.TypeOrder == request.Type && requests.Type == (int)RequestType.Create_Order && requests.StorageId == floorInStorage.Key && requests.Type == (int)RequestType.Create_Order && (requests.Status == 2 || requests.Status == 3) && requests.Id != request.Id)
                                               .Include(requests => requests.Order).ThenInclude(order => order.OrderDetails)
                                               .Include(requests => requests.RequestDetails).ThenInclude(requestDetail => requestDetail.Service).ToListAsync();
                                        requestsAssignStorage = requestsAssignStorage.Where(requests => (requests.DeliveryDate <= model.DeliveryDate && requests.ReturnDate >= model.DeliveryDate) || (model.DeliveryDate <= requests.DeliveryDate && model.ReturnDate >= requests.DeliveryDate)).ToList();
                                        if (!(floorList.Count <= requestsAssignStorage.Count && model.Type == (int)OrderType.Kho_tu_quan))
                                        {
                                            foreach (var requestAssignStorage in requestsAssignStorage)
                                            {
                                                var servicesInRequestDetail = requestAssignStorage.RequestDetails.Select(requestDetail => new
                                                {
                                                    ServiceId = (Guid)requestDetail.ServiceId,
                                                    Height = (decimal?)null,
                                                    Width = (decimal?)null,
                                                    Length = (decimal?)null,
                                                    Amount = (int)requestDetail.Amount
                                                }).ToList();
                                                if (requestAssignStorage.Order != null)
                                                    servicesInRequestDetail = requestAssignStorage.Order.OrderDetails.Select(orderDetail => new
                                                    {
                                                        ServiceId = orderDetail.Id,
                                                        Height = orderDetail.Height,
                                                        Width = orderDetail.Width,
                                                        Length = orderDetail.Length,
                                                        Amount = 0
                                                    }).ToList();
                                                for (int i = 1; i <= servicesInRequestDetail.Count; i++)
                                                {
                                                    for (int j = 0; j < servicesInRequestDetail[i - 1].Amount; j++)
                                                    {
                                                        if (servicesInRequestDetail[i - 1].Amount != 0)
                                                        {
                                                            var service = _serviceService.Get(service => service.Id == servicesInRequestDetail[i - 1].ServiceId).FirstOrDefault();
                                                            if (service.Type != (int)ServiceType.Phu_kien)
                                                            {
                                                                if (serviceMaxHeight < service.Height) serviceMaxHeight = service.Height;
                                                                if (serviceMaxWidth < service.Width) serviceMaxWidth = service.Width;
                                                                if (serviceMaxLength < service.Length) serviceMaxLength = service.Length;
                                                                if (isMany)
                                                                    cuboids.Add(new Cuboid(service.Width, 1, service.Length, 0, service.Id));
                                                                if (!isMany)
                                                                    cuboids.Add(new Cuboid(service.Width, service.Height, service.Length, 0, service.Id));
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (isMany)
                                                                cuboids.Add(new Cuboid((decimal)servicesInRequestDetail[i - 1].Width, 1, (decimal)servicesInRequestDetail[i - 1].Length));
                                                            if (!isMany)
                                                                cuboids.Add(new Cuboid((decimal)servicesInRequestDetail[i - 1].Width, (decimal)servicesInRequestDetail[i - 1].Height, (decimal)servicesInRequestDetail[i - 1].Length));
                                                        }
                                                    }
                                                }
                                            }




                                            List<Cuboid> cuboidTmps = new List<Cuboid>();
                                            cuboidTmps.AddRange(cuboids);
                                            foreach (var orderDetail in orderDetailList)
                                            {
                                                if (isMany) cuboidTmps.Add(new Cuboid((decimal)orderDetail.Width, 1, (decimal)orderDetail.Length, 0, orderDetail.Id));
                                                if (!isMany) cuboidTmps.Add(new Cuboid((decimal)orderDetail.Width, (decimal)orderDetail.Height, (decimal)orderDetail.Length, 0, orderDetail.Id));
                                            }


                                            BinPackParameter parameter = null;
                                            if (isMany) parameter = new BinPackParameter(floorInList.Width, 1, floorInList.Length, cuboids);
                                            else parameter = new BinPackParameter(floorInList.Width, floorInList.Height, floorInList.Length, cuboids);

                                            var binPacker = BinPacker.GetDefault(BinPackerVerifyOption.BestOnly);
                                            var result = binPacker.Pack(parameter);
                                            if (result.BestResult.Count == 1)
                                            {
                                                flag = true;
                                            }
                                            else
                                            {
                                                foreach (var cuboid in result.BestResult.First())
                                                {
                                                    var orderDetail = orderDetailList.Where(orderDetail => orderDetail.Id == (Guid)cuboid.Tag).FirstOrDefault();
                                                    if (orderDetail != null) orderDetailList.Remove(orderDetail);
                                                    var service = serviceList.Where(service => service == (Guid)cuboid.Tag).FirstOrDefault();
                                                    if (service != null) serviceList.Remove(service);
                                                }
                                            }
                                        }

                                    }
                                }
                            }

                        }
                    }

                    if (!flag) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Not enough space in storages");





                    await _orderTimelineService.CreateAsync(new OrderTimeline
                    {
                        RequestId = request.Id,
                        CreatedDate = DateTime.Now,
                        CreatedBy = userId,
                        Datetime = DateTime.Now,
                        Name = "Yêu cầu gia hạn đơn chờ xác nhận"
                    });

                    var staffAssignInStorage = _orderHistoryExtensionService.Get(x => x.OrderId == model.OrderId).Include(x => x.Order).ThenInclude(order => order.Storage).ThenInclude(storage => storage.StaffAssignStorages.Where(staff => staff.RoleName == "Manager" && staff.IsActive == true)).ThenInclude(staffAssignInStorage => staffAssignInStorage.Staff).Select(x => x.Order.Storage.StaffAssignStorages.FirstOrDefault()).FirstOrDefault();

                    await _firebaseService.SendNoti("Customer " + userId + " expand the order: " + model.OrderId, userId, staffAssignInStorage.Staff.DeviceTokenId, request.Id, new
                    {
                        Content = "Customer " + userId + " expand the order: " + model.OrderId,
                        model.OrderId,
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
                    var requestCreated = Get(x => x.Id == request.Id).Include(request => request.Order).FirstOrDefault();

                    await _orderTimelineService.CreateAsync(new OrderTimeline
                    {
                        RequestId = request.Id,
                        CreatedDate = DateTime.Now,
                        CreatedBy = userId,
                        Datetime = DateTime.Now,
                        Name = "Yêu cầu rút đồ về chờ xác nhận"
                    });

                    var requestList = Get(x => x.DeliveryTime == requestCreated.DeliveryTime && x.DeliveryDate == requestCreated.DeliveryDate && request.Status == 2).ToList();
                    // check xem còn nhân viên trong storage nào không 
                    var deliveryStaffs = await _accountService.GetStaff(requestCreated.Order.StorageId, accessToken, new List<string> { "Delivery Staff" }, model.DeliveryDate, new List<string> { model.DeliveryTime }, false);
                    if (deliveryStaffs.Count - requestList.Count <= 0) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Don't have enough delivery staff");



                    newRequest = Get(x => x.Id == request.Id && x.IsActive == true).Include(request => request.Order)
                        .ThenInclude(order => order.Storage).ThenInclude(storage => storage.StaffAssignStorages.Where(staff => staff.RoleName == "Manager" && staff.IsActive == true)).ThenInclude(taffAssignStorage => taffAssignStorage.Staff).FirstOrDefault();
                    if (newRequest.Order == null)
                        throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Order not found");
                    if (newRequest.Order.Storage == null)
                        throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Order not assigned yet");

                    var staffAssignInStorage = newRequest.Order.Storage.StaffAssignStorages.Where(x => x.RoleName == "Manager" && x.IsActive == true).FirstOrDefault();
                    if (staffAssignInStorage == null) return model;
                    await _firebaseService.SendNoti("Customer " + userId + " take back the order: " + model.OrderId, userId, staffAssignInStorage.Staff.DeviceTokenId, request.Id, new
                    {
                        Content = "Customer " + userId + " take back the order: " + model.OrderId,
                        model.OrderId,
                        RequestId = request.Id
                    });
                    return model;
                }
                if (model.Type == (int)RequestType.Create_Order) // customer tao yeu cau tao don
                {
                    
                    int spaceType = 0;
                    bool isMany = false;
                    if (model.TypeOrder == (int)OrderType.Kho_tu_quan) spaceType = 1;
                    if (model.TypeOrder == (int)OrderType.Giu_do_thue) spaceType = 0;


                    // check xem còn nhân viên trong storage nào không 
                    var requestList = Get(x => x.DeliveryDate == model.DeliveryDate && x.DeliveryTime == model.DeliveryTime && x.Status == 2).ToList();
                    if (model.TypeOrder == (int)OrderType.Giu_do_thue && !(bool)model.IsCustomerDelivery)
                    {
                        var deliveryStaffs = await _accountService.GetStaff(null, accessToken, new List<string> { "Delivery Staff" }, model.DeliveryDate, new List<string> { model.DeliveryTime }, true);
                        if (deliveryStaffs.Count - requestList.Count <= 0) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Don't have enough delivery staff");

                    }

                    decimal serviceMaxHeight = 0;
                    decimal serviceMaxWidth = 0;
                    decimal serviceMaxLength = 0;
                    
                    List<Cuboid> cuboids = new List<Cuboid>();
                    List<Service> serviceList = new List<Service>();
                    
                    // Check xem storage còn đủ chỗ không
                    var floorInStorages = await _storageService.GetFloorWithStorage(null,spaceType,(DateTime)model.DeliveryDate, isMany);

                    bool flag = false;
                    bool deliFlag = true;
                    if (floorInStorages == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Not enough space in storages");

                    foreach (var floorInStorage in floorInStorages)
                    {
                        int serviceNum = 0;
                        if (!flag)
                        {
                            // Lay kich thuoc cac service khach hang dat 
                            var services = model.RequestDetails.Select(requestDetail => new
                            {
                                ServiceId = requestDetail.ServiceId,
                                Amount = requestDetail.Amount
                            }).ToList();
                            for (int i = 1; i <= services.Count; i++)
                            {
                                for (int j = 0; j < services[i - 1].Amount; j++)
                                {
                                    var service = _serviceService.Get(service => service.Id == services[i - 1].ServiceId).FirstOrDefault();
                                    if (service.Type != (int)ServiceType.Phu_kien)
                                    {
                                        if (service.Type == (int)ServiceType.Gui_theo_dien_tich) isMany = true;
                                        if (serviceMaxHeight < service.Height) serviceMaxHeight = service.Height;
                                        if (serviceMaxWidth < service.Width) serviceMaxWidth = service.Width;
                                        if (serviceMaxLength < service.Length) serviceMaxLength = service.Length;
                                        if(service.Type == (int)ServiceType.Gui_theo_dien_tich) cuboids.Add(new Cuboid(service.Width, 1, service.Length, 0, service.Id));
                                        if (service.Type != (int)ServiceType.Gui_theo_dien_tich) cuboids.Add(new Cuboid(service.Width, service.Height, service.Length, 0, service.Id));
                                        serviceList.Add(service);
                                        serviceNum++;
                                    }
                                }
                            }

                            var floors = floorInStorage.Value.ToList();
                            List<OrderDetailInFloorViewModel> orderDetailList = new List<OrderDetailInFloorViewModel>();
                            List<FloorGetByIdViewModel> floorList = new List<FloorGetByIdViewModel>();
                            foreach (var floor in floors)
                            {
                                if (floor.Height >= serviceMaxHeight && floor.Width >= serviceMaxWidth && floor.Length >= serviceMaxLength)
                                {
                                    floorList.Add(floor);
                                    orderDetailList.AddRange(floor.OrderDetails);
                                }
                            }
                            if(!(floorList.Count < serviceNum && model.TypeOrder == (int) OrderType.Kho_tu_quan))
                            {
                                foreach (var floorInList in floorList)
                                {
                                    if (!flag)
                                    {
                                        // get request đã được assign vào storage
                                        var requestsAssignStorage = await Get(request => request.IsActive && request.TypeOrder == model.TypeOrder && request.Type == (int)RequestType.Create_Order && request.StorageId == floorInStorage.Key && request.Type == (int)RequestType.Create_Order && (request.Status == 2 || request.Status == 3))
                                            .Include(request => request.Order).ThenInclude(order => order.OrderDetails)
                                            .Include(request => request.RequestDetails).ThenInclude(requestDetail => requestDetail.Service).ToListAsync();
                                        requestsAssignStorage = requestsAssignStorage.Where(request => (request.DeliveryDate <= model.DeliveryDate && request.ReturnDate >= model.DeliveryDate) || (model.DeliveryDate <= request.DeliveryDate && model.ReturnDate >= request.DeliveryDate)).ToList();
                                        if (!(floorList.Count <= requestsAssignStorage.Count && model.TypeOrder == (int)OrderType.Kho_tu_quan))
                                        {
                                            foreach (var requestAssignStorage in requestsAssignStorage)
                                            {
                                                var servicesInRequestDetail = requestAssignStorage.RequestDetails.Select(requestDetail => new
                                                {
                                                    ServiceId = (Guid)requestDetail.ServiceId,
                                                    Height = (decimal?)null,
                                                    Width = (decimal?)null,
                                                    Length = (decimal?)null,
                                                    Amount = (int)requestDetail.Amount
                                                }).ToList();
                                                if (requestAssignStorage.Order != null)
                                                    servicesInRequestDetail = requestAssignStorage.Order.OrderDetails.Select(orderDetail => new
                                                    {
                                                        ServiceId = orderDetail.Id,
                                                        Height = orderDetail.Height,
                                                        Width = orderDetail.Width,
                                                        Length = orderDetail.Length,
                                                        Amount = 0
                                                    }).ToList();
                                                for (int i = 1; i <= servicesInRequestDetail.Count; i++)
                                                {
                                                    for (int j = 0; j < servicesInRequestDetail[i - 1].Amount; j++)
                                                    {
                                                        if (servicesInRequestDetail[i - 1].Amount != 0)
                                                        {
                                                            var service = _serviceService.Get(service => service.Id == servicesInRequestDetail[i - 1].ServiceId).FirstOrDefault();
                                                            if (service.Type != (int)ServiceType.Phu_kien)
                                                            {
                                                                if (service.Type == (int)ServiceType.Gui_theo_dien_tich) isMany = true;
                                                                if (serviceMaxHeight < service.Height) serviceMaxHeight = service.Height;
                                                                if (serviceMaxWidth < service.Width) serviceMaxWidth = service.Width;
                                                                if (serviceMaxLength < service.Length) serviceMaxLength = service.Length;
                                                                
                                                                if(isMany) cuboids.Add(new Cuboid(service.Width, 1, service.Length, 0, service.Id));
                                                                else cuboids.Add(new Cuboid(service.Width, service.Height, service.Length, 0, service.Id));
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if(isMany) cuboids.Add(new Cuboid((decimal)servicesInRequestDetail[i - 1].Width, 1, (decimal)servicesInRequestDetail[i - 1].Length));
                                                            else cuboids.Add(new Cuboid((decimal)servicesInRequestDetail[i - 1].Width, (decimal)servicesInRequestDetail[i - 1].Height, (decimal)servicesInRequestDetail[i - 1].Length));

                                                        }
                                                    }
                                                }
                                            }




                                            List<Cuboid> cuboidTmps = new List<Cuboid>();
                                            cuboidTmps.AddRange(cuboids);
                                            foreach (var orderDetail in orderDetailList)
                                            {
                                                if(isMany) cuboidTmps.Add(new Cuboid((decimal)orderDetail.Width, 1, (decimal)orderDetail.Length, 0, orderDetail.Id));
                                                else cuboidTmps.Add(new Cuboid((decimal)orderDetail.Width, (decimal)orderDetail.Height, (decimal)orderDetail.Length, 0, orderDetail.Id));
                                            }

                                            BinPackParameter parameter = null;
                                            if(isMany) parameter = new BinPackParameter(floorInList.Width, 1, floorInList.Length, cuboids);
                                            else parameter = new BinPackParameter(floorInList.Width, floorInList.Height, floorInList.Length, cuboids);
                                            var binPacker = BinPacker.GetDefault(BinPackerVerifyOption.BestOnly);
                                            var result = binPacker.Pack(parameter);
                                            if (result.BestResult.Count == 1)
                                            {
                                                flag = true;
                                                if (model.TypeOrder == (int)OrderType.Giu_do_thue && !(bool)model.IsCustomerDelivery)
                                                {
                                                    requestList = Get(x => x.DeliveryDate == model.DeliveryDate && x.DeliveryTime == model.DeliveryTime && x.Status == 2 && x.StorageId == floorInStorage.Key).ToList();
                                                    var deliveryStaffs = await _accountService.GetStaff(floorInStorage.Key, accessToken, new List<string> { "Delivery Staff" }, model.DeliveryDate, new List<string> { model.DeliveryTime }, false);
                                                    if (deliveryStaffs.Count - requestList.Count <= 0) deliFlag = false;
                                                }
                                            }
                                            else
                                            {
                                                foreach (var cuboid in result.BestResult.First())
                                                {
                                                    var orderDetail = orderDetailList.Where(orderDetail => orderDetail.Id == (Guid)cuboid.Tag).FirstOrDefault();
                                                    if (orderDetail != null) orderDetailList.Remove(orderDetail);
                                                    var service = serviceList.Where(service => service.Id == (Guid)cuboid.Tag).FirstOrDefault();
                                                    if (service != null) serviceList.Remove(service);
                                                }
                                            }
                                        }
                                        
                                    }
                                }
                            }
                            
                        }
                    }

                    if(!deliFlag) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Don't have enough delivery staff");
                    if (!flag) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Not enough space in storages");

                    


                    request = _mapper.Map<Request>(model);
                    request.CreatedBy = userId;
                    if (role == "Customer") request.CustomerId = userId;
                    if (role == "Office Staff") request.StorageId = storageId;
                    await CreateAsync(request);

                    await _orderTimelineService.CreateAsync(new OrderTimeline
                    {
                        CreatedDate = DateTime.Now,
                        RequestId = request.Id,
                        CreatedBy = userId,
                        Datetime = DateTime.Now,
                        Name = "Yêu cầu tạo đơn chờ xác nhận"
                    });

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
                    model.OrderId,
                    RequestId = request.Id
                });

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

                if(request.Type == (int) RequestType.Extend_Order)
                {
                    OrderHistoryExtension orderExtend = _mapper.Map<OrderHistoryExtension>(model);
                    orderExtend.ModifiedBy = userId;
                    orderExtend.RequestId = request.Id;
                    orderExtend.OrderId = (Guid)request.OrderId;
                    await _orderHistoryExtensionService.CreateAsync(orderExtend);

                    List<OrderHistoryExtensionServiceMap> orderExtendService = new List<OrderHistoryExtensionServiceMap>();
                    var orderDetails = request.Order.OrderDetails.ToList();
                    foreach(var orderDetail in orderDetails)
                    {
                        foreach(var service in orderDetail.OrderDetailServiceMaps.ToList())
                        {
                            var serviceExtend = new OrderHistoryExtensionServiceMap()
                            {
                                Amount = service.Amount,
                                Price = (double)service.TotalPrice,
                                Serviceid = service.ServiceId,
                                OrderHistoryExtensionId = orderExtend.Id
                            };
                            orderExtendService.Add(serviceExtend);
                        }
                    }
                    if(orderExtendService.Count > 0)
                    {
                        orderExtend.OrderHistoryExtensionServiceMaps = orderExtendService;
                        await _orderHistoryExtensionService.UpdateAsync(orderExtend);
                    }
                }

                

                if (model.IsPaid == null)
                {
                    await UpdateAsync(request);

                    string name = null;
                    if (request.Type == (int)RequestType.Create_Order) name = "Nhân viên đang tới lấy hàng";
                    if (request.Type == (int)RequestType.Return_Order) name = "Nhân viên đang tới trả hàng";
                    if (model.Description.Length > 0) name = model.Description;
                    await _orderTimelineService.CreateAsync(new OrderTimeline
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

                // check xem còn nhân viên trong storage nào không 
                if (request.TypeOrder == 1 && (bool)request.IsCustomerDelivery)
                {
                    var deliveryStaffs = await _accountService.GetStaff(null, accessToken, new List<string> { "Delivery Staff" }, request.DeliveryDate, new List<string> { request.DeliveryTime }, true);
                    if (deliveryStaffs.Count == 0) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Don't have enough delivery staff");

                }



                int spaceType = 0;
                bool isMany = false;
                if (request.TypeOrder == (int)OrderType.Kho_tu_quan) spaceType = 1;
                if (request.TypeOrder == (int)OrderType.Giu_do_thue) spaceType = 0;


                // check xem còn nhân viên trong storage nào không 
                var requestList = Get(x => x.DeliveryDate == request.DeliveryDate && x.DeliveryTime == request.DeliveryTime && x.Status == 2).ToList();
                if (request.TypeOrder == (int)OrderType.Giu_do_thue && !(bool)request.IsCustomerDelivery)
                {
                    var deliveryStaffs = await _accountService.GetStaff(model.StorageId, accessToken, new List<string> { "Delivery Staff" }, request.DeliveryDate, new List<string> { request.DeliveryTime }, false);
                    if (deliveryStaffs.Count - requestList.Count <= 0) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Don't have enough delivery staff");

                }

                decimal serviceMaxHeight = 0;
                decimal serviceMaxWidth = 0;
                decimal serviceMaxLength = 0;
                
                List<Cuboid> cuboids = new List<Cuboid>();
                List<Service> serviceList = new List<Service>();
                


                // Check xem storage còn đủ chỗ không
                var floorInStorages = await _storageService.GetFloorWithStorage(model.StorageId, spaceType, (DateTime)request.DeliveryDate, isMany);

                bool flag = false;
                bool deliFlag = true;
                if (floorInStorages == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Not enough space in storages");

                foreach (var floorInStorage in floorInStorages)
                {
                    int serviceNum = 0;
                    if (!flag)
                    {
                        // Lay kich thuoc cac service khach hang dat 
                        var services = request.RequestDetails.Select(requestDetail => new
                        {
                            ServiceId = requestDetail.ServiceId,
                            Amount = requestDetail.Amount
                        }).ToList();
                        for (int i = 1; i <= services.Count; i++)
                        {
                            for (int j = 0; j < services[i - 1].Amount; j++)
                            {
                                var service = _serviceService.Get(service => service.Id == services[i - 1].ServiceId).FirstOrDefault();
                                if (service.Type != (int)ServiceType.Phu_kien)
                                {
                                    if (service.Type == (int)ServiceType.Gui_theo_dien_tich) isMany = true;
                                    if (serviceMaxHeight < service.Height) serviceMaxHeight = service.Height;
                                    if (serviceMaxWidth < service.Width) serviceMaxWidth = service.Width;
                                    if (serviceMaxLength < service.Length) serviceMaxLength = service.Length;
                                    if (service.Type == (int)ServiceType.Gui_theo_dien_tich) cuboids.Add(new Cuboid(service.Width, 1, service.Length, 0, service.Id));
                                    if (service.Type != (int)ServiceType.Gui_theo_dien_tich) cuboids.Add(new Cuboid(service.Width, service.Height, service.Length, 0, service.Id));
                                    serviceList.Add(service);
                                    serviceNum++;
                                }
                            }
                        }



                        var floors = floorInStorage.Value.ToList();
                        List<OrderDetailInFloorViewModel> orderDetailList = new List<OrderDetailInFloorViewModel>();
                        List<FloorGetByIdViewModel> floorList = new List<FloorGetByIdViewModel>();
                        foreach (var floor in floors)
                        {
                            if (floor.Height >= serviceMaxHeight && floor.Width >= serviceMaxWidth && floor.Length >= serviceMaxLength)
                            {
                                floorList.Add(floor);
                                orderDetailList.AddRange(floor.OrderDetails);
                            }
                        }

                        if (!(floorList.Count < serviceNum && request.TypeOrder == (int)OrderType.Kho_tu_quan))
                        {
                            foreach (var floorInList in floorList)
                            {
                                if (!flag)
                                {
                                    // get request đã được assign vào storage
                                    var requestsAssignStorage = await Get(requests => requests.IsActive && requests.TypeOrder == request.TypeOrder && requests.Type == (int)RequestType.Create_Order && requests.StorageId == floorInStorage.Key && requests.Type == (int)RequestType.Create_Order && (requests.Status == 2 || requests.Status == 3))
                                           .Include(requests => requests.Order).ThenInclude(order => order.OrderDetails)
                                           .Include(requests => requests.RequestDetails).ThenInclude(requestDetail => requestDetail.Service).ToListAsync();
                                    requestsAssignStorage = requestsAssignStorage.Where(requests => (requests.DeliveryDate <= request.DeliveryDate && requests.ReturnDate >= request.DeliveryDate) || (request.DeliveryDate <= requests.DeliveryDate && request.ReturnDate >= requests.DeliveryDate)).ToList();
                                    if (!(floorList.Count <= requestsAssignStorage.Count && request.TypeOrder == (int)OrderType.Kho_tu_quan))
                                    {
                                        foreach (var requestAssignStorage in requestsAssignStorage)
                                        {
                                            var servicesInRequestDetail = requestAssignStorage.RequestDetails.Select(requestDetail => new
                                            {
                                                ServiceId = (Guid)requestDetail.ServiceId,
                                                Height = (decimal?)null,
                                                Width = (decimal?)null,
                                                Length = (decimal?)null,
                                                Amount = (int)requestDetail.Amount
                                            }).ToList();
                                            if (requestAssignStorage.Order != null)
                                                servicesInRequestDetail = requestAssignStorage.Order.OrderDetails.Select(orderDetail => new
                                                {
                                                    ServiceId = orderDetail.Id,
                                                    Height = orderDetail.Height,
                                                    Width = orderDetail.Width,
                                                    Length = orderDetail.Length,
                                                    Amount = 0
                                                }).ToList();
                                            for (int i = 1; i <= servicesInRequestDetail.Count; i++)
                                            {
                                                for (int j = 0; j < servicesInRequestDetail[i - 1].Amount; j++)
                                                {
                                                    if (servicesInRequestDetail[i - 1].Amount != 0)
                                                    {
                                                        var service = _serviceService.Get(service => service.Id == servicesInRequestDetail[i - 1].ServiceId).FirstOrDefault();
                                                        if (service.Type != (int)ServiceType.Phu_kien)
                                                        {
                                                            if (service.Type == (int)ServiceType.Gui_theo_dien_tich) isMany = true;
                                                            if (serviceMaxHeight < service.Height) serviceMaxHeight = service.Height;
                                                            if (serviceMaxWidth < service.Width) serviceMaxWidth = service.Width;
                                                            if (serviceMaxLength < service.Length) serviceMaxLength = service.Length;
                                                            if (isMany) cuboids.Add(new Cuboid(service.Width, 1, service.Length, 0, service.Id));
                                                            if(!isMany) cuboids.Add(new Cuboid(service.Width, service.Height, service.Length, 0, service.Id));
                                                            
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (isMany)
                                                            cuboids.Add(new Cuboid((decimal)servicesInRequestDetail[i - 1].Width, 1, (decimal)servicesInRequestDetail[i - 1].Length));
                                                        if (!isMany)
                                                            cuboids.Add(new Cuboid((decimal)servicesInRequestDetail[i - 1].Width, (decimal)servicesInRequestDetail[i - 1].Height, (decimal)servicesInRequestDetail[i - 1].Length));
                                                    }
                                                }
                                            }
                                        }




                                        List<Cuboid> cuboidTmps = new List<Cuboid>();
                                        cuboidTmps.AddRange(cuboids);
                                        foreach (var orderDetail in orderDetailList)
                                        {
                                            if(isMany) cuboidTmps.Add(new Cuboid((decimal)orderDetail.Width, 1, (decimal)orderDetail.Length, 0, orderDetail.Id));
                                            else cuboidTmps.Add(new Cuboid((decimal)orderDetail.Width, (decimal)orderDetail.Height, (decimal)orderDetail.Length, 0, orderDetail.Id));
                                        }

                                        BinPackParameter parameter = null;
                                        if (isMany) parameter = new BinPackParameter(floorInList.Width, 1, floorInList.Length, cuboids);
                                        else parameter = new BinPackParameter(floorInList.Width, floorInList.Height, floorInList.Length, cuboids);

                                        var binPacker = BinPacker.GetDefault(BinPackerVerifyOption.BestOnly);
                                        var result = binPacker.Pack(parameter);
                                        if (result.BestResult.Count == 1)
                                        {
                                            flag = true;
                                            if (request.TypeOrder == (int)OrderType.Giu_do_thue && !(bool)request.IsCustomerDelivery)
                                            {
                                                requestList = Get(x => x.DeliveryDate == request.DeliveryDate && x.DeliveryTime == request.DeliveryTime && x.Status == 2 && x.StorageId == model.StorageId).ToList();
                                                var deliveryStaffs = await _accountService.GetStaff(floorInStorage.Key, accessToken, new List<string> { "Delivery Staff" }, request.DeliveryDate, new List<string> { request.DeliveryTime }, false);
                                                if (deliveryStaffs.Count - requestList.Count <= 0) deliFlag = false;
                                            }
                                        }
                                        else
                                        {
                                            foreach (var cuboid in result.BestResult.First())
                                            {
                                                var orderDetail = orderDetailList.Where(orderDetail => orderDetail.Id == (Guid)cuboid.Tag).FirstOrDefault();
                                                if (orderDetail != null) orderDetailList.Remove(orderDetail);
                                                var service = serviceList.Where(service => service.Id == (Guid)cuboid.Tag).FirstOrDefault();
                                                if (service != null) serviceList.Remove(service);
                                            }
                                        }
                                    }
                                    
                                }
                            }
                        }
                        
                    }
                }

                if (!deliFlag) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Don't have enough delivery staff");
                if (!flag) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Not enough space in storages");




                var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
                var userId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);



                request.StorageId = model.StorageId;
                request.ModifiedBy = userId;
                request.Status = 2;
                await UpdateAsync(request);

                string name = null;
                if (request.Type == (int)RequestType.Create_Order) name = "Yêu cầu tạo đơn đã xử lý";
                if (request.Type == (int)RequestType.Extend_Order) name = "Yêu cầu gia hạn đơn đã được xử lý";
                if (request.Type == (int)RequestType.Return_Order) name = "Yêu cầu rút đồ về đă được xử lý";
                await _orderTimelineService.CreateAsync(new OrderTimeline
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
                if (id != model.Id) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Request Id not matched");

                var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
                var userId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);

                var entity = await Get(x => x.Id == id && x.IsActive == true).FirstOrDefaultAsync();
                if (entity == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Request not found");

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
                var request = await Get(x => x.Id == id && x.IsActive).Include(x => x.Customer).Include(x => x.CreatedByNavigation).FirstOrDefaultAsync();
                if (request == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Order not found");
                request.Status = 4;

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

                await _orderTimelineService.CreateAsync(new OrderTimeline
                {
                    RequestId = request.Id,
                    CreatedDate = DateTime.Now,
                    CreatedBy = userId,
                    Datetime = DateTime.Now,
                    Name = "Đơn đang vận chuyển " + name
                });

                var customer = request.Customer;
                if (customer == null) customer = request.CreatedByNavigation;

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

        public async Task<RequestByIdViewModel> DeliverySendRequestNotification(Guid id, string message, string accessToken)
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

                var staffList = storage.StaffAssignStorages.Where(x => x.RoleName != "Delivery Staff" && x.IsActive).Select(x => x.Staff).ToList();

                foreach (var staff in staffList)
                {
                    if (staff.DeviceTokenId != null)
                    {
                        await _firebaseService.SendNoti(message, userId, staff.DeviceTokenId, request.Id, new
                        {
                            Content = message,
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
    }
}
