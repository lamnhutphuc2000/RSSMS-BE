using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Models;

namespace RSSMS.DataService.Repositories
{
    public interface IScheduleRepository : IBaseRepository<Schedule>
    {

    }
    public class ScheduleRepository : BaseRepository<Schedule>, IScheduleRepository
    {
        public ScheduleRepository(DbContext dbContext) : base(dbContext)
        {
        }
    }
}
