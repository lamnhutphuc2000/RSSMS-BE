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
        Task<OrderByIdViewModel> GetById(Guid id);
        Task<OrderViewModel> Cancel(Guid id, OrderCancelViewModel model, string accessToken);
        Task<OrderViewModel> SendOrderNoti(OrderViewModel model, string accessToken);
        Task<OrderByIdViewModel> Done(Guid id);
        Task<OrderViewModel> UpdateOrders(List<OrderUpdateStatusViewModel> model);
        Task<OrderViewModel> AssignStorage(OrderAssignStorageViewModel model, string accessToken);
    }
    class OrderService : BaseService<Order>, IOrderService
    {
        private readonly IMapper _mapper;
        private readonly IOrderDetailService _orderDetailService;
        private readonly IFirebaseService _firebaseService;
        private readonly IStorageService _storageService;

        public OrderService(IUnitOfWork unitOfWork, IOrderRepository repository, IOrderDetailService orderDetailService 
            ,IFirebaseService firebaseService,
            IStorageService storageService, IMapper mapper) : base(unitOfWork, repository)
        {
            _mapper = mapper;
            _orderDetailService = orderDetailService;
            _firebaseService = firebaseService;
            _storageService = storageService;
        }
        public async Task<OrderByIdViewModel> GetById(Guid id)
        {
            var result = await Get(x => x.Id == id && x.IsActive == true)
                .Include(x => x.OrderHistoryExtensions)
                .Include(x => x.Storage)
                .Include(x => x.OrderDetails)
                .ThenInclude(orderDetail => orderDetail.Images)
                .Include(x => x.OrderDetails)
                .ThenInclude(orderDetail => orderDetail.Floor)
                .ThenInclude(floor => floor.Space)
                .ThenInclude(shelf => shelf.Area)
                .ProjectTo<OrderByIdViewModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

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

            if (role == "Customer")
            {
                order.CustomerId = userId;
            }

            //Check Type of Order
            if (order.Type == 1)
            {
                order.ReturnDate = order.DeliveryDate.Value.AddDays((double)model.Duration);
            }
            else if (order.Type == 0)
            {
                order.ReturnDate = order.DeliveryDate.Value.AddMonths((int)model.Duration);
            }

            //Create request for order
            Request request = new Request
            {
                CreatedBy = userId,
                CreatedDate = now,
                DeliveryAddress = order.DeliveryAddress,
                DeliveryDate = order.DeliveryDate,
                DeliveryTime = order.DeliveryTime,
                IsActive = true,
                Note = "Request for delivery staff to get order",
                Type = 1,
                OrderId = order.Id,
                Status = 1
            };

            OrderTimeline deliveryTimeline = new OrderTimeline
            {
                CreatedDate = now,
                OrderId = order.Id,
                Date = order.DeliveryDate.Value,
                Description = "Delivery date of order",
            };
            order.Status = 1;
            order.OrderTimelines.Add(deliveryTimeline);
            order.Requests.Add(request);

            // random a name for order
            Random random = new Random();
            order.Name = now.Day + now.Month + now.Year + now.Minute + now.Hour + new string(Enumerable.Repeat(chars, 5).Select(s => s[random.Next(s.Length)]).ToArray());
            await CreateAsync(order);


            //Create order detail
            foreach (ServicesOrderViewModel service in model.ListService)
            {
                await _orderDetailService.Create(service, order.Id);
            }

            

            await _firebaseService.PushOrderNoti("New order arrive!", userId, order.Id, null);

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

        public async Task<OrderViewModel> SendOrderNoti(OrderViewModel model, string accessToken)
        {
            var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
            var userId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);
            var role = secureToken.Claims.First(claim => claim.Type.Contains("role")).Value;
            var order = await Get(x => x.IsActive == true && x.Id == model.Id).Include(x => x.Customer).FirstOrDefaultAsync();

            if (role == "Delivery Staff")
            {
                Guid customerId = (Guid)order.CustomerId;
                var registrationId = order.Customer.DeviceTokenId;
                string description = "Please commit order changes";


                // Get list of images
                Dictionary<int, List<AvatarImageViewModel>> imagesOfOrder = new Dictionary<int, List<AvatarImageViewModel>>();
                var orderDetails = model.OrderDetails;
                int num = 0;
                foreach (var orderDetail in orderDetails)
                {
                    var images = orderDetail.Images;
                    foreach (var image in images)
                    {
                        var url = await _firebaseService.UploadImageToFirebase(image.File, "temp", order.Id, orderDetail.Id + "-" + num);
                        if (url != null)
                        {
                            image.File = null;
                            image.Url = url;
                        }
                        num++;
                    }
                    orderDetail.Images = images;
                }
                model.OrderDetails = orderDetails;

                var result = await _firebaseService.SendNoti(description, customerId, registrationId, order.Id, null, model);
            }
            return _mapper.Map<OrderViewModel>(order);
        }

        public async Task<OrderByIdViewModel> Done(Guid id)
        {
            var order = await Get(x => x.Id == id && x.IsActive == true).Include(x => x.OrderDetails).ThenInclude(orderDetail => orderDetail.Floor).FirstOrDefaultAsync();
            if (order == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Order not found");
            //var orderDetails = order.OrderDetails;
            //foreach (var orderDetail in orderDetails)
            //{
            //    if (orderDetail.Floor != null)
            //    {
            //        var boxOrderDetail = orderDetail.Floor;
            //        orderDetail.
            //        foreach (var box in boxOrderDetail)
            //        {
            //            box.IsActive = false;
            //        }
            //        orderDetail.Floor = boxOrderDetail;
            //    }
            //}
            order.Status = 6;
            //order.OrderDetails = orderDetails;
            await UpdateAsync(order);
            return await GetById(id);
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

            await _firebaseService.SendNoti(description, manager.Id, manager.DeviceTokenId, order.Id, null, null);
            return _mapper.Map<OrderViewModel>(order);
        }
    }
}
