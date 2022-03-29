using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Constants;
using RSSMS.DataService.Models;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.Responses;
using RSSMS.DataService.UnitOfWorks;
using RSSMS.DataService.Utilities;
using RSSMS.DataService.ViewModels.StaffAssignStorage;
using RSSMS.DataService.ViewModels.Storages;
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
        Task<StorageDetailViewModel> GetById(Guid id, string accessToken);
        Task<StorageViewModel> Create(StorageCreateViewModel model);
        Task<StorageUpdateViewModel> Update(Guid id, StorageUpdateViewModel model);
        Task<StorageViewModel> Delete(Guid id);
    }
    public class StorageService : BaseService<Storage>, IStorageService
    {
        private readonly IMapper _mapper;
        private readonly IStaffAssignStoragesService _staffAssignStoragesService;
        private readonly IAreaService _areaService;
        private readonly IFirebaseService _firebaseService;
        public StorageService(IUnitOfWork unitOfWork, IStorageRepository repository, IMapper mapper, IStaffAssignStoragesService staffAssignStoragesService, IAreaService areaService, IFirebaseService firebaseService) : base(unitOfWork, repository)
        {
            _mapper = mapper;
            _staffAssignStoragesService = staffAssignStoragesService;
            _areaService = areaService;
            _firebaseService = firebaseService;
        }

        public async Task<StorageViewModel> Create(StorageCreateViewModel model)
        {
            var storage = _mapper.Map<Storage>(model);
            var image = model.Image;
            if (image != null)
            {
                if (image.File != null)
                {
                    var url = await _firebaseService.UploadImageToFirebase(image.File, "storages", storage.Id, "avatar");
                    if (url != null)
                    {
                        storage.ImageUrl = url;
                    }

                }
            }
            await CreateAsync(storage);
            //if (model.ListStaff != null)
            //{
            //    foreach (UserListStaffViewModel staffAssigned in model.ListStaff)
            //    {
            //        StaffAssignStorageCreateViewModel staffAssignModel = new StaffAssignStorageCreateViewModel
            //        {
            //            StorageId = storage.Id,
            //            UserId = staffAssigned.Id
            //        };
            //        await _staffAssignStoragesService.Create(staffAssignModel);
            //    }
            //}
            return _mapper.Map<StorageViewModel>(storage);

        }

        public async Task<StorageViewModel> Delete(Guid id)
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
            var userId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);
            var role = secureToken.Claims.First(claim => claim.Type.Contains("role")).Value;

            var storages = Get(x => x.IsActive == true).Include(a => a.StaffAssignStorages.Where(s => s.RoleName == "Manager" && s.IsActive == true)).ThenInclude(a => a.Staff).ProjectTo<StorageViewModel>(_mapper.ConfigurationProvider).DynamicFilter(model);



            if (types.Count > 0)
                storages = storages.Where(x => types.Contains((int)x.Type));


            if (role == "Manager")
            {
                var storagesManagerManage = _staffAssignStoragesService.Get(x => x.StaffId == userId && x.IsActive == true).Select(x => x.StorageId).ToList();
                storages = storages.Where(x => storagesManagerManage.Contains((Guid)x.Id));
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
                Data = await result.Item2.ToListAsync()
            };
            return rs;
        }

        public async Task<StorageDetailViewModel> GetById(Guid id, string accessToken)
        {
            var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
            var userId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);
            var role = secureToken.Claims.First(claim => claim.Type.Contains("role")).Value;

            var result = await Get(x => x.Id == id && x.IsActive == true)
                                .Include(a => a.StaffAssignStorages.Where(s => s.RoleName == "Manager" && s.IsActive == true))
                                .ThenInclude(a => a.Staff).ProjectTo<StorageDetailViewModel>(_mapper.ConfigurationProvider).FirstOrDefaultAsync();
            if (result == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Storage id not found");

            if (role == "Office Staff")
            {
                result = await Get(x => x.Id == id && x.IsActive == true)
                .Include(a => a.StaffAssignStorages.Where(s => s.RoleName == "Office Staff" && s.IsActive == true && s.StaffId == userId))
                .ThenInclude(a => a.Staff).ProjectTo<StorageDetailViewModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
                //if (result.StaffManageStorages == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Office Staff not manage this storage");

            }

            //if (result != null && result.StaffManageStorages != null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Office Staff not manage this storage");

            return result;
        }

        public async Task<StorageUpdateViewModel> Update(Guid id, StorageUpdateViewModel model)
        {
            if (id != model.Id) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Storage Id not matched");

            var entity = await Get(x => x.Id == id && x.IsActive == true).Include(a => a.Areas).FirstOrDefaultAsync();
            if (entity == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Storage not found");

            var areas = entity.Areas;
            foreach (var area in areas)
            {
                if (_areaService.CheckIsUsed(area.Id)) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Storage is in used");
            }

            var updateEntity = _mapper.Map(model, entity);

            var image = model.Image;
            if (image != null)
            {
                if (image.File != null)
                {
                    var url = await _firebaseService.UploadImageToFirebase(image.File, "storages", id, "avatar");
                    if (url != null) updateEntity.ImageUrl = url;
                }
            }
            await UpdateAsync(updateEntity);

            return _mapper.Map<StorageUpdateViewModel>(updateEntity);
        }
    }
}
