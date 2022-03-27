using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Constants;
using RSSMS.DataService.Models;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.Responses;
using RSSMS.DataService.UnitOfWorks;
using RSSMS.DataService.Utilities;
using RSSMS.DataService.ViewModels.Images;
using RSSMS.DataService.ViewModels.OrderDetails;
using RSSMS.DataService.ViewModels.Orders;
using RSSMS.DataService.ViewModels.Products;
using RSSMS.DataService.ViewModels.Services;
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
        Task<OrderByIdViewModel> SendOrderNoti(OrderCreateViewModel model, string accessToken);
        Task<OrderByIdViewModel> Done(Guid orderId, Guid requestId);
        Task<OrderViewModel> UpdateOrders(List<OrderUpdateStatusViewModel> model);
        Task<OrderViewModel> AssignStorage(OrderAssignStorageViewModel model, string accessToken);
        Task<OrderViewModel> AssignFloor(OrderAssignFloorViewModel model, string accessToken);
    }
    class OrderService : BaseService<Order>, IOrderService
    {
        private readonly IMapper _mapper;
        private readonly IFirebaseService _firebaseService;
        private readonly IStorageService _storageService;
        private readonly IAccountsService _accountService;
        private readonly IServicesService _serviceService;
        private readonly IRequestService _requestService;

        public OrderService(IUnitOfWork unitOfWork, IOrderRepository repository
            ,IFirebaseService firebaseService, IAccountsService accountService,
            IServicesService serviceService,
            IRequestService requestService,
            IStorageService storageService, IMapper mapper) : base(unitOfWork, repository)
        {
            _mapper = mapper;
            _firebaseService = firebaseService;
            _storageService = storageService;
            _accountService = accountService;
            _serviceService = serviceService;
            _requestService = requestService;
        }
        public async Task<OrderByIdViewModel> GetById(Guid id, IList<int> requestTypes)
        {
            var result = await Get(x => x.Id == id && x.IsActive == true)
                .Include(order => order.Requests)
                .Include(order => order.OrderDetails).ThenInclude(orderDetail => orderDetail.OrderDetailServiceMaps).ThenInclude(serviceMap => serviceMap.Service)
                .ProjectTo<OrderByIdViewModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
            var request = result.Requests;
            if (requestTypes.Count > 0)
            {
                request = request.Where(request => requestTypes.Contains((int)request.Type)).ToList();
                result.Requests = request;
            }

            if (result == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Order id not found");
            return result;
        }
        public async Task<DynamicModelResponse<OrderViewModel>> GetAll(OrderViewModel model, IList<int> OrderStatuses, DateTime? dateFrom, DateTime? dateTo, string[] fields, int page, int size, string accessToken)
        {
            var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
            var userId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);
            var role = secureToken.Claims.First(claim => claim.Type.Contains("role")).Value;


            var order = Get(x => x.IsActive == true)
                .Include(x => x.OrderHistoryExtensions)
            .Include(x => x.Storage)
            .Include(x => x.Requests).ThenInclude(request => request.Schedules)
            .Include(x => x.OrderDetails)
            .ThenInclude(orderDetail => orderDetail.Images)
            .Include(x => x.OrderDetails)
            .ThenInclude(orderDetail => orderDetail.OrderDetailServiceMaps);

            if (OrderStatuses.Count > 0)
            {
                order = Get(x => x.IsActive == true).Where(x => OrderStatuses.Contains((int)x.Status))
                    .Include(x => x.OrderHistoryExtensions)
                    .Include(x => x.Storage).Include(x => x.Requests).ThenInclude(request => request.Schedules)
                    .Include(x => x.OrderDetails)
                    .ThenInclude(orderDetail => orderDetail.Images)
                    .Include(x => x.OrderDetails)
                    .ThenInclude(orderDetail => orderDetail.OrderDetailServiceMaps);
            }


            if (dateFrom != null && dateTo != null)
            {
                order = order
                    .Where(x => (x.ReturnDate >= dateFrom && x.ReturnDate <= dateTo) || (x.DeliveryDate >= dateFrom && x.DeliveryDate <= dateTo))
                .Include(x => x.OrderDetails)
                .ThenInclude(orderDetail => orderDetail.OrderDetailServiceMaps);
            }
            if (role == "Manager")
            {
                order = order.Where(x => x.StorageId == null || x.Storage.StaffAssignStorages.Where(x => x.StaffId == userId).First() != null)
                    .Include(x => x.Storage)
                    .Include(x => x.Requests).ThenInclude(request => request.Schedules)
                    .Include(x => x.OrderDetails)
                    .ThenInclude(orderDetail => orderDetail.OrderDetailServiceMaps);
            }

            if (role == "Office staff")
            {
                var storageId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "storage_id").Value);
                order = order.Where(x => x.StorageId == storageId || x.StorageId == null)
                    .Include(x => x.Storage)
                    .Include(x => x.OrderDetails)
                    .ThenInclude(orderDetail => orderDetail.OrderDetailServiceMaps);
            }

            if (role == "Customer")
            {
                order = order.Where(x => x.CustomerId == userId)
                    .Include(x => x.Storage)
                    .Include(x => x.OrderDetails)
                    .ThenInclude(orderDetail => orderDetail.OrderDetailServiceMaps);
            }

            var result = order.OrderByDescending(x => x.CreatedDate)
                .ProjectTo<OrderViewModel>(_mapper.ConfigurationProvider)
                .DynamicFilter(model)
                .PagingIQueryable(page, size, CommonConstant.LimitPaging, CommonConstant.DefaultPaging);
            var meo = result.Item2.ToList();
            if(result.Item2 == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Can not found");


            var rs = new DynamicModelResponse<OrderViewModel>
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

        public async Task<OrderCreateViewModel> Create(OrderCreateViewModel model, string accessToken)
        {
            var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
            var userId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);
            var role = secureToken.Claims.First(claim => claim.Type.Contains("role")).Value;
            Guid? storageId = null;


            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            var order = _mapper.Map<Order>(model);
            var now = DateTime.Now;
            order.Id = new Guid();

            if (role == "Office staff")
            {
                storageId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "storage_id").Value);
                order.StorageId = storageId;
            }

            if (role == "Delivery Staff")
            {
                storageId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "storage_id").Value);
                order.StorageId = storageId;
            }

            if (role == "Customer")
            {
                order.CustomerId = userId;
            }

            //Check Type of Order
            //if (order.Type == 1)
            //{
            //    order.ReturnDate = order.DeliveryDate.Value.AddDays((double)model.Duration);
            //}
            //else if (order.Type == 0)
            //{
            //    order.ReturnDate = order.DeliveryDate.Value.AddMonths((int)model.Duration);
            //}


            OrderTimeline deliveryTimeline = new OrderTimeline
            {
                CreatedDate = now,
                OrderId = order.Id,
                Date = order.DeliveryDate.Value,
                Description = "Delivery date of order",
            };
            order.Status = 1;
            order.OrderTimelines.Add(deliveryTimeline);

            // random a name for order
            Random random = new Random();
            order.Name = now.Day + now.Month + now.Year + now.Minute + now.Hour + new string(Enumerable.Repeat(chars, 5).Select(s => s[random.Next(s.Length)]).ToArray());


            var orderDetailImagesList = model.OrderDetails.Select(orderDetail => orderDetail.OrderDetailImages.ToList()).ToList();
            order.CreatedBy = userId;
            await CreateAsync(order);
            List<OrderDetail> orderDetailToUpdate = new List<OrderDetail>();
            int index = 0;
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
            order.OrderDetails = orderDetailToUpdate;
            await UpdateAsync(order);
            Request request = _requestService.Get(request => request.Id == model.RequestId).FirstOrDefault();
            request.OrderId = order.Id;
            request.Status = 3;

            await _requestService.UpdateAsync(request);

            await _firebaseService.PushOrderNoti("New order arrive!", order.Id, null);

            return model;
        }

        public async Task<OrderUpdateViewModel> Update(Guid id, OrderUpdateViewModel model)
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

        public async Task<OrderViewModel> Cancel(Guid id, OrderCancelViewModel model, string accessToken)
        {
            var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
            var userId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);

            if (id != model.Id) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Id not matched");
            var entity = await Get(x => x.Id == id && x.IsActive == true).FirstOrDefaultAsync();
            if (entity == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Order not found");
            entity.Status = 0;
            entity.ModifiedBy = userId;
            entity.RejectedReason = model.RejectedReason;
            await UpdateAsync(entity);
            return _mapper.Map<OrderViewModel>(entity);
        }

        public async Task<OrderByIdViewModel> SendOrderNoti(OrderCreateViewModel model, string accessToken)
        {
            var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
            var userId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);
            var role = secureToken.Claims.First(claim => claim.Type.Contains("role")).Value;
            var order = _mapper.Map<OrderByIdViewModel>(model);
            try
            {
                if (role == "Delivery Staff")
                {
                    var customer = _accountService.Get(x => x.Id == model.CustomerId).First();
                    var registrationId = customer.DeviceTokenId;
                    order.CustomerName = customer.Name;
                    order.CustomerPhone = customer.Phone;
                    string description = "Đơn cần được cập nhật";


                    // Get list of images
                    var orderDetailImagesList = model.OrderDetails.Select(orderDetail => orderDetail.OrderDetailImages.ToList()).ToList();
                    List<OrderDetailByIdViewModel> orderDetailToUpdate = new List<OrderDetailByIdViewModel>();
                    int index = 0;
                    foreach (var orderDetailImages in orderDetailImagesList)
                    {
                        var orderDetailToAdd = order.OrderDetails.ElementAt(index);
                        int num = 1;
                        List<AvatarImageViewModel> listImageToAdd = new List<AvatarImageViewModel>();
                        foreach (var orderDetailImage in orderDetailImages)
                        {
                            if (orderDetailImage.File != null)
                            {
                                var url = await _firebaseService.UploadImageToFirebase(orderDetailImage.File, "OrderDetail in Order " + order.Id, orderDetailToAdd.Id, "Order detail image - " + num);
                                if (url != null)
                                {
                                    AvatarImageViewModel tmp = new AvatarImageViewModel
                                    {
                                        Url = url,
                                        Name = "Order detail image - " + num,
                                        Note = orderDetailImage.Note,
                                    };
                                    listImageToAdd.Add(tmp);
                                }
                            }
                            num++;
                        }
                        orderDetailToAdd.Images = listImageToAdd;
                        string mainServiceName = null;
                        decimal? mainServicePrice = null;
                        int? mainServiceType = null;
                        string mainServiceUrl = null;
                        if (orderDetailToAdd.OrderDetailServices == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Order detail service can not null");
                        var orderDetailServices = orderDetailToAdd.OrderDetailServices.ToList();
                        foreach(var orderDetailService in orderDetailServices)
                        {
                            var service = _serviceService.Get(x => x.IsActive && x.Id == orderDetailService.ServiceId).FirstOrDefault();
                            if (service == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Service not found");
                            orderDetailService.ServiceName = service.Name;
                            orderDetailService.ServiceType = service.Type;
                            orderDetailService.ServiceUrl = service.ImageUrl;
                            if(service.Type == 3 || service.Type == 2)
                            {
                                mainServiceName = service.Name;
                                mainServicePrice = service.Price;
                                mainServiceType = service.Type;
                                mainServiceUrl = service.ImageUrl;
                            }
                            if(mainServiceType == null)
                            {
                                mainServiceName = service.Name;
                                mainServicePrice = service.Price;
                                mainServiceType = service.Type;
                            }
                        }

                        orderDetailToAdd.ServiceName = mainServiceName;
                        orderDetailToAdd.ServicePrice = mainServicePrice;
                        orderDetailToAdd.ServiceType = mainServiceType;
                        orderDetailToAdd.ServiceImageUrl = mainServiceUrl;
                        orderDetailToAdd.OrderDetailServices = orderDetailServices;
                        orderDetailToUpdate.Add(orderDetailToAdd);
                        index++;
                    }
                    order.OrderDetails = orderDetailToUpdate;
                    order.RequestId = model.RequestId;
                    var result = await _firebaseService.SendNoti(description, customer.Id, customer.DeviceTokenId, null, order);


                    //Dictionary<int, List<AvatarImageViewModel>> imagesOfOrder = new Dictionary<int, List<AvatarImageViewModel>>();
                    //var orderDetails = model.OrderDetails;
                    //int num = 0;
                    //foreach (var orderDetail in orderDetails)
                    //{
                    //    var images = orderDetail.Images;
                    //    foreach (var image in images)
                    //    {
                    //        var url = await _firebaseService.UploadImageToFirebase(image.File, "temp", order.Id, orderDetail.Id + "-" + num);
                    //        if (url != null)
                    //        {
                    //            image.File = null;
                    //            image.Url = url;
                    //        }
                    //        num++;
                    //    }
                    //    orderDetail.Images = images;
                    //}
                    //model.OrderDetails = orderDetails;

                    //var result = await _firebaseService.SendNoti(description, customerId, registrationId, order.Id, null, model);
                }
            }
            catch(Exception e)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, e.Message);
            }
            return order;
        }

        public async Task<OrderByIdViewModel> Done(Guid orderId, Guid requestId)
        {
            var order = await Get(x => x.Id == orderId && x.IsActive == true).Include(x => x.OrderDetails).ThenInclude(orderDetail => orderDetail.Floor)
                .Include(x => x.Requests).FirstOrDefaultAsync();
            if (order == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Order not found");

            var requests = order.Requests;
            foreach(var request in requests)
            {
                if (request.Id == requestId) request.Status = 3;
            }
            order.Requests = requests;


            var orderDetails = order.OrderDetails;
            foreach (var orderDetail in orderDetails)
            {
                if (orderDetail.FloorId != null)
                {
                    orderDetail.FloorId = null;
                }
            }
            order.Status = 6;
            order.OrderDetails = orderDetails;
            await UpdateAsync(order);
            return await GetById(orderId, new List<int>());
        }

        public async Task<OrderViewModel> UpdateOrders(List<OrderUpdateStatusViewModel> model)
        {
            var orderIds = model.Select(x => x.Id);
            var orders = await Get(x => orderIds.Contains(x.Id) && x.IsActive == true).ToListAsync();
            if (orders == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Order not found");
            if (orders.Count < model.Count) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Order not found");

            foreach (var order in orders)
            {
                order.Status = model.Where(a => a.Id == order.Id).First().Status;
                await UpdateAsync(order);
            }

            return null;
        }

        public async Task<OrderViewModel> AssignStorage(OrderAssignStorageViewModel model, string accessToken)
        {
            var storageId = model.StorageId;
            var storage = await _storageService.Get(x => x.Id == storageId && x.IsActive == true).Include(x => x.StaffAssignStorages.Where(staff => staff.IsActive == true)).ThenInclude(staffAssign => staffAssign.Staff).FirstOrDefaultAsync();
            if (storage == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Storage not found");
            var order = await Get(x => x.Id == model.OrderId && x.IsActive == true).Include(x => x.Customer).FirstOrDefaultAsync();
            if (order == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Order not found");
            if (order.Status > 1) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Order had assigned to storage");

            order.Status = 2;
            order.StorageId = storageId;
            await UpdateAsync(order);

            var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
            var userId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);


            var manager = storage.StaffAssignStorages.Where(x => x.IsActive == true && x.RoleName == "Manager").Select(x => x.Staff).FirstOrDefault();
            var customer = order.Customer;
            string description = "Don " + order.Id + " cua khach hang " + customer.Name + " da duoc xu ly ";

            await _firebaseService.SendNoti(description, manager.Id, manager.DeviceTokenId, null, null);
            return _mapper.Map<OrderViewModel>(order);
        }

        public async Task<OrderViewModel> AssignFloor(OrderAssignFloorViewModel model, string accessToken)
        {
            try
            {
                var orderDetailIds = model.OrderDetailAssignFloor.Select(x => x.OrderDetailId).ToList();
                var orders = Get(x => x.IsActive)
                    .Include(order => order.OrderDetails)
                    .Where(order => order.OrderDetails.Any(orderDetail => orderDetailIds.Contains(orderDetail.Id)))
                    .ToList().AsQueryable()
                    .ToList();
                if(orders.Count == 0) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Order not found");
                if(orders.Count > 1) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Order detail not in the same order");
                var order = orders.First();

                var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
                var userId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);

                order.ModifiedBy = userId;
                order.ModifiedDate = DateTime.Now;
                var orderDetails = order.OrderDetails;
                var orderDetailToAssignFloorList = model.OrderDetailAssignFloor;
                ICollection<OrderDetail> orderDetailsListUpdate = new List<OrderDetail>();
                foreach(var orderDetailToAssignFloor in orderDetailToAssignFloorList)
                {
                    foreach(var orderDetail in orderDetails)
                        if (orderDetail.Id == orderDetailToAssignFloor.OrderDetailId) 
                            orderDetail.FloorId = orderDetailToAssignFloor.FloorId;
                }
                order.OrderDetails = orderDetails;
                order.Status = 2;
                await UpdateAsync(order);
                return _mapper.Map<OrderViewModel>(order);
            }
            catch(Exception e)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, e.Message);
            }
        }
    }
}
