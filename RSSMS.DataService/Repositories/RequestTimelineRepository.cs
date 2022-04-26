using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Models;

namespace RSSMS.DataService.Repositories
{
    public interface IRequestTimelineRepository : IBaseRepository<RequestTimeline>
    {

    }
    public class RequestTimelineRepository : BaseRepository<RequestTimeline>, IRequestTimelineRepository
    {
        public RequestTimelineRepository(DbContext dbContext) : base(dbContext)
        {
        }
    }
}
