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
using RSSMS.DataService.ViewModels.Images;
using RSSMS.DataService.ViewModels.OrderDetails;
using RSSMS.DataService.ViewModels.Orders;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.IO.Compression;
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
        //Task<OrderByIdViewModel> SendOrderNoti(OrderCreateViewModel model, string accessToken);
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
            IAccountService accountService,
            IServiceService serviceService,
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
                .Include(order => order.OrderDetails).Include(floor => floor.OrderDetails).ThenInclude(orderDetail => orderDetail.Floor)
                .ThenInclude(floor => floor.Space).ThenInclude(space => space.Area).ThenInclude(area => area.Storage)
                .Include(order => order.Requests)
                .Include(order => order.OrderAdditionalFees)
                .Include(order => order.OrderDetails).ThenInclude(orderDetail => orderDetail.OrderDetailServiceMaps).ThenInclude(serviceMap => serviceMap.Service)
                .ProjectTo<OrderByIdViewModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
                if (result == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Order id not found");
                var request = result.Requests;
                if (requestTypes.Count > 0) return result;

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

                if (OrderStatuses.Count > 0)
                {
                    order = order.Where(order => OrderStatuses.Contains((int)order.Status))
                        .Include(order => order.OrderHistoryExtensions)
                        .Include(order => order.Storage)
                        .Include(order => order.Requests).ThenInclude(request => request.Schedules)
                        .Include(order => order.OrderDetails).ThenInclude(orderDetail => orderDetail.Images)
                        .Include(order => order.OrderDetails).ThenInclude(orderDetail => orderDetail.OrderDetailServiceMaps)
                        .Include(order => order.OrderAdditionalFees);
                }


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
                    if(secureToken.Claims.First(claim => claim.Type == "storage_id").Value != null) 
                        storageId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "storage_id").Value);
                    if(storageId != null)
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

                var result = order.OrderByDescending(order => order.CreatedDate)
                    .ProjectTo<OrderViewModel>(_mapper.ConfigurationProvider)
                    .DynamicFilter(model)
                    .PagingIQueryable(page, size, CommonConstant.LimitPaging, CommonConstant.DefaultPaging);
                var meo = result.Item2.ToList();
                if (result.Item2 == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Can not found");


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
                        if (service.Type != 1) typeService = (int)service.Type;
                        serviceHeight += Decimal.ToDouble((decimal)service.Height);
                        serviceWidth += Decimal.ToDouble((decimal)service.Width);
                        serviceLength += Decimal.ToDouble((decimal)service.Length);
                        serviceVolumne = (int)serviceId.Amount * serviceHeight * serviceLength * serviceWidth;
                        
                    }

                    if(serviceHeight < height || serviceLength < length || serviceWidth < width || serviceVolumne < volumne)
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


                //int spaceType = 0;
                //bool isMany = false;
                //if (model.Type == (int)OrderType.Kho_tu_quan) spaceType = 1;
                //if (model.Type == (int)OrderType.Giu_do_thue) spaceType = 0;



                //decimal serviceMaxHeight = 0;
                //decimal serviceMaxWidth = 0;
                //decimal serviceMaxLength = 0;

                //List<Cuboid> cuboids = new List<Cuboid>();
                //// Lay kich thuoc cac service khach hang dat 
                //var services = model.OrderDetails.Select(orderDetail => new
                //{
                //    Height = (decimal)orderDetail.Height,
                //    Width = (decimal)orderDetail.Width,
                //    Length = (decimal)orderDetail.Length
                //}).ToList();
                //for (int i = 1; i <= services.Count; i++)
                //{
                //    if (typeService == (int)ServiceType.Gui_theo_dien_tich) isMany = true;
                //    if (serviceMaxHeight < services[i].Height) serviceMaxHeight = services[i].Height;
                //    if (serviceMaxWidth < services[i].Width) serviceMaxWidth = services[i].Width;
                //    if (serviceMaxLength < services[i].Length) serviceMaxLength = services[i].Length;
                //    cuboids.Add(new Cuboid(services[i].Width, services[i].Height, services[i].Length));
                //}



                //// Check xem storage còn đủ chỗ không
                //var floorInStorages = await _storageService.GetFloorWithStorage(storageId, spaceType, (DateTime)model.DeliveryDate, isMany);

                //bool flag = false;
                //bool deliFlag = true;
                //if (floorInStorages == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Not enough space in storages");

                //foreach (var floorInStorage in floorInStorages)
                //{
                //    if (!flag)
                //    {
                //        var floors = floorInStorage.Value.ToList();
                //        List<OrderDetailInFloorViewModel> orderDetailList = new List<OrderDetailInFloorViewModel>();
                //        List<FloorGetByIdViewModel> floorList = new List<FloorGetByIdViewModel>();
                //        foreach (var floor in floors)
                //        {
                //            if (floor.Height >= serviceMaxHeight && floor.Width >= serviceMaxWidth && floor.Length >= serviceMaxLength)
                //            {
                //                floorList.Add(floor);
                //                orderDetailList.AddRange(floor.OrderDetails);
                //            }
                //        }
                //        foreach (var floorInList in floorList)
                //        {
                //            if (!flag)
                //            {
                //                // get request đã được assign vào storage
                //                var requestsAssignStorage = await _requestService.Get(request => request.IsActive && request.TypeOrder == request.TypeOrder && request.Type == (int)RequestType.Create_Order && request.StorageId == floorInStorage.Key && request.Type == (int)RequestType.Create_Order && (request.Status == 2 || request.Status == 3))
                //                    .Include(request => request.Order).ThenInclude(order => order.OrderDetails)
                //                    .Include(request => request.RequestDetails).ThenInclude(requestDetail => requestDetail.Service).ToListAsync();
                //                requestsAssignStorage = requestsAssignStorage.Where(request => (request.DeliveryDate <= request.DeliveryDate && request.ReturnDate >= request.DeliveryDate) || (request.DeliveryDate <= request.DeliveryDate && request.ReturnDate >= request.DeliveryDate)).ToList();
                //                foreach (var requestAssignStorage in requestsAssignStorage)
                //                {
                //                    var servicesInRequestDetail = requestAssignStorage.RequestDetails.Select(requestDetail => new
                //                    {
                //                        ServiceId = (Guid)requestDetail.ServiceId,
                //                        Height = (decimal?)null,
                //                        Width = (decimal?)null,
                //                        Length = (decimal?)null,
                //                        Amount = (int)requestDetail.Amount
                //                    }).ToList();
                //                    if (requestAssignStorage.Order != null)
                //                        servicesInRequestDetail = requestAssignStorage.Order.OrderDetails.Select(orderDetail => new
                //                        {
                //                            ServiceId = orderDetail.Id,
                //                            Height = orderDetail.Height,
                //                            Width = orderDetail.Width,
                //                            Length = orderDetail.Length,
                //                            Amount = 0
                //                        }).ToList();
                //                    for (int i = 1; i <= servicesInRequestDetail.Count; i++)
                //                    {
                //                        for (int j = 0; j < servicesInRequestDetail[i - 1].Amount; j++)
                //                        {
                //                            if (servicesInRequestDetail[i - 1].Amount != 0)
                //                            {
                //                                var service = _serviceService.Get(service => service.Id == servicesInRequestDetail[i - 1].ServiceId).FirstOrDefault();
                //                                if (service.Type != (int)ServiceType.Phu_kien)
                //                                {
                //                                    if (service.Type == (int)ServiceType.Gui_theo_dien_tich) isMany = true;
                //                                    if (serviceMaxHeight < service.Height) serviceMaxHeight = service.Height;
                //                                    if (serviceMaxWidth < service.Width) serviceMaxWidth = service.Width;
                //                                    if (serviceMaxLength < service.Length) serviceMaxLength = service.Length;
                //                                    cuboids.Add(new Cuboid(service.Width, service.Height, service.Length, 0, service.Id));
                //                                }
                //                            }
                //                            else
                //                            {
                //                                cuboids.Add(new Cuboid((decimal)servicesInRequestDetail[i - 1].Width, (decimal)servicesInRequestDetail[i - 1].Height, (decimal)servicesInRequestDetail[i - 1].Length));
                //                            }
                //                        }
                //                    }
                //                }




                //                List<Cuboid> cuboidTmps = new List<Cuboid>();
                //                cuboidTmps.AddRange(cuboids);
                //                foreach (var orderDetail in orderDetailList)
                //                    cuboidTmps.Add(new Cuboid((decimal)orderDetail.Width, (decimal)orderDetail.Height, (decimal)orderDetail.Length, 0, orderDetail.Id));

                //                var parameter = new BinPackParameter(floorInList.Width, floorInList.Height, floorInList.Length, cuboids);

                //                var binPacker = BinPacker.GetDefault(BinPackerVerifyOption.BestOnly);
                //                var result = binPacker.Pack(parameter);
                //                if (result.BestResult.Count == 1)
                //                    flag = true;
                //                else
                //                {
                //                    foreach (var cuboid in result.BestResult.First())
                //                    {
                //                        var orderDetail = orderDetailList.Where(orderDetail => orderDetail.Id == (Guid)cuboid.Tag).FirstOrDefault();
                //                        if (orderDetail != null) orderDetailList.Remove(orderDetail);
                //                    }
                //                }
                //            }
                //        }
                //    }
                //}

                //if (!deliFlag) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Don't have enough delivery staff");
                //if (!flag) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Not enough space in storages");



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
                                    IsActive = true,
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
                Request request = _requestService.Get(request => request.Id == model.RequestId).FirstOrDefault();
                request.OrderId = order.Id;
                request.Status = 3;

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



                var updateEntity = _mapper.Map(model, entity);
                var orderDetails = updateEntity.OrderDetails.Select(c => { c.OrderId = id; return c; }).ToList();
                updateEntity.OrderDetails = orderDetails;
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


        //public async Task<OrderByIdViewModel> SendOrderNoti(OrderCreateViewModel model, string accessToken)
        //{

        //    var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
        //    var userId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);
        //    var role = secureToken.Claims.First(claim => claim.Type.Contains("role")).Value;
        //    var order = _mapper.Map<OrderByIdViewModel>(model);
        //    try
        //    {
        //        if (role == "Delivery Staff")
        //        {
        //            var customer = _accountService.Get(x => x.Id == model.CustomerId).First();
        //            var registrationId = customer.DeviceTokenId;
        //            order.CustomerName = customer.Name;
        //            order.CustomerPhone = customer.Phone;
        //            string description = "Đơn cần được cập nhật";


        //            // Get list of images
        //            var orderDetailImagesList = model.OrderDetails.Select(orderDetail => orderDetail.OrderDetailImages.ToList()).ToList();
        //            List<OrderDetailByIdViewModel> orderDetailToUpdate = new List<OrderDetailByIdViewModel>();
        //            int index = 0;
        //            foreach (var orderDetailImages in orderDetailImagesList)
        //            {
        //                var orderDetailToAdd = order.OrderDetails.ElementAt(index);
        //                int num = 1;
        //                List<AvatarImageViewModel> listImageToAdd = new List<AvatarImageViewModel>();
        //                foreach (var orderDetailImage in orderDetailImages)
        //                {
        //                    if (orderDetailImage.File != null)
        //                    {
        //                        var url = await _firebaseService.UploadImageToFirebase(orderDetailImage.File, "OrderDetail in Order " + order.Id, orderDetailToAdd.Id, "Order detail image - " + num);
        //                        if (url != null)
        //                        {
        //                            AvatarImageViewModel tmp = new AvatarImageViewModel
        //                            {
        //                                Url = url,
        //                                Name = "Order detail image - " + num,
        //                                Note = orderDetailImage.Note,
        //                            };
        //                            listImageToAdd.Add(tmp);
        //                        }
        //                    }
        //                    num++;
        //                }
        //                orderDetailToAdd.Images = listImageToAdd;
        //                string mainServiceName = null;
        //                decimal? mainServicePrice = null;
        //                int? mainServiceType = null;
        //                string mainServiceUrl = null;
        //                Guid? mainServiceId = null;
        //                if (orderDetailToAdd.OrderDetailServices == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Order detail service can not null");
        //                var orderDetailServices = orderDetailToAdd.OrderDetailServices.ToList();
        //                foreach (var orderDetailService in orderDetailServices)
        //                {
        //                    var service = _serviceService.Get(x => x.IsActive && x.Id == orderDetailService.ServiceId).FirstOrDefault();
        //                    if (service == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Service not found");
        //                    orderDetailService.ServiceName = service.Name;
        //                    orderDetailService.ServiceType = service.Type;
        //                    orderDetailService.ServiceUrl = service.ImageUrl;
        //                    if (service.Type == 3 || service.Type == 2)
        //                    {
        //                        mainServiceName = service.Name;
        //                        mainServicePrice = service.Price;
        //                        mainServiceType = service.Type;
        //                        mainServiceUrl = service.ImageUrl;
        //                        mainServiceId = service.Id;
        //                    }
        //                    if (mainServiceType == null)
        //                    {
        //                        mainServiceName = service.Name;
        //                        mainServicePrice = service.Price;
        //                        mainServiceType = service.Type;
        //                        mainServiceUrl = service.ImageUrl;
        //                        mainServiceId = service.Id;
        //                    }
        //                }
        //                orderDetailToAdd.ServiceId = mainServiceId;
        //                orderDetailToAdd.ServiceName = mainServiceName;
        //                orderDetailToAdd.ServicePrice = mainServicePrice;
        //                orderDetailToAdd.ServiceType = mainServiceType;
        //                orderDetailToAdd.ServiceImageUrl = mainServiceUrl;
        //                orderDetailToAdd.OrderDetailServices = orderDetailServices;
        //                orderDetailToUpdate.Add(orderDetailToAdd);
        //                index++;
        //            }
        //            order.OrderDetails = orderDetailToUpdate;
        //            order.RequestId = model.RequestId;
        //            var result = await _firebaseService.SendNoti(description, customer.Id, customer.DeviceTokenId, null, order);


        //            //Dictionary<int, List<AvatarImageViewModel>> imagesOfOrder = new Dictionary<int, List<AvatarImageViewModel>>();
        //            //var orderDetails = model.OrderDetails;
        //            //int num = 0;
        //            //foreach (var orderDetail in orderDetails)
        //            //{
        //            //    var images = orderDetail.Images;
        //            //    foreach (var image in images)
        //            //    {
        //            //        var url = await _firebaseService.UploadImageToFirebase(image.File, "temp", order.Id, orderDetail.Id + "-" + num);
        //            //        if (url != null)
        //            //        {
        //            //            image.File = null;
        //            //            image.Url = url;
        //            //        }
        //            //        num++;
        //            //    }
        //            //    orderDetail.Images = images;
        //            //}
        //            //model.OrderDetails = orderDetails;

        //            //var result = await _firebaseService.SendNoti(description, customerId, registrationId, order.Id, null, model);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        throw new ErrorResponse((int)HttpStatusCode.InternalServerError, e.Message);
        //    }
        //    return order;
        //}

        public async Task<OrderByIdViewModel> Done(OrderDoneViewModel model, string accessToken)
        {
            try
            {
                var order = await Get(order => order.Id == model.OrderId && order.IsActive)
                    .Include(order => order.OrderDetails).ThenInclude(orderDetail => orderDetail.Floor)
                    .Include(order => order.Requests).FirstOrDefaultAsync();
                if (order == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Order not found");

                var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
                var userId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);


                var requests = order.Requests;
                foreach (var request in requests)
                    if (request.Id == model.RequestId) request.Status = 3;

                order.Requests = requests;


                var orderDetails = order.OrderDetails;
                foreach (var orderDetail in orderDetails)
                    if (orderDetail.FloorId != null) orderDetail.FloorId = null;

                order.Status = model.Status;
                order.OrderDetails = orderDetails;
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
                    .Include(order => order.StaffAssignStorages.Where(staff => staff.IsActive == true)).ThenInclude(staffAssign => staffAssign.Staff).FirstOrDefaultAsync();
                if (storage == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Storage not found");
                var order = await Get(order => order.Id == model.OrderId && order.IsActive).Include(order => order.Customer).FirstOrDefaultAsync();
                if (order == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Order not found");
                if (order.Status > 1) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Order had assigned to storage");

                order.Status = 2;
                order.StorageId = storageId;
                await UpdateAsync(order);

                var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
                var userId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);


                var manager = storage.StaffAssignStorages.Where(staffAssign => staffAssign.IsActive && staffAssign.RoleName == "Manager").Select(staffAssign => staffAssign.Staff).FirstOrDefault();
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
                var orderDetailToAssignFloorList = model.OrderDetailAssignFloor;
                foreach (var orderDetailToAssignFloor in orderDetailToAssignFloorList)
                {
                    var floor = await _floorService.GetById(orderDetailToAssignFloor.FloorId);
                    var orderDetail = orderDetails.Where(orderDetail => orderDetail.Id == orderDetailToAssignFloor.OrderDetailId).First();
                    var orderDetailSize = (double)(orderDetail.Height * orderDetail.Width * orderDetail.Length);
                    if(orderDetail.Height > floor.Height) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Floor size not enough for order detail");
                    if (orderDetail.Width > floor.Width) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Floor size not enough for order detail");
                    if (orderDetail.Length > floor.Length) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Floor size not enough for order detail");
                    var oldOrderDetail = _orderDetailService.Get(orderDetail => orderDetail.FloorId == floor.Id).ToList();
                    List<Cuboid> cuboids = new List<Cuboid>();
                    for (int i = 0; i < oldOrderDetail.Count; i++)
                        cuboids.Add(new Cuboid((decimal)oldOrderDetail[i].Width, (decimal)oldOrderDetail[i].Height, (decimal)oldOrderDetail[i].Length));
                    cuboids.Add(new Cuboid((decimal)orderDetail.Width, (decimal)orderDetail.Height, (decimal)orderDetail.Length));
                    var parameter = new BinPackParameter(floor.Width, floor.Height, floor.Length, cuboids);

                    var binPacker = BinPacker.GetDefault(BinPackerVerifyOption.BestOnly);
                    var result = binPacker.Pack(parameter);
                    if (result.BestResult.Count > 1)
                        throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Floor size is overload");

                }

                var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
                var userId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);



                order.ModifiedBy = userId;
                order.ModifiedDate = DateTime.Now;
                foreach (var orderDetailToAssignFloor in orderDetailToAssignFloorList)
                {
                    foreach (var orderDetail in orderDetails)
                        if (orderDetail.Id == orderDetailToAssignFloor.OrderDetailId)
                            orderDetail.FloorId = orderDetailToAssignFloor.FloorId;
                }
                order.OrderDetails = orderDetails;
                order.Status = 2;
                await UpdateAsync(order);
                var request = order.Requests.Where(request => request.Type == (int)RequestType.Create_Order).First();
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
                var orderDetailToAssignFloorList = model.OrderDetailAssignFloor;
                foreach (var orderDetailToAssignFloor in orderDetailToAssignFloorList)
                {
                    var floor = await _floorService.GetById(orderDetailToAssignFloor.FloorId);
                    var orderDetail = orderDetails.Where(orderDetail => orderDetail.Id == orderDetailToAssignFloor.OrderDetailId).First();
                    var orderDetailSize = (double)(orderDetail.Height * orderDetail.Width * orderDetail.Length);
                    if (floor.Available - orderDetailSize < 0) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Floor size not enough for order detail");
                }


                var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
                var userId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);

                order.ModifiedBy = userId;
                order.ModifiedDate = DateTime.Now;
                foreach (var orderDetailToAssignFloor in orderDetailToAssignFloorList)
                {
                    if (orderDetailToAssignFloor.OldFloorId != null)
                    {
                        var oldOrderDetail = _orderDetailService.Get(orderDetail => orderDetail.FloorId == orderDetailToAssignFloor.OldFloorId && orderDetail.Id == orderDetailToAssignFloor.OrderDetailId).FirstOrDefault();
                        if (oldOrderDetail != null)
                        {
                            oldOrderDetail.FloorId = null;
                            await _orderDetailService.UpdateAsync(oldOrderDetail);
                        }

                    }
                }
                foreach (var orderDetailToAssignFloor in orderDetailToAssignFloorList)
                {
                    foreach (var orderDetail in orderDetails)
                    {
                        if (orderDetail.Id == orderDetailToAssignFloor.OrderDetailId)
                            orderDetail.FloorId = orderDetailToAssignFloor.FloorId;
                    }

                }
                order.OrderDetails = orderDetails;
                order.Status = 2;
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
