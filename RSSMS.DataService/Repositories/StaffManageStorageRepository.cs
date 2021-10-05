using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.Repositories
{
    public interface IStaffManageStorageRepository : IBaseRepository<StaffManageStorage>
    {

    }
    public class StaffManageStorageRepository : BaseRepository<StaffManageStorage>, IStaffManageStorageRepository
    {
        public StaffManageStorageRepository(DbContext dbContext) : base(dbContext)
        {
        }
    }
}
