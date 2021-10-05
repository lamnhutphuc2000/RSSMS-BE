using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Models;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.Responses;
using RSSMS.DataService.UnitOfWorks;
using RSSMS.DataService.ViewModels.StaffManageUser;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
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
            if(staffAssigned != null)
            {
                var staffs = await Get(x => staffUnAssigned.Contains(x.UserId) && x.StorageId == model.StorageId).ToListAsync();
                foreach (var staff in staffs)
                {
                    await DeleteAsync(staff);
                }
            }
            
            if(staffUnAssigned != null)
            {
                foreach (var staff in staffAssigned)
                {
                    var staffAssign = await Get(x => x.StorageId == model.StorageId && x.UserId == staff).FirstOrDefaultAsync();
                    if (staffAssign == null)
                    {
                        StaffManageStorage staffAdd = new StaffManageStorage();
                        staffAdd.StorageId = model.StorageId;
                        staffAdd.UserId = staff;
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
