using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Constants;
using RSSMS.DataService.Models;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.Responses;
using RSSMS.DataService.UnitOfWorks;
using RSSMS.DataService.Utilities;
using RSSMS.DataService.ViewModels.Orders;
using RSSMS.DataService.ViewModels.Products;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RSSMS.DataService.Services
{
    public interface IOrderService : IBaseService<Order>
    {
        Task<OrderCreateViewModel> Create(OrderCreateViewModel model);
        Task<DynamicModelResponse<OrderViewModel>> GetAll(OrderViewModel model, DateTime? dateFrom, DateTime? dateTo, string[] fields, int page, int size);
        Task<OrderUpdateViewModel> Update(int id, OrderUpdateViewModel model);
        Task<OrderViewModel> GetById(int id);
        Task<OrderViewModel> Cancel(int id, OrderCancelViewModel model);
    }
    class OrderService : BaseService<Order>, IOrderService
    {
        private readonly IMapper _mapper;
        private readonly IOrderDetailService _orderDetailService;
        public OrderService(IUnitOfWork unitOfWork, IOrderRepository repository, IOrderDetailService orderDetailService, IMapper mapper) : base(unitOfWork, repository)
        {
            _mapper = mapper;
            _orderDetailService = orderDetailService;
        }
        public async Task<OrderViewModel> GetById(int id)
        {
            var result = await Get(x => x.Id == id && x.IsActive == true)
                .ProjectTo<OrderViewModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            if (result == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Order id not found");
            return result;
        }
        public async Task<DynamicModelResponse<OrderViewModel>> GetAll(OrderViewModel model, DateTime? dateFrom, DateTime? dateTo, string[] fields, int page, int size)
        {
            
            var order = Get(x => x.IsActive == true)
                .Include(x => x.OrderDetails)
                .ThenInclude(orderDetail => orderDetail.Product);
            if(dateFrom != null && dateTo != null)
            {
                order = Get(x => x.IsActive == true)
                    .Where(x => (x.ReturnDate >= dateFrom && x.ReturnDate <= dateTo) || (x.DeliveryDate >= dateFrom && x.DeliveryDate <= dateTo))
                .Include(x => x.OrderDetails)
                .ThenInclude(orderDetail => orderDetail.Product);
            }
            var result = order
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

        public async Task<OrderCreateViewModel> Create(OrderCreateViewModel model)
        {
            var order = _mapper.Map<Order>(model);

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

            //Create order detail
            foreach (ProductOrderViewModel product in model.ListProduct)
            {
                await _orderDetailService.Create(product, order.Id);
            }

            return model;
        }

        public async Task<OrderUpdateViewModel> Update(int id, OrderUpdateViewModel model)
        {
            if (id != model.Id) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Order Id not matched");

            var entity = await GetAsync(id);
            if (entity == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Order not found");

            var updateEntity = _mapper.Map(model, entity);
            await UpdateAsync(updateEntity);

            return _mapper.Map<OrderUpdateViewModel>(updateEntity);
        }




        public async Task<OrderViewModel> Cancel(int id, OrderCancelViewModel model)
        {
            if (id != model.Id) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Id not matched");
            var entity = await Get(x => x.Id == id && x.IsActive == true).FirstOrDefaultAsync();
            if (entity == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Order not found");
            entity.Status = 0;
            entity.RejectedReason = model.RejectedReason;
            await UpdateAsync(entity);
            return _mapper.Map<OrderViewModel>(entity);
        }
    }
}
