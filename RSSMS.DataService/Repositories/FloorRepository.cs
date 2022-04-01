using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Models;

namespace RSSMS.DataService.Repositories
{
    public interface IFloorsRepository : IBaseRepository<Floor>
    {

    }
    public class FloorRepository : BaseRepository<Floor>, IFloorsRepository
    {
        public FloorRepository(DbContext dbContext) : base(dbContext)
        {
        }
    }
}
