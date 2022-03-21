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
        Task<OrderViewModel> AssignFloor(OrderAssignFloorViewModel model, string accessToken);
    }
    class OrderService : BaseService<Order>, IOrderService
    {
        private readonly IMapper _mapper;
        private readonly IFirebaseService _firebaseService;
        private readonly IStorageService _storageService;

        public OrderService(IUnitOfWork unitOfWork, IOrderRepository repository
            ,IFirebaseService firebaseService,
            IStorageService storageService, IMapper mapper) : base(unitOfWork, repository)
        {
            _mapper = mapper;
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
            if (order.Type == 1)
            {
                order.ReturnDate = order.DeliveryDate.Value.AddDays((double)model.Duration);
            }
            else if (order.Type == 0)
            {
                order.ReturnDate = order.DeliveryDate.Value.AddMonths((int)model.Duration);
            }


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
                    orderDetailImage.File = "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";
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
