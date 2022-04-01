using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Models;

namespace RSSMS.DataService.Repositories
{
    public interface IStaffAssignStoragesRepository : IBaseRepository<StaffAssignStorage>
    {

    }
    public class StaffAssignStorageRepository : BaseRepository<StaffAssignStorage>, IStaffAssignStoragesRepository
    {
        public StaffAssignStorageRepository(DbContext dbContext) : base(dbContext)
        {
        }
    }
}
