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
        private readonly IFloorService _floorService;
        public RequestService(IUnitOfWork unitOfWork, IRequestRepository repository, IMapper mapper
            , IScheduleService scheduleService
            , IFirebaseService firebaseService, IStaffAssignStorageService staffAssignStoragesService
            , IOrderHistoryExtensionService orderHistoryExtensionService
            , IOrderTimelineService orderTimelineService
            , IAccountService accountService
            , IStorageService storageService
            , IServiceService serviceService
            , IFloorService floorService
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
            _floorService = floorService;
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

                    OrderHistoryExtension orderExtend = _mapper.Map<OrderHistoryExtension>(model);
                    orderExtend.ModifiedBy = userId;
                    orderExtend.RequestId = request.Id;
                    orderExtend.OrderId = (Guid)model.OrderId;
                    await _orderHistoryExtensionService.CreateAsync(orderExtend);

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

                    await _orderTimelineService.CreateAsync(new OrderTimeline
                    {
                        RequestId = request.Id,
                        CreatedDate = DateTime.Now,
                        CreatedBy = userId,
                        Datetime = DateTime.Now,
                        Name = "Yêu cầu rút đồ về chờ xác nhận"
                    });

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
                    if (model.TypeOrder == (int)OrderType.Giu_do_thue && (bool)model.IsCustomerDelivery)
                    {
                        var deliveryStaffs = await _accountService.GetStaff(null, accessToken, new List<string> { "Delivery Staff" }, model.DeliveryDate, new List<string> { model.DeliveryTime }, true);
                        if (deliveryStaffs.Count == 0) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Don't have enough delivery staff");

                    }

                    decimal serviceMaxHeight = 0;
                    decimal serviceMaxWidth = 0;
                    decimal serviceMaxLength = 0;
                    
                    List<Cuboid> cuboids = new List<Cuboid>();
                    List<Service> serviceList = new List<Service>();
                    // Lay kich thuoc cac service khach hang dat 
                    var services = model.RequestDetails.Select(requestDetail => new
                    {
                        ServiceId = requestDetail.ServiceId,
                        Amount = requestDetail.Amount
                    }).ToList();
                    for (int i = 1; i <= services.Count; i++)
                    {
                        var service = _serviceService.Get(service => service.Id == services[i - 1].ServiceId).FirstOrDefault();
                        if(service.Type != (int)ServiceType.Phu_kien)
                        {
                            if (service.Type == (int)ServiceType.Gui_theo_dien_tich) isMany = true;
                            if (serviceMaxHeight < service.Height) serviceMaxHeight = service.Height;
                            if (serviceMaxWidth < service.Width) serviceMaxWidth = service.Width;
                            if (serviceMaxLength < service.Length) serviceMaxLength = service.Length;
                            cuboids.Add(new Cuboid(service.Width, service.Height, service.Length, 0 , service.Id));
                            serviceList.Add(service);
                        }
                            
                    }


                    
                    // Check xem storage còn đủ chỗ không
                    var floorInStorages = await _storageService.GetFloorWithStorage(null,spaceType,(DateTime)model.DeliveryDate, isMany);

                    bool flag = false;
                    if (floorInStorages == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Not enough space in storages");

                    foreach (var floorInStorage in floorInStorages)
                    {
                        if(!flag)
                        {
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
                            foreach (var floorInList in floorList)
                            {
                                if(!flag)
                                {
                                    List<Cuboid> cuboidTmps = new List<Cuboid>();
                                    cuboidTmps.AddRange(cuboids);
                                    foreach (var orderDetail in orderDetailList)
                                        cuboidTmps.Add(new Cuboid((decimal)orderDetail.Width, (decimal)orderDetail.Height, (decimal)orderDetail.Length,0,orderDetail.Id));

                                    var parameter = new BinPackParameter(floorInList.Width, floorInList.Height, floorInList.Length, cuboids);

                                    var binPacker = BinPacker.GetDefault(BinPackerVerifyOption.BestOnly);
                                    var result = binPacker.Pack(parameter);
                                    if (result.BestResult.Count == 1) flag = true;
                                    else
                                    {
                                        foreach (var cuboid in result.BestResult.First())
                                        {
                                            var orderDetail = orderDetailList.Where(orderDetail => orderDetail.Id == (Guid)cuboid.Tag).FirstOrDefault();
                                            if(orderDetail != null) orderDetailList.Remove(orderDetail);
                                            var service = serviceList.Where(service => service.Id == (Guid)cuboid.Tag).FirstOrDefault();
                                            if (service != null) serviceList.Remove(service);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    
                    if(!flag) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Not enough space in storages");

                    


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

                var request = await Get(x => x.Id == id && x.IsActive == true).Include(x => x.Order).ThenInclude(order => order.OrderHistoryExtensions).FirstOrDefaultAsync();
                if (request == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Request not found");

                request.Status = model.Status;
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
                // check xem còn kho nào còn trống không
                //var storages = await _storageService.GetStorageWithUsage(model.StorageId);
                //var services = request.RequestDetails.Select(requestDetail => new {
                //    ServiceId = requestDetail.ServiceId,
                //    Amount = requestDetail.Amount
                //}).ToList();
                //double height = 0;
                //double width = 0;
                //double length = 0;
                //double volumne = 0;
                //for (int i = 1; i <= services.Count; i++)
                //{
                //    height = 0;
                //    width = 0;
                //    length = 0;
                //    var service = _serviceService.Get(service => service.Id == services[i - 1].ServiceId).FirstOrDefault();
                //    height += Decimal.ToDouble(service.Height);
                //    width += Decimal.ToDouble(service.Width);
                //    length += Decimal.ToDouble(service.Length);
                //    volumne += (int)services[i-1].Amount * height * width * length;
                //}

                //bool flag = false;
                //if (request.TypeOrder == 1)
                //{
                //    int i = 0;
                //    do
                //    {
                //        var areas = storages[i].Areas.Where(area => area.Type == 1).ToList();
                //        if (areas.Select(area => area.Available).Sum() >= volumne) flag = true;
                //        if (areas.Count == 0) flag = false;
                //        i++;
                //    } while (!flag && i < storages.Count);
                //}
                //if (request.TypeOrder == 0)
                //{
                //    int i = 0;
                //    do
                //    {
                //        var areas = storages[i].Areas.Where(area => area.Type == 0).ToList();
                //        foreach (var area in areas)
                //        {
                //            var spaces = area.SpacesInArea;
                //            if (!flag)
                //                foreach (var space in spaces)
                //                {
                //                    if (space.Floors.Count > 0)
                //                        if (space.Floors.Select(floor => floor.Available).Sum() >= volumne) flag = true;
                //                }


                //        }
                //        i++;
                //    } while (!flag && i < storages.Count);
                //}

                //if (!flag) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Not enough space in storages");




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

                await _firebaseService.SendNoti("Nhân viên đang di chuyển đến để " + notiName, userId, customer.DeviceTokenId, request.Id, new
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
