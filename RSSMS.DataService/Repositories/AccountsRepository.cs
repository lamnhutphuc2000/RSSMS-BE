using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Models;

namespace RSSMS.DataService.Repositories
{
    public interface IAccountsRepository : IBaseRepository<Account>
    {

    }
    public class AccountsRepository : BaseRepository<Account>, IAccountsRepository
    {
        public AccountsRepository(DbContext dbContext) : base(dbContext)
        {
        }
    }
}
