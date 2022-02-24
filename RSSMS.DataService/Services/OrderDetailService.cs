﻿using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.UnitOfWorks;
using RSSMS.DataService.ViewModels.Products;
using System;
using System.Threading.Tasks;

namespace RSSMS.DataService.Services
{

    public interface IOrderDetailService : IBaseService<OrderDetail>
    {
        Task<ProductOrderViewModel> Create(ProductOrderViewModel model, Guid orderID);
    }
    class OrderDetailService : BaseService<OrderDetail>, IOrderDetailService
    {
        private readonly IMapper _mapper;

        public OrderDetailService(IUnitOfWork unitOfWork, IOrderDetailRepository repository, IMapper mapper) : base(unitOfWork, repository)
        {
            _mapper = mapper;
        }

        public async Task<ProductOrderViewModel> Create(ProductOrderViewModel model, Guid orderID)
        {
            var orderDetails = _mapper.Map<OrderDetail>(model);

            orderDetails.OrderId = orderID;
            orderDetails.TotalPrice = (decimal?)(model.Price * model.Amount);

            await CreateAsync(orderDetails);
            return model;
        }
    }
}
