using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Models;

namespace RSSMS.DataService.Repositories
{
    public interface IFloorsRepository : IBaseRepository<Floor>
    {

    }
    public class FloorsRepository : BaseRepository<Floor>, IFloorsRepository
    {
        public FloorsRepository(DbContext dbContext) : base(dbContext)
        {
        }
    }
}
