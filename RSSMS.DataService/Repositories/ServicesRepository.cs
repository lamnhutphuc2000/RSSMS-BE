using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Models;

namespace RSSMS.DataService.Repositories
{
    public interface IServicesRepository : IBaseRepository<Service>
    {

    }
    public class ServicesRepository : BaseRepository<Service>, IServicesRepository
    {
        public ServicesRepository(DbContext dbContext) : base(dbContext)
        {
        }
    }
}
