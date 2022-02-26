using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.Repositories
{
    public interface IServicesRepository : IBaseRepository<Service>
    {

    }
    public class ServicesRepository : BaseRepository<Service>, IServicesRepository
    {
        public ServicesRepository(DbContext dbContext) : base(dbContext)
        {
        }
    }
}
