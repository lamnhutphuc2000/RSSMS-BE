using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Models;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.UnitOfWorks;
using RSSMS.DataService.ViewModels.Orders;
using System;
using System.Threading.Tasks;

namespace RSSMS.DataService.Services
{
    public interface IOrderService : IBaseService<Order>
    {
        Task<OrderStorageViewModel> GetSelfStorageOrderInfo(int id);
    }
    class OrderService : BaseService<Order>, IOrderService
    {
        private readonly IMapper _mapper;
        public OrderService(IUnitOfWork unitOfWork, IOrderRepository repository, IMapper mapper) : base(unitOfWork, repository)
        {
            _mapper = mapper;
        }

        public async Task<OrderStorageViewModel> GetSelfStorageOrderInfo(int id)
        {
            var orderSelfStorageInfo = await Get(x => x.Id == id).ProjectTo<OrderStorageViewModel>(_mapper.ConfigurationProvider).FirstOrDefaultAsync();
            if (orderSelfStorageInfo == null) return null; 

            //Count Remaining Time
           /* if (DateTime.Compare((DateTime)order.ReturnDate, DateTime.Now) > 0)
            {
                TimeSpan difference = (DateTime)order.ReturnDate - DateTime.Now;
                orderSelfStorageInfo.RemainingTime = difference.Days;
            }
            else
            {
                orderSelfStorageInfo.RemainingTime = 0;
            }*/
            return orderSelfStorageInfo;
            
        }
    }
}
