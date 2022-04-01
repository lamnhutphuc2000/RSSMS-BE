using RSSMS.DataService.Models;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.UnitOfWorks;

namespace RSSMS.DataService.Services
{

    public interface IOrderDetailService : IBaseService<OrderDetail>
    {
    }
    class OrderDetailService : BaseService<OrderDetail>, IOrderDetailService
    {
        public OrderDetailService(IUnitOfWork unitOfWork, IOrderDetailRepository repository) : base(unitOfWork, repository)
        {
        }
    }
}
