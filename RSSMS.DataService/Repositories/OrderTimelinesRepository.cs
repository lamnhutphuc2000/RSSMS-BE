using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.Repositories
{
    public interface IOrderTimelinesRepository : IBaseRepository<OrderTimeline>
    {

    }
    public class OrderTimelinesRepository : BaseRepository<OrderTimeline>, IOrderTimelinesRepository
    {
        public OrderTimelinesRepository(DbContext dbContext) : base(dbContext)
        {
        }
    }
}
