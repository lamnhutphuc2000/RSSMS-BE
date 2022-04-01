using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Models;

namespace RSSMS.DataService.Repositories
{
    public interface IStaffAssignStorageRepository : IBaseRepository<StaffAssignStorage>
    {

    }
    public class StaffAssignStorageRepository : BaseRepository<StaffAssignStorage>, IStaffAssignStorageRepository
    {
        public StaffAssignStorageRepository(DbContext dbContext) : base(dbContext)
        {
        }
    }
}
