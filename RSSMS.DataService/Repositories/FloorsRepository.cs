using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.Repositories
{
    public interface IFloorsRepository : IBaseRepository<Floor>
    {

    }
    public class FloorsRepository : BaseRepository<Floor>, IFloorsRepository
    {
        public FloorsRepository(DbContext dbContext) : base(dbContext)
        {
        }
    }
}
