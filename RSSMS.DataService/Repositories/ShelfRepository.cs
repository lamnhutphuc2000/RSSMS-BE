using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Models;

namespace RSSMS.DataService.Repositories
{
    public interface IShelfRepository : IBaseRepository<Shelf>
    {

    }
    public class ShelfRepository : BaseRepository<Shelf>, IShelfRepository
    {
        public ShelfRepository(DbContext dbContext) : base(dbContext)
        {
        }
    }
}
