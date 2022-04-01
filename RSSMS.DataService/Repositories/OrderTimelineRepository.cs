using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Models;

namespace RSSMS.DataService.Repositories
{
    public interface IOrderTimelineRepository : IBaseRepository<OrderTimeline>
    {

    }
    public class OrderTimelineRepository : BaseRepository<OrderTimeline>, IOrderTimelineRepository
    {
        public OrderTimelineRepository(DbContext dbContext) : base(dbContext)
        {
        }
    }
}
