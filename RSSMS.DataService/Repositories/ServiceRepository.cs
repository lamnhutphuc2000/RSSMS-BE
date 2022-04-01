using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Models;

namespace RSSMS.DataService.Repositories
{
    public interface IServicesRepository : IBaseRepository<Service>
    {

    }
    public class ServiceRepository : BaseRepository<Service>, IServicesRepository
    {
        public ServiceRepository(DbContext dbContext) : base(dbContext)
        {
        }
    }
}
