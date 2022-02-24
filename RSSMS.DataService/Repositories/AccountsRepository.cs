using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Models;
using System;
using System.Collections.Generic;
using System.Text;

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
