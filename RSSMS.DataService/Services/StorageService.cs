using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Constants;
using RSSMS.DataService.Models;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.Responses;
using RSSMS.DataService.UnitOfWorks;
using RSSMS.DataService.Utilities;
using RSSMS.DataService.ViewModels.Areas;
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
        Task<List<StorageViewModel>> GetStorageWithUsage(Guid? storageId);
    }
    public class StorageService : BaseService<Storage>, IStorageService
    {
        private readonly IMapper _mapper;
        private readonly IStaffAssignStorageService _staffAssignStoragesService;
        private readonly IAreaService _areaService;
        private readonly IFirebaseService _firebaseService;
        public StorageService(IUnitOfWork unitOfWork, IStorageRepository repository, IMapper mapper, IStaffAssignStorageService staffAssignStoragesService, IAreaService areaService, IFirebaseService firebaseService) : base(unitOfWork, repository)
        {
            _mapper = mapper;
            _staffAssignStoragesService = staffAssignStoragesService;
            _areaService = areaService;
            _firebaseService = firebaseService;
        }

        public async Task<StorageViewModel> Create(StorageCreateViewModel model)
        {
            try
            {
                var storage = _mapper.Map<Storage>(model);
                var image = model.Image;
                await CreateAsync(storage);
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
                await UpdateAsync(storage);
                return _mapper.Map<StorageViewModel>(storage);
            }
            catch (ErrorResponse e)
            {
                throw new ErrorResponse((int)e.Error.Code, e.Error.Message);
            }
            catch (Exception e)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, e.Message);
            }
            

        }

        public async Task<StorageViewModel> Delete(Guid id)
        {
            try
            {
                var entity = await Get(x => x.Id == id && x.IsActive).Include(a => a.Areas.Where(area => area.IsActive)).FirstOrDefaultAsync();
                if (entity == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Storage id not found");
                var areas = entity.Areas;
                foreach (var area in areas)
                    if (_areaService.CheckIsUsed(area.Id)) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Storage is in used");
                entity.IsActive = false;
                await UpdateAsync(entity);
                return _mapper.Map<StorageViewModel>(entity);
            }
            catch (ErrorResponse e)
            {
                throw new ErrorResponse((int)e.Error.Code, e.Error.Message);
            }
            catch (Exception e)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, e.Message);
            }
            
        }

        public async Task<DynamicModelResponse<StorageViewModel>> GetAll(StorageViewModel model, List<int> types, string[] fields, int page, int size, string accessToken)
        {
            try
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
            catch (ErrorResponse e)
            {
                throw new ErrorResponse((int)e.Error.Code, e.Error.Message);
            }
            catch (Exception e)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, e.Message);
            }
            
        }

        public async Task<StorageDetailViewModel> GetById(Guid id, string accessToken)
        {
            try
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
            catch (ErrorResponse e)
            {
                throw new ErrorResponse((int)e.Error.Code, e.Error.Message);
            }
            catch (Exception e)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, e.Message);
            }
            
        }

        public async Task<StorageUpdateViewModel> Update(Guid id, StorageUpdateViewModel model)
        {
            try
            {
                if (id != model.Id) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Storage Id not matched");

                var entity = await Get(storage => storage.Id == id && storage.IsActive).Include(a => a.Areas).FirstOrDefaultAsync();
                if (entity == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Storage not found");

                var areas = entity.Areas.Where(area => area.IsActive);
                foreach (var area in areas)
                    if (_areaService.CheckIsUsed(area.Id)) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Storage is in used");

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
            catch (ErrorResponse e)
            {
                throw new ErrorResponse((int)e.Error.Code, e.Error.Message);
            }
            catch (Exception e)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, e.Message);
            }
            
        }


        public async Task<List<StorageViewModel>> GetStorageWithUsage(Guid? storageId)
        {
            List<StorageViewModel> result = new List<StorageViewModel>();
            IQueryable<Storage> storages = Get(storage => storage.IsActive).Include(storage => storage.Areas);
            if (storageId != null) storages = storages.Where(storage => storage.Id == storageId).Include(storage => storage.Areas);
            if (storages.ToList().Count == 0) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Not enough storage");
            var storageList = storages.ToList();
            result = storages.ProjectTo<StorageViewModel>(_mapper.ConfigurationProvider).ToList();
            for (int i = 0; i <storageList.Count; i++)
            {
                List<AreaDetailViewModel> areasInStorage = new List<AreaDetailViewModel>();
                var areas = storageList[i].Areas.Where(area => area.IsActive).ToList();
                for(int j = 0; j < areas.Count; j ++)
                {
                    areasInStorage.Add(await _areaService.GetById(areas[j].Id));
                }
                result[i].Areas = areasInStorage;
            }
            return result;
        }
    }
}
