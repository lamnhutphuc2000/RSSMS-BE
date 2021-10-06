using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Models;

namespace RSSMS.DataService.Repositories
{
    public interface IAreaRepository : IBaseRepository<Area>
    {

    }
    public class AreaRepository : BaseRepository<Area>, IAreaRepository
    {
        public AreaRepository(DbContext dbContext) : base(dbContext)
        {
        }
    }
}
