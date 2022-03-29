using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Models;

namespace RSSMS.DataService.Repositories
{
    public interface IStaffAssignStoragesRepository : IBaseRepository<StaffAssignStorage>
    {

    }
    public class StaffAssignStoragesRepository : BaseRepository<StaffAssignStorage>, IStaffAssignStoragesRepository
    {
        public StaffAssignStoragesRepository(DbContext dbContext) : base(dbContext)
        {
        }
    }
}
