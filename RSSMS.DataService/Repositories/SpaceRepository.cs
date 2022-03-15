using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Models;

namespace RSSMS.DataService.Repositories
{
    public interface ISpaceRepository : IBaseRepository<Space>
    {

    }
    public class SpaceRepository : BaseRepository<Space>, ISpaceRepository
    {
        public SpaceRepository(DbContext dbContext) : base(dbContext)
        {
        }
    }
}
