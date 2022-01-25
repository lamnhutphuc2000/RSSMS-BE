using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.Repositories
{
    public interface IOrderHistoryExtensionRepository : IBaseRepository<OrderHistoryExtension>
    {

    }
    public class OrderHistoryExtensionRepository : BaseRepository<OrderHistoryExtension>, IOrderHistoryExtensionRepository
    {
        public OrderHistoryExtensionRepository(DbContext dbContext) : base(dbContext)
        {
        }
    }
}
