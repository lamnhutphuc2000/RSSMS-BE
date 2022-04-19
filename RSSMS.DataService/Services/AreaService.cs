using _3DBinPacking.Enum;
using _3DBinPacking.Model;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Constants;
using RSSMS.DataService.Enums;
using RSSMS.DataService.Models;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.Responses;
using RSSMS.DataService.UnitOfWorks;
using RSSMS.DataService.Utilities;
using RSSMS.DataService.ViewModels.Areas;
using RSSMS.DataService.ViewModels.Floors;
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
        Task<bool> CheckIsUsed(Guid id);
        Task<List<FloorGetByIdViewModel>> GetFloorOfArea(Guid storageId, int spaceType, DateTime date, bool isMany);
        Task<bool> CheckAreaAvailable(Guid storageId, int spaceType, DateTime dateFrom, DateTime dateTo, bool isMany, List<Cuboid> cuboids, List<Request> requestsAssignToStorage, bool isCustomerDelivery);
    }
    public class AreaService : BaseService<Area>, IAreaService

    {
        private readonly IMapper _mapper;
        private readonly ISpaceService _spaceService;
        private readonly IUtilService _utilService;
        private readonly IServiceService _serviceService;
        private readonly IAccountService _accountService;
        public AreaService(IUnitOfWork unitOfWork, ISpaceService spaceService,
            IUtilService utilService, IServiceService serviceService,
            IAccountService accountService,
            IAreaRepository repository, IMapper mapper) : base(unitOfWork, repository)
        {
            _mapper = mapper;
            _spaceService = spaceService;
            _utilService = utilService;
            _serviceService = serviceService;
            _accountService = accountService;
        }

        public async Task<AreaViewModel> Create(AreaCreateViewModel model)
        {
            Area areaToCreate = null;
            try
            {
                // Validate input
                _utilService.ValidateString(model.Name, "tên khu vực");
                _utilService.ValidateString(model.Description, "giới thiệu khu vực");
                _utilService.ValidateDecimal(model.Width, " chiều rộng khu vực");
                _utilService.ValidateDecimal(model.Height, " chiều cao khu vực");
                _utilService.ValidateDecimal(model.Length, " chiều dài khu vực");
                _utilService.ValidateInt(model.Type, " loại khu vực");

                // Check area name is existed
                var entity = Get(area => area.StorageId == model.StorageId && area.Name == model.Name && area.IsActive).FirstOrDefault();
                if (entity != null) throw new ErrorResponse((int)HttpStatusCode.Conflict, "Tên khu vực đã tồn tại trong kho này");

                // Create new Area
                areaToCreate = _mapper.Map<Area>(model);
                await CreateAsync(areaToCreate);


                // Check is Storage oversize
                var area = Get(area => area.Id == areaToCreate.Id).Include(area => area.Storage)
                    .ThenInclude(storage => storage.Areas).FirstOrDefault();

                var storage = area.Storage;
                var areaList = storage.Areas.Where(area => area.IsActive).ToList();
                if (storage.Height < area.Height || storage.Width < area.Width || storage.Length < area.Length)
                    throw new InvalidOperationException();
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



                return _mapper.Map<AreaViewModel>(areaToCreate);
            }
            catch (ErrorResponse e)
            {
                throw new ErrorResponse((int)e.Error.Code, e.Error.Message);
            }
            catch (InvalidOperationException)
            {
                if (areaToCreate != null) await DeleteAsync(areaToCreate);
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
                if (area == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Không tìm thấy khu vực");

                // Check area is inused or not
                if (await CheckIsUsed(id)) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Khu vực đang được sử dụng");

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
                if (area == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Không tìm thấy khu vực");

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

                if (area.Height == null || area.Width == null || area.Length == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Area height, width, length can not be null");

                // Get usage inside of each space in space list
                double usage = 0;
                double used = 0;
                double available = 0;
                double total = 0;
                foreach (var space in spaces)
                {
                    var spaceById = await _spaceService.GetById(space.Id);
                    total += spaceById.Floors.Select(floor => floor.Used).Sum();
                    total += spaceById.Floors.Select(floor => floor.Available).Sum();
                    used += spaceById.Floors.Select(floor => floor.Used).Sum();
                    spacesInArea.Add(spaceById);
                }

                // Calculate area used, available and total size
                available = total - used;
                if (total != 0) usage = used * 100 / total;
                else usage = 0;
                result.Usage = Math.Round(usage, 2);
                result.Used = Math.Round(used, 2);
                result.Available = Math.Round(available, 2);
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
                if (result.Item2.ToList().Count < 1) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Không tìm thấy khu vực");
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
                _utilService.ValidateString(model.Name, "tên khu vực");
                _utilService.ValidateString(model.Description, "giới thiệu khu vực");
                _utilService.ValidateDecimal(model.Width, " chiều rộng khu vực");
                _utilService.ValidateDecimal(model.Height, " chiều cao khu vực");
                _utilService.ValidateDecimal(model.Length, " chiều dài khu vực");
                _utilService.ValidateInt(model.Type, " loại khu vực");

                if (id != model.Id) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Id khu vực không khớp");

                Area entity = await Get(area => area.Id == id && area.IsActive).FirstOrDefaultAsync();
                if (entity == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Không tìm thấy khu vực");

                Area anotherArea = Get(area => area.Id != id && area.Name == model.Name && area.StorageId == entity.StorageId && area.IsActive).FirstOrDefault();
                if (anotherArea != null) throw new ErrorResponse((int)HttpStatusCode.Conflict, "Tên khu vực đã tồn tại");

                AreaDetailViewModel area = await GetById(id);
                if (area.Used > 0) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Khu vực đang được sử dụng");

                // check area update size
                if (entity.Height != model.Height || entity.Length != model.Length || entity.Width != model.Width)
                {
                    // Check is Storage oversize
                    entity = Get(area => area.Id == id).Include(area => area.Storage)
                        .ThenInclude(storage => storage.Areas)
                        .FirstOrDefault();

                    var storage = entity.Storage;
                    var areaList = storage.Areas.Where(area => area.IsActive).ToList();

                    List<Cuboid> cuboids = new List<Cuboid>();
                    for (int i = 0; i < areaList.Count; i++)
                    {
                        // Check if the area is the area to updated
                        if (areaList[i].Id == id)
                            cuboids.Add(new Cuboid((decimal)model.Width, (decimal)model.Height, (decimal)model.Length));
                        else
                            cuboids.Add(new Cuboid((decimal)areaList[i].Width, (decimal)areaList[i].Height, (decimal)areaList[i].Length));
                    }
                    var parameter = new BinPackParameter(storage.Width, storage.Height, storage.Length, cuboids);

                    var binPacker = BinPacker.GetDefault(BinPackerVerifyOption.BestOnly);
                    var result = binPacker.Pack(parameter);
                    if (result.BestResult.Count > 1)
                        throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Kích thước khu vực vượt quá kích thước kho");


                    cuboids = new List<Cuboid>();
                    cuboids.Add(new Cuboid((decimal)model.Width, (decimal)model.Height, (decimal)model.Length));
                    var floorsInSpaces = area.SpacesInArea.Select(x => x.Floors.ToList()).ToList();
                    foreach (var space in floorsInSpaces)
                    {
                        foreach (var floor in space)
                        {
                            cuboids.Add(new Cuboid((decimal)floor.Width, (decimal)floor.Height, (decimal)floor.Length));
                        }
                    }

                    parameter = new BinPackParameter(storage.Width, storage.Height, storage.Length, cuboids);

                    binPacker = BinPacker.GetDefault(BinPackerVerifyOption.BestOnly);
                    try
                    {
                        result = binPacker.Pack(parameter);
                    }
                    catch (InvalidOperationException)
                    {
                        throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Kích thước khu vực quá nhỏ so với không gian bên trong đã được tạo");
                    }
                    if (result.BestResult.Count > 1)
                        throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Kích thước khu vực quá nhỏ so với không gian bên trong đã được tạo");
                }



                Area updateEntity = _mapper.Map(model, entity);
                await UpdateAsync(updateEntity);

                return _mapper.Map<AreaViewModel>(updateEntity);
            }
            catch (ErrorResponse e)
            {
                throw new ErrorResponse((int)e.Error.Code, e.Error.Message);
            }
            catch (InvalidOperationException)
            {
                throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Kích thước khu vực không hợp lệ");
            }
            catch (Exception ex)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<bool> CheckIsUsed(Guid id)
        {
            try
            {
                Area area = Get(area => area.Id == id && area.IsActive).Include(area => area.Spaces).FirstOrDefault();
                if (area == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Không tìm thấy khu vực");
                var spaces = area.Spaces.Where(space => space.IsActive);
                foreach (var space in spaces)
                    if (await _spaceService.CheckIsUsed(space.Id)) return true;
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

        public async Task<List<FloorGetByIdViewModel>> GetFloorOfArea(Guid storageId, int spaceType, DateTime date, bool isMany)
        {
            List<FloorGetByIdViewModel> result = new List<FloorGetByIdViewModel>();
            var areas = await Get(area => area.IsActive && area.StorageId == storageId).ToListAsync();
            foreach (var area in areas)
            {
                //var space = await _spaceService.GetFloorOfSpace(area.Id, spaceType, date, isMany);
                //if (space != null) result.AddRange(space);
            }
            if (result.Count == 0) return null;
            return result;
        }

        public async Task<bool> CheckAreaAvailable(Guid storageId, int spaceType, DateTime dateFrom, DateTime dateTo, bool isMany, List<Cuboid> cuboids, List<Request> requestsAssignToStorage, bool isCustomerDelivery)
        {
            bool result = false;

            var areas = await Get(area => area.IsActive && area.StorageId == storageId).ToListAsync();

            List<FloorGetByIdViewModel> floorList = new List<FloorGetByIdViewModel>();
            // cuboidsTmp chứa các order detail đã có sẵn, các request đã được assign
            List<Cuboid> cuboidsTmp = new List<Cuboid>();
            cuboidsTmp.AddRange(cuboids);
            foreach (var area in areas)
            {
                var floors = await _spaceService.GetFloorOfSpace(area.Id, spaceType, dateFrom, dateTo, isMany);
                if (floors != null)
                {
                    floorList.AddRange(floors);
                    foreach (var floor in floors)
                    {
                        foreach (var orderDetail in floor.OrderDetails)
                        {
                            if (isMany) cuboidsTmp.Add(new Cuboid((decimal)orderDetail.Width, 1, (decimal)orderDetail.Length, 0, orderDetail.Id));
                            else cuboidsTmp.Add(new Cuboid((decimal)orderDetail.Width, (decimal)orderDetail.Height, (decimal)orderDetail.Length, 0, orderDetail.Id));
                        }
                    }

                }
            }
            if (floorList.Count == 0) return result;

            if (spaceType == (int)SpaceType.Dien_tich && (floorList.Count < cuboids.Count || floorList.Count <= requestsAssignToStorage.Count)) return false;

            foreach (var request in requestsAssignToStorage)
            {
                var servicesInRequestDetail = request.RequestDetails.Select(requestDetail => new
                {
                    ServiceId = (Guid)requestDetail.ServiceId,
                    Height = (decimal?)null,
                    Width = (decimal?)null,
                    Length = (decimal?)null,
                    Amount = (int)requestDetail.Amount
                }).ToList();
                if (request.Order != null)
                    servicesInRequestDetail = request.Order.OrderDetails.Select(orderDetail => new
                    {
                        ServiceId = orderDetail.Id,
                        Height = orderDetail.Height,
                        Width = orderDetail.Width,
                        Length = orderDetail.Length,
                        Amount = 0
                    }).ToList();

                for (int i = 0; i < servicesInRequestDetail.Count; i++)
                {
                    for (int j = 0; j < servicesInRequestDetail[i].Amount; j++)
                    {
                        if (servicesInRequestDetail[i].Amount != 0)
                        {
                            var service = _serviceService.Get(service => service.Id == servicesInRequestDetail[i].ServiceId).FirstOrDefault();
                            if (service.Type != (int)ServiceType.Phu_kien)
                            {

                                if (isMany) cuboidsTmp.Add(new Cuboid(service.Width, 1, service.Length, 0, service.Id));
                                else cuboidsTmp.Add(new Cuboid(service.Width, service.Height, service.Length, 0, service.Id));
                            }
                        }
                        else
                        {
                            if (isMany) cuboidsTmp.Add(new Cuboid((decimal)servicesInRequestDetail[i].Width, 1, (decimal)servicesInRequestDetail[i].Length));
                            else cuboidsTmp.Add(new Cuboid((decimal)servicesInRequestDetail[i].Width, (decimal)servicesInRequestDetail[i].Height, (decimal)servicesInRequestDetail[i].Length));

                        }
                    }
                }
            }

            foreach (var floor in floorList)
            {
                BinPackParameter parameter = null;
                List<Cuboid> cuboidsToPack = new List<Cuboid>();
                foreach (var cuboid in cuboidsTmp)
                {
                    if (spaceType == (int)SpaceType.Dien_tich)
                    {
                        if (cuboid.Height == floor.Height && cuboid.Depth == floor.Length && cuboid.Width == floor.Width)
                            cuboidsToPack.Add(cuboid);
                    }
                    else
                    {
                        if (cuboid.Height <= floor.Height && cuboid.Depth <= floor.Length && cuboid.Width <= floor.Width)
                            cuboidsToPack.Add(cuboid);
                    }

                }
                if (cuboidsToPack.Count != 0)
                {
                    if (isMany) parameter = new BinPackParameter(floor.Width, 1, floor.Length, cuboidsToPack);
                    else parameter = new BinPackParameter(floor.Width, floor.Height, floor.Length, cuboidsToPack);

                    var binPacker = BinPacker.GetDefault(BinPackerVerifyOption.BestOnly);
                    var binResult = binPacker.Pack(parameter);
                    if (binResult.BestResult.Count == 1)
                    {
                        foreach (var cuboid in binResult.BestResult.First())
                        {
                            var cuboidTmp = cuboidsTmp.Where(x => x.Tag == cuboid.Tag).FirstOrDefault();
                            cuboidsTmp.Remove(cuboidTmp);
                        }
                        if (cuboidsTmp.Count == 0) result = true;
                    }
                    else
                    {
                        foreach (var cuboid in binResult.BestResult.First())
                        {
                            var cuboidTmp = cuboidsTmp.Where(x => x.Tag == cuboid.Tag).FirstOrDefault();
                            cuboidsTmp.Remove(cuboidTmp);
                        }
                    }
                }
            }

            if (cuboidsTmp.Count == 0) result = true;

            return result;
        }
    }
}
