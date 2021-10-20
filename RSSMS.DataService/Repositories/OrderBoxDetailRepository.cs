using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Models;

namespace RSSMS.DataService.Repositories
{
    public interface IOrderBoxDetailRepository : IBaseRepository<OrderBoxDetail>
    {

    }
    public class OrderBoxDetailRepository : BaseRepository<OrderBoxDetail>, IOrderBoxDetailRepository
    {
        public OrderBoxDetailRepository(DbContext dbContext) : base(dbContext)
        {
        }
    }
}
