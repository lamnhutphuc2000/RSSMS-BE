using RSSMS.DataService.Models;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.UnitOfWorks;

namespace RSSMS.DataService.Services
{
    public interface IStaffAssignStorageService : IBaseService<StaffAssignStorage>
    {
    }
    public class StaffAssignStorageService : BaseService<StaffAssignStorage>, IStaffAssignStorageService
    {
        public StaffAssignStorageService(IUnitOfWork unitOfWork, IStaffAssignStorageRepository repository) : base(unitOfWork, repository)
        {
        }

    }
}
