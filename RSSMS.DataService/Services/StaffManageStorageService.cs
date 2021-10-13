using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Models;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.Responses;
using RSSMS.DataService.UnitOfWorks;
using RSSMS.DataService.ViewModels.StaffManageUser;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RSSMS.DataService.Services
{
    public interface IStaffManageStorageService : IBaseService<StaffManageStorage>
    {
        Task<StaffManageStorageCreateViewModel> Create(StaffManageStorageCreateViewModel model);
        Task<StaffManageStorageCreateViewModel> AssignStaffToStorage(StaffAssignViewModel model);
    }
    class StaffManageStorageService : BaseService<StaffManageStorage>, IStaffManageStorageService
    {
        private readonly IMapper _mapper;
        public StaffManageStorageService(IUnitOfWork unitOfWork, IStaffManageStorageRepository repository, IMapper mapper) : base(unitOfWork, repository)
        {
            _mapper = mapper;
        }

        public async Task<StaffManageStorageCreateViewModel> AssignStaffToStorage(StaffAssignViewModel model)
        {
            var staffAssigned = model.UserAssigned;
            var staffUnAssigned = model.UserUnAssigned;
            if (staffAssigned != null)
            {
                var managerAssigned = staffAssigned.Where(a => a.RoleName == "Manager");
                if (managerAssigned != null)
                {
                    if (managerAssigned.Count() > 1) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "More than 1 manager assigned to this storage");
                    var managerInStorage = Get(x => x.StorageId == model.StorageId && x.RoleName == "Manager").FirstOrDefault();
                    if (managerInStorage != null)
                    {
                        if (managerInStorage.UserId != managerAssigned.FirstOrDefault().UserId)
                        {
                            throw new ErrorResponse((int)HttpStatusCode.BadRequest, "This storage has assigned manager");
                        }
                    }
                }

                foreach (var staff in staffAssigned)
                {
                    var staffManageStorage = await Get(x => x.UserId == staff.UserId && x.RoleName != "Manager").FirstOrDefaultAsync();
                    if (staffManageStorage != null)
                    {
                        throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Staff has assigned to a storage before");
                    }

                }



                if (staffUnAssigned != null)
                {
                    var staffs = Get(x => x.StorageId == model.StorageId).ToList().Where(x => staffUnAssigned.Any(a => a.UserId == x.UserId)).ToList();

                    foreach (var staff in staffs)
                    {
                        await DeleteAsync(staff);
                    }
                }






                foreach (var staff in staffAssigned)
                {
                    var staffAssign = await Get(x => x.StorageId == model.StorageId && x.UserId == staff.UserId).FirstOrDefaultAsync();
                    if (staffAssign == null)
                    {
                        StaffManageStorage staffAdd = new StaffManageStorage
                        {
                            StorageId = model.StorageId,
                            UserId = staff.UserId,
                            RoleName = staff.RoleName,
                            StorageName = model.StorageName
                        };
                        await CreateAsync(staffAdd);
                    }
                }
            }




            return null;
        }

        public async Task<StaffManageStorageCreateViewModel> Create(StaffManageStorageCreateViewModel model)
        {
            var StaffManageStorages = await Get(x => x.StorageId == model.StorageId && x.UserId == model.UserId).FirstOrDefaultAsync();
            if (StaffManageStorages != null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "User is assigned to the storage");
            var staffAssigned = _mapper.Map<StaffManageStorage>(model);
            await CreateAsync(staffAssigned);
            return model;
        }
    }
}
