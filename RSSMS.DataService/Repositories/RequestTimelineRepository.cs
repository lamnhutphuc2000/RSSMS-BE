using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Models;
using System;
using System.Collections.Generic;
using System.Text;

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
