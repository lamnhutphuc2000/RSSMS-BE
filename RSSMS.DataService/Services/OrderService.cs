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
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
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
        private readonly IOrderTimelinesService _orderTimelineService;
        public OrderService(IUnitOfWork unitOfWork, IOrderRepository repository
            , IFirebaseService firebaseService, IAccountsService accountService,
            IServicesService serviceService,
            IRequestService requestService,
            IOrderTimelinesService orderTimelineService,
            IStorageService storageService, IMapper mapper) : base(unitOfWork, repository)
        {
            _mapper = mapper;
            _firebaseService = firebaseService;
            _storageService = storageService;
            _accountService = accountService;
            _serviceService = serviceService;
            _requestService = requestService;
            _orderTimelineService = orderTimelineService;
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

            if (role == "Office Staff")
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

            if (role == "Office Staff")
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


            //OrderTimeline deliveryTimeline = new OrderTimeline
            //{
            //    CreatedDate = now,
            //    OrderId = order.Id,
            //    Date = order.DeliveryDate.Value,
            //    Description = "Delivery date of order",
            //};
            order.Status = 1;
            //order.OrderTimelines.Add(deliveryTimeline);

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

            await _orderTimelineService.CreateAsync(new OrderTimeline
            {
                OrderId = order.Id,
                CreatedDate = DateTime.Now,
                CreatedBy = userId,
                Datetime = DateTime.Now,
                Name = "Đon đang vận chuyển về kho"
            });

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

        //public static void CopyTo(Stream src, Stream dest)
        //{
        //    byte[] bytes = new byte[4096];

        //    int cnt;

        //    while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
        //    {
        //        dest.Write(bytes, 0, cnt);
        //    }
        //}
        public static byte[] Decompress(byte[] input)
        {
            using var source = new MemoryStream(input);
            byte[] lengthBytes = new byte[4];
            source.Read(lengthBytes, 0, 4);

            var length = BitConverter.ToInt32(lengthBytes, 0);
            using var decompressionStream = new GZipStream(source,
                CompressionMode.Decompress);
            var result = new byte[length];
            decompressionStream.Read(result, 0, length);
            return result;
        }


        public async Task<OrderByIdViewModel> SendOrderNoti(OrderCreateViewModel model, string accessToken)
        {

            byte[] compressed = Convert.FromBase64String("yhIAAB+LCAAAAAAAAArsWNtu3DYQ/ZWFgPTJtEmKF9FAUDRwgxRtWqNxnto8DG+7snXZSlwnbuCnPvQD+jn5mvxJR7J2vb4lTpE2RRBhVxKHnBnOzJkhqddZ6bP9ZlVVO5lb9amtQ/cdUjKXW8scjYRSzohQXBBwkhEbIvVgjAgQs52sC7+tQp9GlgJ8FFAoIqUAIkALYoxihEbnHefW5pRll2p+hDog15O3b/5q5rPvy7dv/mxmRx0+/2i2hh0u2mYYR7XWQmpJBXY2I+/FtH2oytPQnX3jfRf6HocyjuLqmS3DIKgLadU1d/WmNkF12JUO5emC4rVLdzLwvkxl20D1OGAH47fRD0LvunI5tFFsfVL+eqHuOLgU/M8B+rFjUHK2HKTsZGX/vA/dwTTjbD9C1YdLEw4gDaZyyjmhOcnZEaX74y+7HHRUjn4roJ6RGaNQb2y8wi4JVdvsF0MmZmwv4awOTXoa0qLF6KFxftXBYMwBnPUb5060p22TFhtqnyCt+smiQxgglLoVGoIh62AeBjisR46Ei1hPMOsCztNPk0XHIkKG3/ZkpzGPztZME842M2g7P/gxQVkh7ZfXI4wHYeOFoL1xW18oPVZtO6L8QtYilPNFQhcMEX5Z+rSY3qvQzDcNjNspomREejBOaqYVMUxJTA5jSFE4S1gUSgueK+bybMNytI7+1J6AfwRI3hAnCHI5Io1uKazRf8+7CjkWKS37/b29WHbBQh8m5+7O23ZeBViW/a5r671Tumf3ur6veyKjc8UuLJf9sk1jZ7sHfR9S/4A/nhQMr5wBi1FTwql2RDBmiRXCEe+jUkWwRREdDoNTSNDtHi/nX0OVHtbBl/BVak9C8zDX3jinDAEGnIjINDFSCSKMdCI6q4XzaG05WDNE7MWVGD6bpjKG8p97+lbPfja+uw4nqNtVk8bX7So2Qej8xfnOR0qLfCst8u20wMZ2sKQrOHNgCM8tzj5aTQqNN+0jLaR1Kgdx3Q5+PXjPyt/D7OmNxBhL8CfIDO1cbpQ0RBUR18E4QNHjOhhzFTgHagNl745uDMqx6DkxVFiUUBQEgqJE25BL8FpJcSUzPjBoZbWprKt/wxc/XWbpA07LBm8jCZ/Ye59ZroUgh9/IGazFJ8E/u9VtWnpvXeGIkyoS4YucgLQGVzUIeS4oum/YgDTtuIzUJ8ebbUE2aptd6JqNmoaVMju/Z835MBjfAdvPBpfXs/WOqrPOz6HofCng/7MCru4q4PJqAdc6+JwriztPFnAp4wgJbwCRFRyzGvOu4O8t4I/aqr1ZvuWnKd9WBgBjKREyV0RwwwgUXhAWogHFAtWevjtNOI25Y1IRHhhGFHOGgLKSOOUiBSuVoeZL+b7hNum4sF4CHhgVOt5aRSw3nHilNTeIJCbkZfl2zasTf+w/Wgn/MCDfCtzPBpn3LeBThn4p4B9tB35tKfRGB+pQNKM5nmU0o6gELEEQBI9fNkApeQOUP7Tu5L8DJQOQXFFBqBj2BOgIUkRBSWDSRZ3LWPj37CoUuo3KwhIwviCikHHYVUiSAx5womfM4QeVe3sSD33rtXCd+U/Kwdyzb1+l0PT4ZWL6JHD+NwAAAP//AwA3qiriyhIAAA==");
            byte[] decompressed = Decompress(compressed);
            string chuot = Encoding.UTF8.GetString(decompressed);

            //string chuot;
            //var bytes = Convert.FromBase64String("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");
            //using (var msi = new MemoryStream(bytes))
            //using (var mso = new MemoryStream())
            //{
            //    using (var gs = new GZipStream(msi, CompressionMode.Decompress))
            //    {
            //        gs.CopyTo(mso);
            //    }
            //    chuot = Encoding.Unicode.GetString(mso.ToArray());
            //}

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
                        Guid? mainServiceId = null;
                        if (orderDetailToAdd.OrderDetailServices == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Order detail service can not null");
                        var orderDetailServices = orderDetailToAdd.OrderDetailServices.ToList();
                        foreach (var orderDetailService in orderDetailServices)
                        {
                            var service = _serviceService.Get(x => x.IsActive && x.Id == orderDetailService.ServiceId).FirstOrDefault();
                            if (service == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Service not found");
                            orderDetailService.ServiceName = service.Name;
                            orderDetailService.ServiceType = service.Type;
                            orderDetailService.ServiceUrl = service.ImageUrl;
                            if (service.Type == 3 || service.Type == 2)
                            {
                                mainServiceName = service.Name;
                                mainServicePrice = service.Price;
                                mainServiceType = service.Type;
                                mainServiceUrl = service.ImageUrl;
                                mainServiceId = service.Id;
                            }
                            if (mainServiceType == null)
                            {
                                mainServiceName = service.Name;
                                mainServicePrice = service.Price;
                                mainServiceType = service.Type;
                                mainServiceUrl = service.ImageUrl;
                                mainServiceId = service.Id;
                            }
                        }
                        orderDetailToAdd.ServiceId = mainServiceId;
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
            catch (Exception e)
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
            foreach (var request in requests)
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
                    .Include(order => order.Requests)
                    .ToList().AsQueryable()
                    .ToList();
                if (orders.Count == 0) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Order not found");
                if (orders.Count > 1) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Order detail not in the same order");
                var order = orders.First();

                var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
                var userId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);

                order.ModifiedBy = userId;
                order.ModifiedDate = DateTime.Now;
                var orderDetails = order.OrderDetails;
                var orderDetailToAssignFloorList = model.OrderDetailAssignFloor;
                ICollection<OrderDetail> orderDetailsListUpdate = new List<OrderDetail>();
                foreach (var orderDetailToAssignFloor in orderDetailToAssignFloorList)
                {
                    foreach (var orderDetail in orderDetails)
                        if (orderDetail.Id == orderDetailToAssignFloor.OrderDetailId)
                            orderDetail.FloorId = orderDetailToAssignFloor.FloorId;
                }
                order.OrderDetails = orderDetails;
                order.Status = 2;
                await UpdateAsync(order);

                await _orderTimelineService.CreateAsync(new OrderTimeline
                {
                    OrderId = order.Id,
                    CreatedDate = DateTime.Now,
                    CreatedBy = userId,
                    Datetime = DateTime.Now,
                    Name = "Đon đã lưu kho"
                });


                return _mapper.Map<OrderViewModel>(order);
            }
            catch (Exception e)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, e.Message);
            }
        }
    }
}
