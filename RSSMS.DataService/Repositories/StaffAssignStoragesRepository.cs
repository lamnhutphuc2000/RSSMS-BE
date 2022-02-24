using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Models;
using System;
using System.Collections.Generic;
using System.Text;

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
