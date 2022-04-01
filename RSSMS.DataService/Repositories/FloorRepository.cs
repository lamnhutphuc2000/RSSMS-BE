using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Models;

namespace RSSMS.DataService.Repositories
{
    public interface IFloorRepository : IBaseRepository<Floor>
    {

    }
    public class FloorRepository : BaseRepository<Floor>, IFloorRepository
    {
        public FloorRepository(DbContext dbContext) : base(dbContext)
        {
        }
    }
}
