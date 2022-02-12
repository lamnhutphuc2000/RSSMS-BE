using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Models;

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
