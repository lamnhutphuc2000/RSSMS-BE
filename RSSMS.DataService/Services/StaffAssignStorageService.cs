using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Models;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.Responses;
using RSSMS.DataService.UnitOfWorks;
using RSSMS.DataService.ViewModels.StaffAssignStorage;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RSSMS.DataService.Services
{
    public interface IStaffAssignStorageService : IBaseService<StaffAssignStorage>
    {
        Task<StaffAssignStorageCreateViewModel> Create(StaffAssignStorageCreateViewModel model);
        Task<StaffAssignStorageCreateViewModel> AssignStaffToStorage(StaffAssignInStorageViewModel model, string accessToken);
    }
    public class StaffAssignStorageService : BaseService<StaffAssignStorage>, IStaffAssignStorageService
    {
        private readonly IMapper _mapper;
        public StaffAssignStorageService(IUnitOfWork unitOfWork, IStaffAssignStorageRepository repository, IMapper mapper) : base(unitOfWork, repository)
        {
            _mapper = mapper;
        }

        public async Task<StaffAssignStorageCreateViewModel> AssignStaffToStorage(StaffAssignInStorageViewModel model, string accessToken)
        {
            var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
            var uid = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);

            var staffAssigned = model.UserAssigned;
            var staffUnAssigned = model.UserUnAssigned;
            if (staffAssigned != null)
            {
                var managerAssigned = staffAssigned.Where(a => a.RoleName == "Manager").ToList();
                if (managerAssigned.Count > 0)
                {
                    if (managerAssigned.Count > 1) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "More than 1 manager assigned to this storage");
                    var managerInStorage = Get(x => x.RoleName == "Manager" && x.StorageId == model.StorageId && x.IsActive == true).FirstOrDefault();
                    if (managerInStorage != null)
                    {
                        if (staffAssigned.Where(x => x.UserId == managerInStorage.StaffId).FirstOrDefault() == null && staffUnAssigned.Where(x => x.UserId == managerInStorage.StaffId).FirstOrDefault() == null)
                            throw new ErrorResponse((int)HttpStatusCode.BadRequest, "More than 1 manager assigned to this storage");
                    }
                }

                foreach (var staff in staffAssigned)
                {
                    var staffManageStorage = await Get(x => x.StaffId == staff.UserId && x.RoleName != "Manager" && x.StorageId != model.StorageId && x.IsActive == true).FirstOrDefaultAsync();
                    if (staffManageStorage != null)
                    {
                        throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Staff has assigned to a storage before");
                    }

                }

                if (staffUnAssigned != null)
                {
                    var staffs = Get(x => x.StorageId == model.StorageId && x.IsActive == true).ToList().Where(x => staffUnAssigned.Any(a => a.UserId == x.StaffId)).ToList();

                    foreach (var staff in staffs)
                    {
                        staff.IsActive = false;
                        staff.ModifiedBy = uid;
                        await UpdateAsync(staff);
                    }
                }


                foreach (var staff in staffAssigned)
                {
                    var staffAssign = await Get(x => x.StorageId == model.StorageId && x.StaffId == staff.UserId && x.IsActive == true).FirstOrDefaultAsync();
                    if (staffAssign == null)
                    {
                        StaffAssignStorage staffAdd = new StaffAssignStorage
                        {
                            StorageId = model.StorageId,
                            StaffId = staff.UserId,
                            RoleName = staff.RoleName,
                            IsActive = true,
                            CreatedDate = DateTime.Now
                        };
                        await CreateAsync(staffAdd);
                    }
                }
            }

            return null;
        }

        public async Task<StaffAssignStorageCreateViewModel> Create(StaffAssignStorageCreateViewModel model)
        {
            var staffAssignStorage = await Get(x => x.StorageId == model.StorageId && x.StaffId == model.UserId && x.IsActive == true).FirstOrDefaultAsync();
            if (staffAssignStorage != null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "User is assigned to the storage");
            var staffAssigned = _mapper.Map<StaffAssignStorage>(model);
            await CreateAsync(staffAssigned);
            return model;
        }
    }
}
