using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Models;

namespace RSSMS.DataService.Repositories
{
    public interface IAccountsRepository : IBaseRepository<Account>
    {

    }
    public class AccountRepository : BaseRepository<Account>, IAccountsRepository
    {
        public AccountRepository(DbContext dbContext) : base(dbContext)
        {
        }
    }
}
