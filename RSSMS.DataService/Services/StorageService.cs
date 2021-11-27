using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Constants;
using RSSMS.DataService.Models;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.Responses;
using RSSMS.DataService.UnitOfWorks;
using RSSMS.DataService.Utilities;
using RSSMS.DataService.ViewModels.StaffManageUser;
using RSSMS.DataService.ViewModels.Storages;
using RSSMS.DataService.ViewModels.Users;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
namespace RSSMS.DataService.Services
{
    public interface IStorageService : IBaseService<Storage>
    {
        Task<DynamicModelResponse<StorageViewModel>> GetAll(StorageViewModel model, List<int> types, string[] fields, int page, int size, string accessToken);
        Task<StorageGetIdViewModel> GetById(int id);
        Task<StorageViewModel> Create(StorageCreateViewModel model);
        Task<StorageUpdateViewModel> Update(int id, StorageUpdateViewModel model);
        Task<StorageViewModel> Delete(int id);
    }
    public class StorageService : BaseService<Storage>, IStorageService
    {
        private readonly IMapper _mapper;
        private readonly IStaffManageStorageService _staffManageStorageService;
        public StorageService(IUnitOfWork unitOfWork, IStorageRepository repository, IMapper mapper, IStaffManageStorageService staffManageStorageService) : base(unitOfWork, repository)
        {
            _mapper = mapper;
            _staffManageStorageService = staffManageStorageService;
        }

        public async Task<StorageViewModel> Create(StorageCreateViewModel model)
        {
            var storage = _mapper.Map<Storage>(model);
            await CreateAsync(storage);

            foreach (UserListStaffViewModel staffAssigned in model.ListStaff)
            {
                var staffAssignModel = _mapper.Map<StaffManageStorageCreateViewModel>(storage);
                staffAssignModel.UserId = staffAssigned.Id;
                await _staffManageStorageService.Create(staffAssignModel);
            }

            return _mapper.Map<StorageViewModel>(storage); ;

        }

        public async Task<StorageViewModel> Delete(int id)
        {
            var entity = await GetAsync<int>(id);
            if (entity == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Storage id not found");
            entity.IsActive = false;
            await UpdateAsync(entity);
            return _mapper.Map<StorageViewModel>(entity);
        }

        public async Task<DynamicModelResponse<StorageViewModel>> GetAll(StorageViewModel model, List<int> types, string[] fields, int page, int size, string accessToken)
        {
            var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
            var userId = Int32.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);
            var role = secureToken.Claims.First(claim => claim.Type.Contains("role")).Value;

            var storages = Get(x => x.IsActive == true).Include(a => a.StaffManageStorages.Where(s => s.RoleName == "Manager"))
                .ThenInclude(a => a.User).ProjectTo<StorageViewModel>(_mapper.ConfigurationProvider).DynamicFilter(model);



            if (types.Count > 0)
            {
                storages = storages.Where(x => types.Contains((int)x.Type));
            }


            if (role == "Manager")
            {
                var storagesManagerManage = _staffManageStorageService.Get(x => x.UserId == userId).Select(x => x.StorageId).ToList();
                storages = storages.Where(x => storagesManagerManage.Contains((int)x.Id));
            }


            var result = storages.PagingIQueryable(page, size, CommonConstant.LimitPaging, CommonConstant.DefaultPaging);

            if (result.Item2.ToList().Count < 1) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Can not found");
            var rs = new DynamicModelResponse<StorageViewModel>
            {
                Metadata = new PagingMetaData
                {
                    Page = page,
                    Size = size,
                    Total = result.Item1,
                    TotalPage = (int)Math.Ceiling((double)result.Item1 / size)
                },
                Data = result.Item2.ToList()
            };
            return rs;
        }

        public async Task<StorageGetIdViewModel> GetById(int id)
        {
            var result = await Get(x => x.Id == id && x.IsActive == true)
                .Include(a => a.StaffManageStorages.Where(s => s.RoleName == "Manager"))
                .ThenInclude(a => a.User).ProjectTo<StorageGetIdViewModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            if (result == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Storage id not found");

            return result;
        }

        public async Task<StorageUpdateViewModel> Update(int id, StorageUpdateViewModel model)
        {
            if (id != model.Id) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Storage Id not matched");

            var entity = await GetAsync(id);
            if (entity == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Storage not found");

            var updateEntity = _mapper.Map(model, entity);
            await UpdateAsync(updateEntity);

            return _mapper.Map<StorageUpdateViewModel>(updateEntity);
        }
    }
}
