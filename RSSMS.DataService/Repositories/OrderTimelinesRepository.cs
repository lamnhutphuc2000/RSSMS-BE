using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Models;

namespace RSSMS.DataService.Repositories
{
    public interface IOrderTimelinesRepository : IBaseRepository<OrderTimeline>
    {

    }
    public class OrderTimelinesRepository : BaseRepository<OrderTimeline>, IOrderTimelinesRepository
    {
        public OrderTimelinesRepository(DbContext dbContext) : base(dbContext)
        {
        }
    }
}
