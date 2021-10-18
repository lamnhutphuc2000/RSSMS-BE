using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.UnitOfWorks;
using RSSMS.DataService.ViewModels.Products;

namespace RSSMS.DataService.Services
{

    public interface IOrderDetailService : IBaseService<OrderDetail>
    {
        Task<ProductOrderViewModel> Create(ProductOrderViewModel model, int orderID);
    }
    class OrderDetailService : BaseService<OrderDetail>, IOrderDetailService
    {
        private readonly IMapper _mapper;

        public OrderDetailService(IUnitOfWork unitOfWork, IOrderDetailRepository repository, IMapper mapper) : base(unitOfWork, repository)
        {
            _mapper = mapper;
        }

        public async Task<ProductOrderViewModel> Create(ProductOrderViewModel model,int orderID)
        {
            var orderDetails = _mapper.Map<OrderDetail>(model);

            orderDetails.OrderId = orderID;
            orderDetails.TotalPrice = (double?)(model.Price * model.Amount);

            await CreateAsync(orderDetails);
            return model;
        }
    }
}
