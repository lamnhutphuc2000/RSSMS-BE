using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Models;

namespace RSSMS.DataService.Repositories
{
    public interface IOrderTimelinesRepository : IBaseRepository<OrderTimeline>
    {

    }
    public class OrderTimelineRepository : BaseRepository<OrderTimeline>, IOrderTimelinesRepository
    {
        public OrderTimelineRepository(DbContext dbContext) : base(dbContext)
        {
        }
    }
}
