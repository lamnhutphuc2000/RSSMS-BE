using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Models;

namespace RSSMS.DataService.Repositories
{
    public interface IRolesRepository : IBaseRepository<Role>
    {

    }
    public class RolesRepository : BaseRepository<Role>, IRolesRepository
    {
        public RolesRepository(DbContext dbContext) : base(dbContext)
        {
        }
    }
}
