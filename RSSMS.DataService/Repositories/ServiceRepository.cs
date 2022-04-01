using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Models;

namespace RSSMS.DataService.Repositories
{
    public interface IServiceRepository : IBaseRepository<Service>
    {

    }
    public class ServiceRepository : BaseRepository<Service>, IServiceRepository
    {
        public ServiceRepository(DbContext dbContext) : base(dbContext)
        {
        }
    }
}
