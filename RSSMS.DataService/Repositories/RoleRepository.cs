using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Models;

namespace RSSMS.DataService.Repositories
{
    public interface IRolesRepository : IBaseRepository<Role>
    {

    }
    public class RoleRepository : BaseRepository<Role>, IRolesRepository
    {
        public RoleRepository(DbContext dbContext) : base(dbContext)
        {
        }
    }
}
