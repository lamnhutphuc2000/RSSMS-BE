using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Models;

namespace RSSMS.DataService.Repositories
{
    public interface IStorageRepository : IBaseRepository<Storage>
    {

    }
    public class StorageRepository : BaseRepository<Storage>, IStorageRepository
    {
        public StorageRepository(DbContext dbContext) : base(dbContext)
        {
        }
    }
}
