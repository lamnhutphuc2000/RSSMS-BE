using System;
using System.Threading.Tasks;
using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.UnitOfWorks;
using Microsoft.EntityFrameworkCore;

namespace RSSMS.DataService.Services
{
    public interface IOrderService : IBaseService<Order>
    {
        Task<int> GetTimeRemaining(int id);
    }
    class OrderService : BaseService<Order>, IOrderService
    {
        public OrderService(IUnitOfWork unitOfWork, IOrderRepository repository, IMapper mapper) : base(unitOfWork, repository)
        {
        }

        public async Task<int> GetTimeRemaining(int id)
        {
            var order = await Get(x => x.Id == id).FirstOrDefaultAsync();
            if(order != null)
            {
                DateTime returnDate = (DateTime)order.ReturnDate;
                if(DateTime.Compare(returnDate, DateTime.Now) > 0)
                {
                    TimeSpan difference = returnDate - DateTime.Now;
                    return difference.Days;
                }
            }
            return 0;
        }
    }
}
