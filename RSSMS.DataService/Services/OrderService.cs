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
using RSSMS.DataService.ViewModels.Orders;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RSSMS.DataService.Services
{
    public interface IOrderService : IBaseService<Order>
    {
        Task<OrderCreateViewModel> Create(OrderCreateViewModel model, string accessToken);
        Task<DynamicModelResponse<OrderViewModel>> GetAll(OrderViewModel model, IList<int> OrderStatuses, DateTime? dateFrom, DateTime? dateTo, string[] fields, int page, int size, string accessToken);
        Task<OrderUpdateViewModel> Update(Guid id, OrderUpdateViewModel model);
        Task<OrderByIdViewModel> GetById(Guid id, IList<int> requestTypes);
        Task<OrderViewModel> Cancel(Guid id, OrderCancelViewModel model, string accessToken);
        Task<OrderByIdViewModel> Done(OrderDoneViewModel model, string accessToken);
        Task<OrderViewModel> UpdateOrders(List<OrderUpdateStatusViewModel> model);
        Task<OrderViewModel> AssignStorage(OrderAssignStorageViewModel model, string accessToken);
        Task<OrderViewModel> AssignFloor(OrderAssignFloorViewModel model, string accessToken);
        Task<OrderViewModel> AssignAnotherFloor(OrderAssignAnotherFloorViewModel model, string accessToken);
    }
    class OrderService : BaseService<Order>, IOrderService
    {
        private readonly IMapper _mapper;
        private readonly IFirebaseService _firebaseService;
        private readonly IStorageService _storageService;
        private readonly IRequestService _requestService;
        private readonly IOrderTimelineService _orderTimelineService;
        private readonly IOrderDetailService _orderDetailService;
        private readonly IFloorService _floorService;
        private readonly IAccountService _accountService;
        private readonly IServiceService _serviceService;
        public OrderService(IUnitOfWork unitOfWork, IOrderRepository repository
            , IFirebaseService firebaseService,
            IRequestService requestService,
            IOrderTimelineService orderTimelineService,
            IOrderDetailService orderDetailService,
            IFloorService floorService,
            IServiceService serviceService,
            IAccountService accountService,
            IStorageService storageService, IMapper mapper) : base(unitOfWork, repository)
        {
            _mapper = mapper;
            _firebaseService = firebaseService;
            _storageService = storageService;
            _requestService = requestService;
            _orderTimelineService = orderTimelineService;
            _orderDetailService = orderDetailService;
            _floorService = floorService;
            _accountService = accountService;
            _serviceService = serviceService;
        }
        public async Task<OrderByIdViewModel> GetById(Guid id, IList<int> requestTypes)
        {
            try
            {
                var result = await Get(x => x.Id == id && x.IsActive)
                    .Include(order => order.OrderDetails).ThenInclude(orderDetail => orderDetail.Export).ThenInclude(export => export.CreatedByNavigation)
                    .Include(order => order.OrderDetails).ThenInclude(orderDetail => orderDetail.Export).ThenInclude(export => export.DeliveryByNavigation)
                    .Include(order => order.OrderDetails).ThenInclude(orderDetail => orderDetail.Import).ThenInclude(import => import.CreatedByNavigation)
                    .Include(order => order.OrderDetails).ThenInclude(orderDetail => orderDetail.Import).ThenInclude(import => import.DeliveryByNavigation)
                    .Include(order => order.Storage)
                    .Include(order => order.OrderDetails).Include(floor => floor.OrderDetails)
                    .Include(order => order.Requests)
                    .Include(order => order.OrderAdditionalFees)
                    .Include(order => order.OrderDetails).ThenInclude(orderDetail => orderDetail.OrderDetailServiceMaps).ThenInclude(serviceMap => serviceMap.Service)
                    .ProjectTo<OrderByIdViewModel>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync();
                if (result == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Order id not found");
                var request = result.Requests;
                if (requestTypes.Count == 0) return result;

                request = request.Where(request => requestTypes.Contains((int)request.Type)).ToList();
                result.Requests = request;
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
        public async Task<DynamicModelResponse<OrderViewModel>> GetAll(OrderViewModel model, IList<int> OrderStatuses, DateTime? dateFrom, DateTime? dateTo, string[] fields, int page, int size, string accessToken)
        {
            try
            {
                var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
                var userId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);
                var role = secureToken.Claims.First(claim => claim.Type.Contains("role")).Value;


                var order = Get(order => order.IsActive)
                    .Include(order => order.OrderHistoryExtensions)
                    .Include(order => order.Storage)
                    .Include(order => order.Requests).ThenInclude(request => request.Schedules)
                    .Include(order => order.OrderDetails).ThenInclude(orderDetail => orderDetail.Images)
                    .Include(order => order.OrderDetails).ThenInclude(orderDetail => orderDetail.OrderDetailServiceMaps)
                    .Include(order => order.OrderAdditionalFees);

                
                

                if (dateFrom != null && dateTo != null)
                {
                    order = order
                        .Where(order => (order.ReturnDate >= dateFrom && order.ReturnDate <= dateTo) || (order.DeliveryDate >= dateFrom && order.DeliveryDate <= dateTo))
                        .Include(order => order.OrderHistoryExtensions)
                        .Include(order => order.Storage)
                        .Include(order => order.Requests).ThenInclude(request => request.Schedules)
                        .Include(order => order.OrderDetails).ThenInclude(orderDetail => orderDetail.Images)
                        .Include(order => order.OrderDetails).ThenInclude(orderDetail => orderDetail.OrderDetailServiceMaps)
                        .Include(order => order.OrderAdditionalFees);
                }
                if (role == "Manager")
                {
                    order = order
                        .Where(order => order.StorageId == null || order.Storage.StaffAssignStorages.Where(order => order.StaffId == userId).First() != null)
                        .Include(order => order.OrderHistoryExtensions)
                        .Include(order => order.Storage)
                        .Include(order => order.Requests).ThenInclude(request => request.Schedules)
                        .Include(order => order.OrderDetails).ThenInclude(orderDetail => orderDetail.Images)
                        .Include(order => order.OrderDetails).ThenInclude(orderDetail => orderDetail.OrderDetailServiceMaps)
                        .Include(order => order.OrderAdditionalFees);
                }

                if (role == "Office Staff")
                {
                    Guid? storageId = null;
                    if (secureToken.Claims.First(claim => claim.Type == "storage_id").Value != null)
                        storageId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "storage_id").Value);
                    if (storageId != null)
                        order = order.Where(order => order.StorageId == storageId || order.StorageId == null)
                            .Include(order => order.OrderHistoryExtensions)
                            .Include(order => order.Storage)
                            .Include(order => order.Requests).ThenInclude(request => request.Schedules)
                            .Include(order => order.OrderDetails).ThenInclude(orderDetail => orderDetail.Images)
                            .Include(order => order.OrderDetails).ThenInclude(orderDetail => orderDetail.OrderDetailServiceMaps)
                            .Include(order => order.OrderAdditionalFees);
                }

                if (role == "Customer")
                {
                    order = order.Where(order => order.CustomerId == userId)
                        .Include(order => order.OrderHistoryExtensions)
                        .Include(order => order.Storage)
                        .Include(order => order.Requests).ThenInclude(request => request.Schedules)
                        .Include(order => order.OrderDetails).ThenInclude(orderDetail => orderDetail.Images)
                        .Include(order => order.OrderDetails).ThenInclude(orderDetail => orderDetail.OrderDetailServiceMaps)
                        .Include(order => order.OrderAdditionalFees);
                }
                var tmps = order.ToList();
                foreach(var tmp in tmps)
                {
                    if(tmp.Status < 5)
                    {
                        if ((tmp.ReturnDate - DateTime.Now).Value.Days < 0)
                        {
                            tmp.Status = 4;
                        }
                        else if ((tmp.ReturnDate - DateTime.Now).Value.Days < 3)
                        {
                            tmp.Status = 3;
                        }
                    }
                }
                if (OrderStatuses.Count > 0)
                {
                    tmps = tmps.Where(x => OrderStatuses.Contains((int)x.Status)).AsQueryable().Include(order => order.OrderHistoryExtensions)
                        .Include(order => order.Storage)
                        .Include(order => order.Requests).ThenInclude(request => request.Schedules)
                        .Include(order => order.OrderDetails).ThenInclude(orderDetail => orderDetail.Images)
                        .Include(order => order.OrderDetails).ThenInclude(orderDetail => orderDetail.OrderDetailServiceMaps)
                        .Include(order => order.OrderAdditionalFees).ToList();
                    var ids = tmps.AsQueryable().Select(x => x.Id).ToList();
                    order = order.Where(x => ids.Contains(x.Id)).Include(order => order.OrderHistoryExtensions)
                            .Include(order => order.Storage)
                            .Include(order => order.Requests).ThenInclude(request => request.Schedules)
                            .Include(order => order.OrderDetails).ThenInclude(orderDetail => orderDetail.Images)
                            .Include(order => order.OrderDetails).ThenInclude(orderDetail => orderDetail.OrderDetailServiceMaps)
                            .Include(order => order.OrderAdditionalFees);
                }
                    
                var result = order.OrderByDescending(order => order.CreatedDate)
                    .ProjectTo<OrderViewModel>(_mapper.ConfigurationProvider).DynamicFilter(model)
                        .PagingIQueryable(page, size, CommonConstant.LimitPaging, CommonConstant.DefaultPaging);

                if (result.Item2.Count() < 1) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Can not found");


                var rs = new DynamicModelResponse<OrderViewModel>
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

        public async Task<OrderCreateViewModel> Create(OrderCreateViewModel model, string accessToken)
        {
            try
            {
                var orderDetails = model.OrderDetails.ToList();
                double height = 0;
                double width = 0;
                double length = 0;
                double volumne = 0;
                double serviceHeight = 0;
                double serviceWidth = 0;
                double serviceLength = 0;
                double serviceVolumne = 0;

                int typeService = 1;
                int spaceType = 0;
                if (model.Type == (int)OrderType.Kho_tu_quan) spaceType = 1;
                bool isMany = false;



                // service list chứa list service người dùng đặt
                List<Cuboid> cuboid = new List<Cuboid>();

                // check order detail customer order
                foreach (var orderDetail in orderDetails)
                {


                    height = Decimal.ToDouble((decimal)orderDetail.Height);
                    width = Decimal.ToDouble((decimal)orderDetail.Width);
                    length = Decimal.ToDouble((decimal)orderDetail.Length);
                    volumne = height * width * length;
                    var servicesIds = orderDetail.OrderDetailServices.Select(service => new { ServiceId = service.ServiceId, Amount = service.Amount });
                    serviceHeight = 0;
                    serviceWidth = 0;
                    serviceLength = 0;
                    serviceVolumne = 0;
                    foreach (var serviceId in servicesIds)
                    {
                        var service = await _serviceService.GetById(serviceId.ServiceId);
                        if (service.Type == (int)ServiceType.Gui_theo_dien_tich) isMany = true;
                        if (service.Type != 1) typeService = (int)service.Type;
                        serviceHeight += Decimal.ToDouble((decimal)service.Height);
                        serviceWidth += Decimal.ToDouble((decimal)service.Width);
                        serviceLength += Decimal.ToDouble((decimal)service.Length);
                        serviceVolumne = (int)serviceId.Amount * serviceHeight * serviceLength * serviceWidth;

                    }
                    if(!((decimal)orderDetail.Height == 0 && (decimal)orderDetail.Width ==0 && (decimal)orderDetail.Length == 0))
                        cuboid.Add(new Cuboid((decimal)orderDetail.Width, (decimal)orderDetail.Height, (decimal)orderDetail.Length, 0, Guid.NewGuid()));
                    if (serviceHeight < height || serviceLength < length || serviceWidth < width || serviceVolumne < volumne)
                        throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Order detail is bigger than service");
                }


                var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
                var userId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);
                var role = secureToken.Claims.First(claim => claim.Type.Contains("role")).Value;
                Guid? storageId = null;


                string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

                var order = _mapper.Map<Order>(model);
                var now = DateTime.Now;
                order.Id = new Guid();

                if (role == "Office Staff")
                {
                    if (secureToken.Claims.First(claim => claim.Type == "storage_id").Value != null)
                        storageId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "storage_id").Value);
                    if (storageId != null)
                        order.StorageId = storageId;
                }

                if (role == "Delivery Staff")
                {
                    storageId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "storage_id").Value);
                    order.StorageId = storageId;
                }



                if (role == "Customer")
                    order.CustomerId = userId;





                var checkResult = await _requestService.CheckStorageAvailable(spaceType, isMany, (int)model.Type, (DateTime)model.DeliveryDate, (DateTime)model.ReturnDate, cuboid, storageId, (bool)model.IsUserDelivery, model.RequestId, accessToken, new List<string> { model.DeliveryTime }, true);
                if (!checkResult) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Kho không còn chỗ chứa");

                order.Status = 1;

                // Random Order name
                Random random = new Random();
                order.Name = now.Day + now.Month + now.Year + now.Minute + now.Hour + new string(Enumerable.Repeat(chars, 5).Select(s => s[random.Next(s.Length)]).ToArray());


                // Get list order detail images
                var orderDetailImagesList = model.OrderDetails.Select(orderDetail => orderDetail.OrderDetailImages.ToList()).ToList();
                order.CreatedBy = userId;
                await CreateAsync(order);
                List<OrderDetail> orderDetailToUpdate = new List<OrderDetail>();
                int index = 0;

                // Upload Order detail images to firebase
                foreach (var orderDetailImages in orderDetailImagesList)
                {
                    var orderDetailToAddImg = order.OrderDetails.ElementAt(index);
                    int num = 1;
                    List<Image> listImageToAdd = new List<Image>();
                    foreach (var orderDetailImage in orderDetailImages)
                    {
                        if (orderDetailImage.File != null)
                        {
                            var url = await _firebaseService.UploadImageToFirebase(orderDetailImage.File, "OrderDetail in Order " + order.Id, orderDetailToAddImg.Id, "Order detail image - " + num);
                            if (url != null)
                            {
                                Image tmp = new Image
                                {
                                    Url = url,
                                    Name = "Order detail image - " + num,
                                    Note = orderDetailImage.Note,
                                    OrderDetailid = orderDetailToAddImg.Id
                                };
                                listImageToAdd.Add(tmp);
                            }
                        }
                        num++;
                    }
                    orderDetailToAddImg.Images = listImageToAdd;
                    orderDetailToUpdate.Add(orderDetailToAddImg);
                    index++;
                }

                // Add order detail back to order and update order
                order.OrderDetails = orderDetailToUpdate;

                await UpdateAsync(order);

                //
                Request request = _requestService.Get(request => request.Id == model.RequestId).Include(request => request.Schedules).FirstOrDefault();
                request.OrderId = order.Id;
                request.Status = (int)RequestStatus.Hoan_thanh;
                var schedules = request.Schedules;
                request.Schedules = schedules.Select(schedule => { schedule.Status = 2; return schedule; }).ToList();

                await _requestService.UpdateAsync(request);

                await _orderTimelineService.CreateAsync(new OrderTimeline
                {
                    RequestId = model.RequestId,
                    CreatedDate = DateTime.Now,
                    CreatedBy = userId,
                    Datetime = DateTime.Now,
                    Name = "Đơn đang vận chuyển về kho"
                });

                await _firebaseService.PushOrderNoti("New order arrive!", order.Id, null);

                return model;
            }
            catch (InvalidOperationException)
            {
                throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Order heigh, width, length overload");
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

        public async Task<OrderUpdateViewModel> Update(Guid id, OrderUpdateViewModel model)
        {
            try
            {
                if (id != model.Id) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Order Id not matched");

                var entity = await Get(x => x.Id == id && x.IsActive == true)
                    .Include(x => x.Requests).ThenInclude(request => request.Schedules)
                    .Include(x => x.OrderDetails)
                    .ThenInclude(orderDetails => orderDetails.Images).AsNoTracking().FirstOrDefaultAsync();
                if (entity == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Order not found");

                bool entityIsPaid = entity.IsPaid;

                var updateEntity = _mapper.Map(model, entity);
                var orderDetails = updateEntity.OrderDetails.Select(c => { c.OrderId = id; return c; }).ToList();
                updateEntity.OrderDetails = orderDetails;
                if (model.IsPaid == null) updateEntity.IsPaid = entityIsPaid;
                await UpdateAsync(updateEntity);

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

        public async Task<OrderViewModel> Cancel(Guid id, OrderCancelViewModel model, string accessToken)
        {
            try
            {
                var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
                var userId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);

                if (id != model.Id) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Id not matched");
                var entity = await Get(order => order.Id == id && order.IsActive).FirstOrDefaultAsync();
                if (entity == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Order not found");
                entity.Status = 0;
                entity.ModifiedBy = userId;
                entity.RejectedReason = model.RejectedReason;
                await UpdateAsync(entity);
                return _mapper.Map<OrderViewModel>(entity);
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



        public async Task<OrderByIdViewModel> Done(OrderDoneViewModel model, string accessToken)
        {
            try
            {
                var order = await Get(order => order.Id == model.OrderId && order.IsActive)
                    .Include(order => order.OrderDetails).ThenInclude(orderDetail => orderDetail.Import)
                    .Include(order => order.OrderDetails).ThenInclude(orderDetail => orderDetail.TransferDetails).ThenInclude(transferDetail => transferDetail.Transfer)
                    .Include(order => order.Requests).ThenInclude(request => request.Schedules).FirstOrDefaultAsync();
                if (order == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Không tìm thấy đơn");

                var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
                var userId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);
                var account = await _accountService.Get(account => account.IsActive && account.Id == userId).Include(account => account.Role)
                                .Include(account => account.StaffAssignStorages).FirstOrDefaultAsync();
                if(account == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Không tìm thấy tài khoản");



                if(account.Role.Name == "Office Staff")
                {
                    if(model.OrderAdditionalFees == null)
                    {
                        var deliveryAccount = await _accountService.Get(account => account.IsActive && account.Id == model.DeliveryBy).Include(account => account.Role)
                                                                .Include(account => account.Schedules).FirstOrDefaultAsync();
                        if(deliveryAccount == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Không tìm thấy tài khoản người vận chuyển");
                        if(deliveryAccount.Role.Name != "Delivery Staff") throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Người nhận không phải người vận chuyển");
                        var requestReturnOrder = order.Requests.Where(request => request.IsActive && request.Type == (int)RequestType.Tra_don && request.Status == (int)RequestStatus.Da_xu_ly).FirstOrDefault();
                        if(requestReturnOrder != null)
                            if( deliveryAccount.Schedules.Where(schedule => schedule.IsActive && schedule.RequestId == requestReturnOrder.Id).FirstOrDefault() == null ) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Người vận chuyển không có lịch trả đơn này");

                        
                        order.Status = (int)OrderStatus.Da_xuat_kho;
                        var orderDetails = order.OrderDetails;
                        // Lấy hết đồ trong order ra
                        IDictionary<Guid, Export> exports = new Dictionary<Guid, Export>();
                        Export export = null;
                        foreach (var orderDetail in orderDetails)
                        {
                            if (orderDetail.ImportId != null)
                            {
                                Guid? floorId = null;
                                if (orderDetail.TransferDetails.Count == 0) 
                                    floorId = orderDetail.Import.FloorId;
                                else
                                    floorId = orderDetail.TransferDetails.OrderByDescending(transferDetail => transferDetail.Transfer.CreatedDate).Select(transferDetail => transferDetail.Transfer.FloorToId).FirstOrDefault();
                                if(exports.Count == 0)
                                {
                                    export = new Export()
                                    {
                                        Code = orderDetail.Import.Code,
                                        CreatedBy = userId,
                                        CreatedDate = DateTime.Now,
                                        FloorId = floorId,
                                        DeliveryBy = model.DeliveryBy,
                                        ReturnAddress = model.ReturnAddress,
                                    };
                                    exports.Add((Guid)floorId, export);
                                }
                                else
                                {
                                    if(!exports.Keys.Contains((Guid)floorId))
                                    {
                                        export = new Export()
                                        {
                                            Code = orderDetail.Import.Code,
                                            CreatedBy = userId,
                                            CreatedDate = DateTime.Now,
                                            FloorId = floorId,
                                            DeliveryBy = model.DeliveryBy,
                                            ReturnAddress = model.ReturnAddress,
                                        };
                                        exports.Add((Guid)floorId, export);
                                    }
                                    else
                                        export = exports[(Guid)floorId];
                                }
                                orderDetail.Export = export;
                                await _orderDetailService.UpdateAsync(orderDetail);
                            }
                        }
                        await UpdateAsync(order);
                        return await GetById(model.OrderId, new List<int>());
                    }
                }
                var requests = order.Requests;
                foreach (var request in requests)
                {
                    if (request.Id == model.RequestId)
                    {
                        request.Status = (int)RequestStatus.Hoan_thanh;
                        var schedules = request.Schedules;
                        request.Schedules = schedules.Select(schedule => { schedule.Status = 2; return schedule; }).ToList();
                    }
                    if (request.Status == (int)RequestStatus.Dang_van_chuyen) request.Status = (int)RequestStatus.Hoan_thanh;
                }
                if (model.OrderAdditionalFees != null)
                {
                    var orderAddtionalFee = model.OrderAdditionalFees.AsQueryable().ProjectTo<OrderAdditionalFee>(_mapper.ConfigurationProvider);
                    order.OrderAdditionalFees = orderAddtionalFee.ToList();
                }


                order.Requests = requests;

                
                order.Status = model.Status;
                order.ModifiedDate = DateTime.Now;
                order.ModifiedBy = userId;
                await UpdateAsync(order);
                return await GetById(model.OrderId, new List<int>());
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

        public async Task<OrderViewModel> UpdateOrders(List<OrderUpdateStatusViewModel> model)
        {
            try
            {
                var orderIds = model.Select(order => order.Id);
                var orders = await Get(order => orderIds.Contains(order.Id) && order.IsActive).ToListAsync();
                if (orders == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Order not found");
                if (orders.Count < model.Count) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Order not found");

                foreach (var order in orders)
                {
                    order.Status = model.Where(a => a.Id == order.Id).First().Status;
                    await UpdateAsync(order);
                }

                return null;
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

        public async Task<OrderViewModel> AssignStorage(OrderAssignStorageViewModel model, string accessToken)
        {
            try
            {
                var storageId = model.StorageId;
                var storage = await _storageService.Get(order => order.Id == storageId && order.IsActive)
                    .Include(order => order.StaffAssignStorages.Where(staff => staff.IsActive)).ThenInclude(staffAssign => staffAssign.Staff).ThenInclude(staff => staff.Role).FirstOrDefaultAsync();
                if (storage == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Storage not found");
                var order = await Get(order => order.Id == model.OrderId && order.IsActive).Include(order => order.Customer).FirstOrDefaultAsync();
                if (order == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Order not found");
                if (order.Status > 1) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Order had assigned to storage");

                order.Status = 2;
                order.StorageId = storageId;
                await UpdateAsync(order);

                var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
                var userId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);


                var manager = storage.StaffAssignStorages.Where(staffAssign => staffAssign.IsActive && staffAssign.Staff.Role.Name == "Manager").Select(staffAssign => staffAssign.Staff).FirstOrDefault();
                var customer = order.Customer;
                string description = "Don " + order.Id + " cua khach hang " + customer.Name + " da duoc xu ly ";

                await _firebaseService.SendNoti(description, manager.Id, manager.DeviceTokenId, null, null);
                return _mapper.Map<OrderViewModel>(order);
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

        public async Task<OrderViewModel> AssignFloor(OrderAssignFloorViewModel model, string accessToken)
        {
            try
            {
                var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
                var userId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);
                var acc = _accountService.Get(account => account.IsActive && account.Id == userId)
                    .Include(account => account.Role).Include(account => account.StaffAssignStorages).FirstOrDefault();
                if(acc == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Không tìm thấy tài khoản");
                if(acc.Role.Name != "Office Staff") throw new ErrorResponse((int)HttpStatusCode.NotFound, "Không phải nhân viên thủ kho");
                var orderDetailIds = model.OrderDetailAssignFloor.Select(orderDetailAssign => orderDetailAssign.OrderDetailId).ToList();
                var orders = Get(order => order.IsActive)
                    .Include(order => order.OrderDetails)
                    .Where(order => order.OrderDetails.Any(orderDetail => orderDetailIds.Contains(orderDetail.Id)))
                    .Include(order => order.Requests)
                    .ToList().AsQueryable()
                    .ToList();
                if (orders.Count == 0) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Không tìm thấy đơn");
                if (orders.Count > 1) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Các món đồ không thuộc cùng 1 đơn");
                var order = orders.First();
                if(order.Status != (int)OrderStatus.Dang_van_chuyen) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Đơn không đang vận chuyển");

                // Check xem tầng còn đủ chỗ không
                var orderDetails = order.OrderDetails;
                bool isMany = false;
                foreach (var orderDetailAssign in model.OrderDetailAssignFloor)
                    if (orderDetailAssign.ServiceType == (int)ServiceType.Gui_theo_dien_tich)
                        isMany = true;
                var request = order.Requests.Where(request => request.IsActive && request.Type == (int)RequestType.Tao_don && request.Status == 3).First();
                var orderDetailToAssignFloorList = model.OrderDetailAssignFloor;

                // Check xem nhân viên thuộc kho hay không
                var storageId = _floorService.Get(floor => floor.Id == orderDetailToAssignFloorList.First().FloorId).Include(floor => floor.Space).ThenInclude(space => space.Area).ThenInclude(area => area.Storage).Select(floor => floor.Space.Area.StorageId).FirstOrDefault();
                if(acc.StaffAssignStorages.Where(staffAssign => staffAssign.IsActive && staffAssign.StorageId == storageId).FirstOrDefault() == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Nhân viên không thuộc kho");

                foreach (var orderDetailToAssignFloor in orderDetailToAssignFloorList)
                {
                    var floor = await _floorService.GetById(orderDetailToAssignFloor.FloorId);
                    var orderDetail = orderDetails.Where(orderDetail => orderDetail.Id == orderDetailToAssignFloor.OrderDetailId).First();
                    var orderDetailSize = (double)(orderDetail.Height * orderDetail.Width * orderDetail.Length);
                    if (orderDetail.Height > floor.Height) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Kích cỡ không đủ để chứa đồ");
                    if (orderDetail.Width > floor.Width) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Kích cỡ không đủ để chứa đồ");
                    if (orderDetail.Length > floor.Length) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Kích cỡ không đủ để chứa đồ");
                    var floorWithOrderDetail = await _floorService.GetFloorWithOrderDetail(floor.Id);

                    var oldOrderDetails = floorWithOrderDetail.OrderDetails.ToList();

                    if (orderDetail.Height != 0 && orderDetail.Length != 0 && orderDetail.Width != 0)
                    {
                        // Check kho tự quản
                        if (request.TypeOrder == (int)OrderType.Kho_tu_quan)
                            if (oldOrderDetails.Count > 0) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Tầng đã được sử dụng");

                        List<Cuboid> cuboids = new List<Cuboid>();
                        for (int i = 0; i < oldOrderDetails.Count; i++)
                        {
                            if (isMany) cuboids.Add(new Cuboid((decimal)oldOrderDetails[i].Width, 1, (decimal)oldOrderDetails[i].Length));
                            else cuboids.Add(new Cuboid((decimal)oldOrderDetails[i].Width, (decimal)oldOrderDetails[i].Height, (decimal)oldOrderDetails[i].Length));
                        }
                        BinPackParameter parameter = null;
                        if (isMany)
                        {
                            cuboids.Add(new Cuboid((decimal)orderDetail.Width, 1, (decimal)orderDetail.Length));
                            parameter = new BinPackParameter(floor.Width, 1, floor.Length, cuboids);
                        }
                        else
                        {
                            cuboids.Add(new Cuboid((decimal)orderDetail.Width, (decimal)orderDetail.Height, (decimal)orderDetail.Length));
                            parameter = new BinPackParameter(floor.Width, floor.Height, floor.Length, cuboids);
                        }

                        var binPacker = BinPacker.GetDefault(BinPackerVerifyOption.BestOnly);
                        var result = binPacker.Pack(parameter);
                        if (result.BestResult.Count > 1)
                            throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Floor size is overload");
                    }

                }



                DateTime now = DateTime.Now;

                order.ModifiedBy = userId;
                order.ModifiedDate = now;
                string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                foreach (var orderDetailToAssignFloor in orderDetailToAssignFloorList)
                {
                    Random random = new Random();
                    Import import = new Import()
                    {
                        CreatedBy = acc.Id,
                        FloorId = orderDetailToAssignFloor.FloorId,
                        DeliveryBy = model.DeliveryId,
                        CreatedDate = now,
                        Code = now.Day + now.Month + now.Year + now.Minute + now.Hour + new string(Enumerable.Repeat(chars, 5).Select(s => s[random.Next(s.Length)]).ToArray())
                    };
                    int index = 0;
                    foreach (var orderDetail in orderDetails)
                        if (orderDetail.Id == orderDetailToAssignFloor.OrderDetailId)
                        {
                            orderDetail.ImportNote = orderDetailToAssignFloor.ImportNote;
                            orderDetail.Import = import;
                            orderDetail.ImportCode = import.Code + " - " + index;
                        }
                    
                }
                order.OrderDetails = orderDetails;
                order.Status = (int)OrderStatus.Da_luu_kho;
                await UpdateAsync(order);
                await _orderTimelineService.CreateAsync(new OrderTimeline
                {
                    RequestId = request.Id,
                    CreatedDate = DateTime.Now,
                    CreatedBy = userId,
                    Datetime = DateTime.Now,
                    Name = "Đơn đã lưu kho"
                });


                return _mapper.Map<OrderViewModel>(order);
            }
            catch (InvalidOperationException)
            {
                throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Order heigh, width, length overload");
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
        public async Task<OrderViewModel> AssignAnotherFloor(OrderAssignAnotherFloorViewModel model, string accessToken)
        {
            try
            {
                var orderDetailIds = model.OrderDetailAssignFloor.Select(orderDetailAssign => orderDetailAssign.OrderDetailId).ToList();
                var orders = Get(order => order.IsActive)
                    .Include(order => order.OrderDetails)
                    .Where(order => order.OrderDetails.Any(orderDetail => orderDetailIds.Contains(orderDetail.Id)))
                    .Include(order => order.Requests)
                    .ToList().AsQueryable()
                    .ToList();
                if (orders.Count == 0) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Order not found");
                if (orders.Count > 1) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Order detail not in the same order");
                var order = orders.First();


                var orderDetails = order.OrderDetails;
                bool isMany = false;
                foreach (var orderDetailAssign in model.OrderDetailAssignFloor)
                    if (orderDetailAssign.ServiceType == (int)ServiceType.Gui_theo_dien_tich)
                        isMany = true;
                var request = order.Requests.Where(request => request.IsActive && request.Type == (int)RequestType.Tao_don && request.Status == 3).First();
                var orderDetailToAssignFloorList = model.OrderDetailAssignFloor;
                foreach (var orderDetailToAssignFloor in orderDetailToAssignFloorList)
                {
                    var floor = await _floorService.GetById(orderDetailToAssignFloor.FloorId);
                    var orderDetail = orderDetails.Where(orderDetail => orderDetail.Id == orderDetailToAssignFloor.OrderDetailId).First();
                    var orderDetailSize = (double)(orderDetail.Height * orderDetail.Width * orderDetail.Length);
                    if (orderDetail.Height > floor.Height) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Floor size not enough for order detail");
                    if (orderDetail.Width > floor.Width) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Floor size not enough for order detail");
                    if (orderDetail.Length > floor.Length) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Floor size not enough for order detail");
                    var floorWithOrderDetail = await _floorService.GetFloorWithOrderDetail(floor.Id);

                    var oldOrderDetails = floorWithOrderDetail.OrderDetails.ToList();

                    if (orderDetail.Height != 0 && orderDetail.Length != 0 && orderDetail.Width != 0)
                    {
                        // Check kho tự quản
                        if (request.TypeOrder == (int)OrderType.Kho_tu_quan)
                            if (oldOrderDetails.Count > 0) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Floor đã được xài");

                        List<Cuboid> cuboids = new List<Cuboid>();
                        for (int i = 0; i < oldOrderDetails.Count; i++)
                        {
                            if (isMany) cuboids.Add(new Cuboid((decimal)oldOrderDetails[i].Width, 1, (decimal)oldOrderDetails[i].Length));
                            else cuboids.Add(new Cuboid((decimal)oldOrderDetails[i].Width, (decimal)oldOrderDetails[i].Height, (decimal)oldOrderDetails[i].Length));
                        }
                        BinPackParameter parameter = null;
                        if (isMany)
                        {
                            cuboids.Add(new Cuboid((decimal)orderDetail.Width, 1, (decimal)orderDetail.Length));
                            parameter = new BinPackParameter(floor.Width, 1, floor.Length, cuboids);
                        }
                        else
                        {
                            cuboids.Add(new Cuboid((decimal)orderDetail.Width, (decimal)orderDetail.Height, (decimal)orderDetail.Length));
                            parameter = new BinPackParameter(floor.Width, floor.Height, floor.Length, cuboids);
                        }

                        var binPacker = BinPacker.GetDefault(BinPackerVerifyOption.BestOnly);
                        var result = binPacker.Pack(parameter);
                        if (result.BestResult.Count > 1)
                            throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Floor size is overload");
                    }

                }





                var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
                var userId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);
                DateTime now = DateTime.Now;
                order.ModifiedBy = userId;
                order.ModifiedDate = now;
                foreach (var orderDetailToAssignFloor in orderDetailToAssignFloorList)
                {
                    if (orderDetailToAssignFloor.OldFloorId != null)
                    {
                        Transfer transfer = new Transfer()
                        {
                            CreatedDate = now,
                            CreatedBy = userId,
                            FloorFromId = orderDetailToAssignFloor.OldFloorId,
                            FloorToId = orderDetailToAssignFloor.FloorId,

                        };
                        TransferDetail transferDetail = new TransferDetail()
                        {
                            OrderDetailId = orderDetailToAssignFloor.OrderDetailId,
                            Transfer = transfer
                        };

                        var oldOrderDetail = orderDetails.Where(orderDetail => orderDetail.Id == orderDetailToAssignFloor.OrderDetailId).FirstOrDefault();
                        if (oldOrderDetail != null)
                        {
                            List<TransferDetail> transferDetailList = new List<TransferDetail>();
                            transferDetailList.Add(transferDetail);
                            oldOrderDetail.TransferDetails = transferDetailList;
                            await _orderDetailService.UpdateAsync(oldOrderDetail);
                        }

                    }
                }
                order.Status = (int)OrderStatus.Da_luu_kho;
                await UpdateAsync(order);

                return _mapper.Map<OrderViewModel>(order);
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
