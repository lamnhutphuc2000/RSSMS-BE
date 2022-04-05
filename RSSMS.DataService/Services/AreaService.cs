using _3DBinPacking.Enum;
using _3DBinPacking.Model;
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
using RSSMS.DataService.ViewModels.Spaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RSSMS.DataService.Services
{

    public interface IAreaService : IBaseService<Area>
    {
        Task<DynamicModelResponse<AreaViewModel>> GetByStorageId(Guid id, AreaViewModel model, List<int> types, string[] fields, int page, int size);
        Task<AreaViewModel> Create(AreaCreateViewModel model);
        Task<AreaViewModel> Delete(Guid id);
        Task<AreaViewModel> Update(Guid id, AreaUpdateViewModel model);
        Task<AreaDetailViewModel> GetById(Guid id);
        bool CheckIsUsed(Guid id);
    }
    public class AreaService : BaseService<Area>, IAreaService

    {
        private readonly IMapper _mapper;
        private readonly ISpaceService _spaceService;
        private readonly IUtilService _utilService;
        public AreaService(IUnitOfWork unitOfWork, ISpaceService spaceService,
            IUtilService utilService,
            IAreaRepository repository, IMapper mapper) : base(unitOfWork, repository)
        {
            _mapper = mapper;
            _spaceService = spaceService;
            _utilService = utilService;
        }

        public async Task<AreaViewModel> Create(AreaCreateViewModel model)
        {
            Area areaToCreate = null;
            try
            {
                // Validate input
                _utilService.ValidateString(model.Name,"Area name");

                // Check area name is existed
                var entity = Get(area => area.StorageId == model.StorageId && area.Name == model.Name && area.IsActive).FirstOrDefault();
                if (entity != null) throw new ErrorResponse((int)HttpStatusCode.Conflict, "Area name existed");

                // Create new Area
                areaToCreate = _mapper.Map<Area>(model);
                await CreateAsync(areaToCreate);


                // Check is Storage oversize
                var area = Get(area => area.Id == areaToCreate.Id).Include(area => area.Storage)
                    .ThenInclude(storage => storage.Areas).FirstOrDefault();

                var storage = area.Storage;
                var areaList = storage.Areas.ToList();

                List<Cuboid> cuboids = new List<Cuboid>();
                for (int i = 0; i < areaList.Count; i++)
                    cuboids.Add(new Cuboid((decimal)areaList[i].Width, (decimal)areaList[i].Height, (decimal)areaList[i].Length));

                var parameter = new BinPackParameter(storage.Width, storage.Height, storage.Length, cuboids);

                var binPacker = BinPacker.GetDefault(BinPackerVerifyOption.BestOnly);
                var result = binPacker.Pack(parameter);
                if (result.BestResult.Count > 1)
                {
                    await DeleteAsync(areaToCreate);
                    throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Storage size is overload");
                }



                //var areaCreated = Get(area => area.Id == areaToCreate.Id).Include(area => area.Storage).ThenInclude(storage => storage.Areas).FirstOrDefault();
                //var areas = areaCreated.Storage.Areas.ToList();
                //decimal storageHeight = areaCreated.Storage.Height;
                //decimal storageWidth = areaCreated.Storage.Width;
                //decimal storageLength = areaCreated.Storage.Length;
                //double storageVolumne = (double)(storageHeight * storageWidth * storageLength);

                //double areasVolumne = 0;
                //double areasUsed = 0;
                //double areasAvailable = 0;
                //for(int i =0; i <areas.Count; i++)
                //{
                //    areasUsed = 0;
                //    areasAvailable = 0;
                //    var area = await GetById(areas[i].Id);
                //    areasUsed += area.Used;
                //    areasAvailable += area.Available;
                //    areasVolumne += (areasUsed + areasAvailable);
                //}

                //if(storageHeight < model.Height || storageLength < model.Length || storageWidth < model.Width || storageVolumne < areasVolumne)
                //{
                //    await DeleteAsync(areaCreated);
                //    throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Storage size is overload");
                //}

                return _mapper.Map<AreaViewModel>(areaToCreate);
            }
            catch (ErrorResponse e)
            {
                throw new ErrorResponse((int)e.Error.Code, e.Error.Message);
            }
            catch(InvalidOperationException)
            {
                if(areaToCreate != null )await DeleteAsync(areaToCreate);
                throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Storage size is overload");
            }
            catch (Exception ex)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<AreaViewModel> Delete(Guid id)
        {
            try
            {
                // Get area to delete
                var area = await Get(area => area.Id == id && area.IsActive).Include(area => area.Spaces).FirstOrDefaultAsync();
                if (area == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Area id not found");

                // Check area is inused or not
                var areaIsUsed = CheckIsUsed(id);
                if (areaIsUsed) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Area is in used");

                // Change area isActive to false and update
                area.IsActive = false;
                await UpdateAsync(area);
                return _mapper.Map<AreaViewModel>(area);
            }
            catch (ErrorResponse e)
            {
                throw new ErrorResponse((int)e.Error.Code, e.Error.Message);
            }
            catch (Exception ex)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<AreaDetailViewModel> GetById(Guid id)
        {
            try
            {
                List<SpaceViewModel> spacesInArea = new List<SpaceViewModel>();
                // Get area
                var area = await Get(area => area.Id == id && area.IsActive).Include(area => area.Spaces).ThenInclude(space => space.Floors).FirstOrDefaultAsync();
                if (area == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Area id not found");

                // Mapping area to the result
                var result = _mapper.Map<AreaDetailViewModel>(area);

                // Get space list in area
                var spaces = area.Spaces.Where(spaces => spaces.IsActive).ToList();
                if (spaces.Count == 0)
                {
                    result.Usage = 0;
                    result.Used = 0;
                    result.Available = (double)(area.Height * area.Width * area.Length);
                }

                if(area.Height == null || area.Width == null || area.Length == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Area height, width, length can not be null");

                // Get usage inside of each space in space list
                double usage = 0;
                double used = 0;
                double available = 0;
                foreach (var space in spaces)
                {
                    var spaceById = await _spaceService.GetById(space.Id);
                    usage += spaceById.Floors.Select(floor => floor.Usage).Sum();
                    spacesInArea.Add(spaceById);
                }

                // Calculate area used, available and total size
                double total = (double)(area.Height * area.Width * area.Length);
                used = usage * total / 100;
                available = total - used;

                result.Usage = usage;
                result.Used = used;
                result.Available = available;
                result.SpacesInArea = spacesInArea;
                return result;
            }
            catch (ErrorResponse e)
            {
                throw new ErrorResponse((int)e.Error.Code, e.Error.Message);
            }
            catch (Exception ex)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, ex.Message);
            }
            
        }

        public async Task<DynamicModelResponse<AreaViewModel>> GetByStorageId(Guid id, AreaViewModel model, List<int> types, string[] fields, int page, int size)
        {
            try
            {
                // Get area list
                var areas = Get(area => area.StorageId == id && area.IsActive)
                .ProjectTo<AreaViewModel>(_mapper.ConfigurationProvider);

                // Get type required
                if (types.Count > 0) areas = areas.Where(area => types.Contains((int)area.Type));

                // Filter the result
                var result = areas
                     .DynamicFilter(model)
                    .PagingIQueryable(page, size, CommonConstant.LimitPaging, CommonConstant.DefaultPaging);
                if (result.Item2.ToList().Count < 1) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Area not found");
                var rs = new DynamicModelResponse<AreaViewModel>
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
            catch (Exception ex)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<AreaViewModel> Update(Guid id, AreaUpdateViewModel model)
        {
            try
            {
                // Validate input
                _utilService.ValidateString(model.Name, "Area name");

                if (id != model.Id) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Area Id not matched");

                Area entity = await Get(area => area.Id == id && area.IsActive).FirstOrDefaultAsync();
                if (entity == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Area not found");

                Area anotherArea = Get(area => area.Id != id && area.Name == model.Name && area.StorageId == entity.StorageId && area.IsActive).FirstOrDefault();
                if (anotherArea != null) throw new ErrorResponse((int)HttpStatusCode.Conflict, "Area name existed");

                AreaDetailViewModel area = await GetById(id);
                if(area.Used > 0) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Area is already in used");

                if (entity.Height != model.Height || entity.Length != model.Length || entity.Width != model.Width)
                {
                    area = await GetById(id);
                    double newAreaSize = (double)(model.Height * model.Width * model.Length);
                    if(newAreaSize - area.Used < 0) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "New area size is smaller than the total service in area");
                }

                Area updateEntity = _mapper.Map(model, entity);
                await UpdateAsync(updateEntity);

                return _mapper.Map<AreaViewModel>(updateEntity);
            }
            catch (ErrorResponse e)
            {
                throw new ErrorResponse((int)e.Error.Code, e.Error.Message);
            }
            catch (Exception ex)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public bool CheckIsUsed(Guid id)
        {
            try
            {
                Area area = Get(area => area.Id == id && area.IsActive).Include(area => area.Spaces).FirstOrDefault();
                if (area == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Area not found");
                var spaces = area.Spaces.Where(space => space.IsActive);
                foreach (var space in spaces)
                    if (_spaceService.CheckIsUsed(space.Id)) return true;
                return false;
            }
            catch (ErrorResponse e)
            {
                throw new ErrorResponse((int)e.Error.Code, e.Error.Message);
            }
            catch (Exception ex)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, ex.Message);
            }
            
        }
    }
}
