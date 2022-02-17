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
        Task<OrderUpdateViewModel> Update(int id, OrderUpdateViewModel model);
        Task<OrderByIdViewModel> GetById(int id);
        Task<OrderViewModel> Cancel(int id, OrderCancelViewModel model, string accessToken);
        Task<OrderViewModel> SendOrderNoti(OrderViewModel model, string accessToken);
        Task<OrderByIdViewModel> Done(int id);
    }
    class OrderService : BaseService<Order>, IOrderService
    {
        private readonly IMapper _mapper;
        private readonly IOrderDetailService _orderDetailService;
        private readonly IStaffManageStorageService _staffmanageStorageService;
        private readonly INotificationService _notificationService;
        private readonly INotificationDetailService _notificationDetailService;
        private readonly IFirebaseService _firebaseService;

        public OrderService(IUnitOfWork unitOfWork, IOrderRepository repository, IOrderDetailService orderDetailService, IStaffManageStorageService staffmanageStorageService, INotificationService notificationService, INotificationDetailService notificationDetailService, IFirebaseService firebaseService, IMapper mapper) : base(unitOfWork, repository)
        {
            _mapper = mapper;
            _orderDetailService = orderDetailService;
            _staffmanageStorageService = staffmanageStorageService;
            _notificationService = notificationService;
            _notificationDetailService = notificationDetailService;
            _firebaseService = firebaseService;
        }
        public async Task<OrderByIdViewModel> GetById(int id)
        {
            var result = await Get(x => x.Id == id && x.IsActive == true)
                .Include(x => x.OrderHistoryExtensions)
                .Include(x => x.OrderStorageDetails)
                .Include(x => x.OrderDetails)
                .ThenInclude(orderDetail => orderDetail.Images)
                .Include(x => x.OrderDetails)
                .ThenInclude(orderDetail => orderDetail.BoxOrderDetails.Where(boxOrderDetail => boxOrderDetail.IsActive == true))
                .ThenInclude(boxOrderDetail => boxOrderDetail.Box)
                .ThenInclude(box => box.Shelf)
                .ThenInclude(shelf => shelf.Area)
                .ProjectTo<OrderByIdViewModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            if (result == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Order id not found");
            return result;
        }
        public async Task<DynamicModelResponse<OrderViewModel>> GetAll(OrderViewModel model, IList<int> OrderStatuses, DateTime? dateFrom, DateTime? dateTo, string[] fields, int page, int size, string accessToken)
        {
            var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
            var userId = Int32.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);
            var role = secureToken.Claims.First(claim => claim.Type.Contains("role")).Value;


            var order = Get(x => x.IsActive == true)
                .Include(x => x.OrderHistoryExtensions)
                .Include(x => x.OrderStorageDetails)
                .Include(x => x.Schedules)
                .Include(x => x.OrderDetails)
                .ThenInclude(orderDetail => orderDetail.Images)
                .Include(x => x.OrderDetails)
                .ThenInclude(orderDetail => orderDetail.Product);

            if (OrderStatuses != null)
            {
                if (OrderStatuses.Count > 0)
                {
                    order = Get(x => x.IsActive == true).Where(x => OrderStatuses.Contains((int)x.Status))
                        .Include(x => x.OrderHistoryExtensions)
                        .Include(x => x.OrderStorageDetails).Include(x => x.Schedules)
                        .Include(x => x.OrderDetails)
                .ThenInclude(orderDetail => orderDetail.Images)
                    .Include(x => x.OrderDetails)
                    .ThenInclude(orderDetail => orderDetail.Product);
                }
            }


            if (dateFrom != null && dateTo != null)
            {
                order = order
                    .Where(x => (x.ReturnDate >= dateFrom && x.ReturnDate <= dateTo) || (x.DeliveryDate >= dateFrom && x.DeliveryDate <= dateTo))
                .Include(x => x.OrderDetails)
                .ThenInclude(orderDetail => orderDetail.Product);
            }
            if (role == "Manager")
            {
                order = order.Where(x => x.OrderStorageDetails.Count == 0 || x.ManagerId.HasValue == false || x.ManagerId == userId)
                    .Include(x => x.OrderStorageDetails)
                    .Include(x => x.Schedules)
                    .Include(x => x.OrderDetails)
                    .ThenInclude(orderDetail => orderDetail.Product);
            }

            if (role == "Office staff")
            {
                var storageId = Int32.Parse(secureToken.Claims.First(claim => claim.Type == "storage_id").Value);
                order = order.Where(x => x.OrderStorageDetails.Any(a => a.StorageId == storageId) || x.OrderStorageDetails.Count == 0)
                    .Include(x => x.OrderStorageDetails)
                    .Include(x => x.OrderDetails)
                    .ThenInclude(orderDetail => orderDetail.Product);
            }

            if (role == "Customer")
            {
                order = order.Where(x => x.CustomerId == userId)
                    .Include(x => x.OrderStorageDetails)
                    .Include(x => x.OrderDetails)
                    .ThenInclude(orderDetail => orderDetail.Product);
            }

            var result = order.OrderByDescending(x => x.CreatedDate)
                .ProjectTo<OrderViewModel>(_mapper.ConfigurationProvider)
                .DynamicFilter(model)
                .PagingIQueryable(page, size, CommonConstant.LimitPaging, CommonConstant.DefaultPaging);
            if (result.Item2.ToList().Count < 1) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Can not found");


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
            var userId = Int32.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);
            var role = secureToken.Claims.First(claim => claim.Type.Contains("role")).Value;
            int storageId = -1;
            var order = _mapper.Map<Order>(model);

            if (role == "Manager")
            {
                order.ManagerId = userId;
            }

            if (role == "Office staff")
            {
                storageId = Int32.Parse(secureToken.Claims.First(claim => claim.Type == "storage_id").Value);
                var managerId = _staffmanageStorageService.Get(x => x.StorageId == storageId && x.RoleName == "Manager").Select(x => x.UserId).FirstOrDefault();
                if (managerId != 0) order.ManagerId = managerId;
            }

            if (role == "Customer")
            {
                order.CustomerId = userId;
            }

            //Check Type of Order
            if (order.TypeOrder == 1)
            {
                order.ReturnDate = order.DeliveryDate.Value.AddDays((double)model.Duration);
            }
            else if (order.TypeOrder == 0)
            {
                order.ReturnDate = order.DeliveryDate.Value.AddMonths((int)model.Duration);
            }

            await CreateAsync(order);

            if (role == "Office staff")
            {
                OrderStorageDetail orderStorage = new OrderStorageDetail();
                orderStorage.IsActive = true;
                orderStorage.OrderId = order.Id = order.Id;
                if (storageId != -1)
                {
                    orderStorage.StorageId = storageId;
                    order.Status = 2;
                    order.OrderStorageDetails.Add(orderStorage);
                    await UpdateAsync(order);
                }
            }

            //Create order detail
            foreach (ProductOrderViewModel product in model.ListProduct)
            {
                await _orderDetailService.Create(product, order.Id);
            }

            Notification noti = new Notification
            {
                Description = "New order arrive!",
                CreateDate = DateTime.Now,
                IsActive = true,
                Type = 0,
                OrderId = order.Id
            };
            await _notificationService.CreateAsync(noti);

            await _notificationDetailService.PushOrderNoti("New order arrive!", userId, noti.Id, order.Id, null, noti.Id);

            return model;
        }

        public async Task<OrderUpdateViewModel> Update(int id, OrderUpdateViewModel model)
        {
            if (id != model.Id) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Order Id not matched");

            var entity = await Get(x => x.Id == id && x.IsActive == true)
                .Include(x => x.Schedules)
                .Include(x => x.OrderDetails)
                .ThenInclude(orderDetails => orderDetails.Images).AsNoTracking().FirstOrDefaultAsync();
            if (entity == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Order not found");



            var updateEntity = _mapper.Map(model, entity);
            var orderDetails = updateEntity.OrderDetails.Select(c => { c.OrderId = id; return c; }).ToList();
            updateEntity.OrderDetails = orderDetails;
            await UpdateAsync(updateEntity);

            return model;
        }

        public async Task<OrderViewModel> Cancel(int id, OrderCancelViewModel model, string accessToken)
        {
            var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
            var userId = Int32.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);

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
            var userId = Int32.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);
            var role = secureToken.Claims.First(claim => claim.Type.Contains("role")).Value;
            var order = await Get(x => x.IsActive == true && x.Id == model.Id).Include(x => x.Customer).FirstOrDefaultAsync();

            if (role == "Delivery Staff")
            {
                int customerId = (int)order.CustomerId;
                var registrationId = order.Customer.DeviceTokenId;
                string description = "Please commit order changes";
                Notification noti = new Notification
                {
                    CreateDate = DateTime.Now,
                    IsActive = true,
                    OrderId = order.Id,
                    Status = 0,
                    Type = 2,
                    Description = description,
                    Note = "Delivery staff id: " + userId + " has commit the order: " + order.Id + " changes",
                };
                await _notificationService.CreateAsync(noti);


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

                var result = await _notificationDetailService.SendNoti(description, userId, customerId, registrationId, noti.Id, order.Id, null, model);
            }
            return _mapper.Map<OrderViewModel>(order);
        }

        public async Task<OrderByIdViewModel> Done(int id)
        {
            var order = await Get(x => x.Id == id && x.IsActive == true).Include(x => x.OrderDetails).ThenInclude(orderDetail => orderDetail.BoxOrderDetails).FirstOrDefaultAsync();
            if(order == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Order not found");
            var orderDetails = order.OrderDetails;
            foreach(var orderDetail in orderDetails)
            {
                if(orderDetail.BoxOrderDetails != null)
                {
                    var boxOrderDetail = orderDetail.BoxOrderDetails;
                    foreach(var box in boxOrderDetail)
                    {
                        box.IsActive = false;
                    }
                    orderDetail.BoxOrderDetails = boxOrderDetail;
                }
            }
            order.Status = 6;
            order.OrderDetails = orderDetails;
            await UpdateAsync(order);
            return await GetById(id);
        }
    }
}
