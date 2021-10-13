using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Models;

namespace RSSMS.DataService.Repositories
{
    public interface IBoxRepository : IBaseRepository<Box>
    {

    }
    public class BoxRepository : BaseRepository<Box>, IBoxRepository
    {
        public BoxRepository(DbContext dbContext) : base(dbContext)
        {
        }
    }
}
