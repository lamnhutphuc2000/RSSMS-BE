using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.Repositories
{
    public interface IBoxOrderDetailRepository : IBaseRepository<BoxOrderDetail>
    {

    }
    public class BoxOrderDetailRepository : BaseRepository<BoxOrderDetail>, IBoxOrderDetailRepository
    {
        public BoxOrderDetailRepository(DbContext dbContext) : base(dbContext)
        {
        }
    }
}
