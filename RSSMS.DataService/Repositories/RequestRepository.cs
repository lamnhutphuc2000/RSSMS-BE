using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Models;

namespace RSSMS.DataService.Repositories
{
    public interface IRequestRepository : IBaseRepository<Request>
    {

    }
    class RequestRepository : BaseRepository<Request>, IRequestRepository
    {
        public RequestRepository(DbContext dbContext) : base(dbContext)
        {
        }
    }
}
