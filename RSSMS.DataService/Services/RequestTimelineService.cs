using RSSMS.DataService.Models;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.UnitOfWorks;
using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.Services
{
    public interface IRequestTimelineService : IBaseService<RequestTimeline>
    {
    }
    public class RequestTimelineService : BaseService<RequestTimeline>, IRequestTimelineService
    {
        public RequestTimelineService(IUnitOfWork unitOfWork, IRequestTimelineRepository repository ) : base(unitOfWork, repository)
        {
        }


    }
}
