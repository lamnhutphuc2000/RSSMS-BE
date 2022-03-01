using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.Repositories
{
    public interface IRolesRepository : IBaseRepository<Role>
    {

    }
    public class RolesRepository : BaseRepository<Role>, IRolesRepository
    {
        public RolesRepository(DbContext dbContext) : base(dbContext)
        {
        }
    }
}
