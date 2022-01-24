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
        Task<StorageGetIdViewModel> GetById(int id, string accessToken);
        Task<StorageViewModel> Create(StorageCreateViewModel model);
        Task<StorageUpdateViewModel> Update(int id, StorageUpdateViewModel model);
        Task<StorageViewModel> Delete(int id);
    }
    public class StorageService : BaseService<Storage>, IStorageService
    {
        private readonly IMapper _mapper;
        private readonly IStaffManageStorageService _staffManageStorageService;
        private readonly IAreaService _areaService;
        private readonly IFirebaseService _firebaseService;
        public StorageService(IUnitOfWork unitOfWork, IStorageRepository repository, IMapper mapper, IStaffManageStorageService staffManageStorageService, IAreaService areaService, IFirebaseService firebaseService) : base(unitOfWork, repository)
        {
            _mapper = mapper;
            _staffManageStorageService = staffManageStorageService;
            _areaService = areaService;
            _firebaseService = firebaseService;
        }

        public async Task<StorageViewModel> Create(StorageCreateViewModel model)
        {
            var storage = _mapper.Map<Storage>(model);
            var images = model.Images;
            storage.Images = null;
            await CreateAsync(storage);

            
            foreach (var avatar in images)
            {
                var url = await _firebaseService.UploadImageToFirebase(avatar.File, "storages", storage.Id, "avatar");
                if (url != null) avatar.Url = url;
            }
            storage.Images = images.AsQueryable().ProjectTo<Image>(_mapper.ConfigurationProvider).ToList();

            await UpdateAsync(storage);

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
            var entity = await Get(x => x.Id == id && x.IsActive == true).Include(a => a.Areas).FirstOrDefaultAsync();
            if (entity == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Storage id not found");
            var areas = entity.Areas;
            foreach (var area in areas)
            {
                if (_areaService.CheckIsUsed(area.Id)) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Storage is in used");
            }
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

        public async Task<StorageGetIdViewModel> GetById(int id, string accessToken)
        {
            var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
            var userId = Int32.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);
            var role = secureToken.Claims.First(claim => claim.Type.Contains("role")).Value;

            var result = await Get(x => x.Id == id && x.IsActive == true)
                .Include(a => a.StaffManageStorages.Where(s => s.RoleName == "Manager"))
                .ThenInclude(a => a.User).ProjectTo<StorageGetIdViewModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            if (role == "Office staff")
            {
                result = await Get(x => x.Id == id && x.IsActive == true)
                .Include(a => a.StaffManageStorages.Where(s => s.RoleName == "Office staff"))
                .ThenInclude(a => a.User).ProjectTo<StorageGetIdViewModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
                if (result != null && result.StaffManageStorages != null)
                {
                    if (result.StaffManageStorages.Where(x => x.UserId == userId).FirstOrDefault() == null)
                    {
                        throw new ErrorResponse((int)HttpStatusCode.NotFound, "Office staff not manage this storage");
                    }
                }

            }



            if (result == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Storage id not found");

            return result;
        }

        public async Task<StorageUpdateViewModel> Update(int id, StorageUpdateViewModel model)
        {
            if (id != model.Id) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Storage Id not matched");

            var entity = await Get(x => x.Id == id && x.IsActive == true).Include(a => a.Areas).FirstOrDefaultAsync();
            if (entity == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Storage not found");

            var areas = entity.Areas;
            foreach (var area in areas)
            {
                if (_areaService.CheckIsUsed(area.Id)) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Storage is in used");
            }

            var images = model.Images;
            foreach (var avatar in images)
            {
                var url = await _firebaseService.UploadImageToFirebase(avatar.File, "storages", id, "avatar");
                if (url != null) avatar.Url = url;
            }
            model.Images = images;

            var updateEntity = _mapper.Map(model, entity);
            await UpdateAsync(updateEntity);

            return _mapper.Map<StorageUpdateViewModel>(updateEntity);
        }
    }
}
