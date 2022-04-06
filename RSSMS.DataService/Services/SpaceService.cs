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
using RSSMS.DataService.ViewModels.Floors;
using RSSMS.DataService.ViewModels.OrderDetails;
using RSSMS.DataService.ViewModels.Spaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RSSMS.DataService.Services
{
    public interface ISpaceService : IBaseService<Space>
    {
        Task<DynamicModelResponse<SpaceViewModel>> GetAll(SpaceViewModel model, DateTime? date, string[] fields, int page, int size);
        Task<SpaceViewModel> GetById(Guid id);
        Task<SpaceViewModel> Create(SpaceCreateViewModel model, string accessToken);
        Task<SpaceViewModel> Delete(Guid id);
        Task<SpaceViewModel> Update(Guid id, SpaceUpdateViewModel model, string accessToken);
        bool CheckIsUsed(Guid id);

        Task<List<FloorGetByIdViewModel>> GetFloorOfSpace(Guid areaId, int spaceType, DateTime date, bool isMany);

    }
    public class SpaceService : BaseService<Space>, ISpaceService
    {
        private readonly IMapper _mapper;
        private readonly IFloorService _floorsService;
        public SpaceService(IUnitOfWork unitOfWork, IFloorService floorsService, ISpaceRepository repository, IMapper mapper) : base(unitOfWork, repository)
        {
            _mapper = mapper;
            _floorsService = floorsService;
        }

        public async Task<SpaceViewModel> Create(SpaceCreateViewModel model, string accessToken)
        {
            Space spaceToCreate = null;
            try
            {
                var space = Get(x => x.Name == model.Name && x.AreaId == model.AreaId && x.IsActive == true).FirstOrDefault();
                if (space != null) throw new ErrorResponse((int)HttpStatusCode.Conflict, "Space name is existed");


                var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
                var userId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);


                spaceToCreate = _mapper.Map<Space>(model);
                spaceToCreate.ModifiedBy = userId;
                await CreateAsync(spaceToCreate);

                await _floorsService.CreateNumberOfFloor(spaceToCreate.Id, model.NumberOfFloor, model.FloorHeight, model.FloorWidth, model.FloorLength, DateTime.Now);


                // Check is Area oversize
                // Lay space va area cua space vua tao
                space = Get(space => space.Id == spaceToCreate.Id).Include(space => space.Area).First();
                var area = space.Area;
                if(model.NumberOfFloor * model.FloorHeight > area.Height)
                {
                    var floors = spaceToCreate.Floors.ToList();
                    for (int i = 0; i < floors.Count; i++)
                        await _floorsService.DeleteAsync(floors[i]);
                    await DeleteAsync(spaceToCreate);
                    throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Space heigh is larger than area height.");
                }

                var spacesInArea = Get(space => space.AreaId == area.Id && space.IsActive).Include(space => space.Floors).ToList();



                List<Cuboid> cuboids = new List<Cuboid>();
                for (int i = 0; i < spacesInArea.Count; i++)
                {
                    if(spacesInArea[i].Floors.Count > 0)
                        cuboids.Add(new Cuboid((decimal)spacesInArea[i].Floors.First().Width, (decimal)area.Height, (decimal)spacesInArea[i].Floors.First().Length));
                }
                var parameter = new BinPackParameter((decimal)area.Width, (decimal)area.Height, (decimal)area.Length, cuboids);

                var binPacker = BinPacker.GetDefault(BinPackerVerifyOption.BestOnly);
                var result = binPacker.Pack(parameter);
                if (result.BestResult.Count > 1)
                {
                    var floors = spaceToCreate.Floors.ToList();
                    for (int i = 0; i < floors.Count; i++)
                        await _floorsService.DeleteAsync(floors[i]);
                    await DeleteAsync(spaceToCreate);
                    throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Area size is overload");
                }
                    
                return _mapper.Map<SpaceViewModel>(spaceToCreate);
            }
            catch(InvalidOperationException)
            {
                if(spaceToCreate != null)
                {
                    var floors = spaceToCreate.Floors.ToList();
                    for (int i = 0; i < floors.Count; i++)
                        await _floorsService.DeleteAsync(floors[i]);
                    await DeleteAsync(spaceToCreate);
                }
                throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Area size is overload");
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

        public async Task<SpaceViewModel> Delete(Guid id)
        {
            try
            {
                var entity = await Get(x => x.Id == id && x.IsActive == true).Include(a => a.Floors).ThenInclude(Floors => Floors.OrderDetails).FirstOrDefaultAsync();
                if (entity == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Space id not found");
                var shelfIsUsed = CheckIsUsed(id);
                if (shelfIsUsed) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Space is in used");
                entity.IsActive = false;
                await UpdateAsync(entity);
                return _mapper.Map<SpaceViewModel>(entity);
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

        public async Task<SpaceViewModel> GetById(Guid id)
        {
            try
            {
                var space = await Get(x => x.Id == id && x.IsActive == true)
            .Include(x => x.Floors.Where(a => a.IsActive == true))
            .ThenInclude(x => x.OrderDetails).ThenInclude(x => x.Order).FirstOrDefaultAsync();
                if (space == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Space id not found");
                var result = _mapper.Map<SpaceViewModel>(space);
                result.Floors = await _floorsService.GetFloorInSpace(id, null);
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

        public async Task<SpaceViewModel> Update(Guid id, SpaceUpdateViewModel model, string accessToken)
        {
            try
            {
                if (id != model.Id) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Space Id not matched");

                var entity = await Get(x => x.Id == id && x.IsActive).Include(a => a.Floors).ThenInclude(floor => floor.OrderDetails).FirstOrDefaultAsync();
                if (entity == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Space not found");

                var space = Get(x => x.Name == model.Name && x.AreaId == entity.AreaId && x.Id != id && x.IsActive).Include(x => x.Floors.Where(x => x.IsActive == true)).FirstOrDefault();
                if (space != null) throw new ErrorResponse((int)HttpStatusCode.Conflict, "Space name is existed");


                var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
                var userId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);

                Space spaceToUpdate = null;
                var floor = entity.Floors.FirstOrDefault();
                if (floor == null)
                {
                    spaceToUpdate = _mapper.Map(model, entity);
                    await UpdateAsync(spaceToUpdate);
                    return _mapper.Map<SpaceViewModel>(spaceToUpdate);
                }

                var oldNumberOfFloor = entity.Floors.Where(floor => floor.IsActive == true).Count();

                if (oldNumberOfFloor != model.NumberOfFloor || floor.Height != model.FloorHeight || floor.Width != model.FloorWidth || floor.Length != model.FloorLength)
                {
                    // Check is Area oversize
                    // Lay space va area cua space vua tao
                    space = Get(space => space.Id == id).Include(space => space.Area).First();
                    var area = space.Area;
                    if (model.NumberOfFloor * model.FloorHeight > area.Height)
                        throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Space heigh is larger than area height.");

                    var spacesInArea = Get(space => space.AreaId == area.Id && space.IsActive).Include(space => space.Floors).ToList();
                    List<Cuboid> cuboids = new List<Cuboid>();
                    for (int i = 0; i < spacesInArea.Count; i++)
                    {
                        if (spacesInArea[i].Id != id && spacesInArea[i].Floors.Count > 0)
                            cuboids.Add(new Cuboid((decimal)spacesInArea[i].Floors.First().Width, (decimal)area.Height, (decimal)spacesInArea[i].Floors.First().Length));
                        else
                            cuboids.Add(new Cuboid((decimal)model.FloorWidth, (decimal)area.Height, (decimal)model.FloorLength));
                    }
                    var parameter = new BinPackParameter((decimal)area.Width, (decimal)area.Height, (decimal)area.Length, cuboids);

                    var binPacker = BinPacker.GetDefault(BinPackerVerifyOption.BestOnly);
                    var result = binPacker.Pack(parameter);
                    if (result.BestResult.Count > 1)
                        throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Area size is overload");


                    await _floorsService.RemoveFloors(id);
                    await _floorsService.CreateNumberOfFloor(id, model.NumberOfFloor, model.FloorHeight, model.FloorWidth, model.FloorLength, DateTime.Now);
                }


                spaceToUpdate = _mapper.Map(model, entity);
                spaceToUpdate.ModifiedBy = userId;
                await UpdateAsync(spaceToUpdate);
                return await GetById(model.Id);
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
        public async Task<DynamicModelResponse<SpaceViewModel>> GetAll(SpaceViewModel model, DateTime? date, string[] fields, int page, int size)
        {
            try
            {
                var spaces = Get(x => x.IsActive == true)
                .Include(x => x.Floors.Where(a => a.IsActive == true))
                .ThenInclude(x => x.OrderDetails)
                .ProjectTo<SpaceViewModel>(_mapper.ConfigurationProvider)
                .DynamicFilter(model)
                .PagingIQueryable(page, size, CommonConstant.LimitPaging, CommonConstant.DefaultPaging);

                var result = await spaces.Item2.ToListAsync();
                if (result.Count < 1) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Space not found");
                foreach (var space in result)
                {
                    space.Floors = await _floorsService.GetFloorInSpace((Guid)space.Id, date);
                }

                var rs = new DynamicModelResponse<SpaceViewModel>
                {
                    Metadata = new PagingMetaData
                    {
                        Page = page,
                        Size = size,
                        Total = spaces.Item1,
                        TotalPage = (int)Math.Ceiling((double)spaces.Item1 / size)
                    },
                    Data = result
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


        public bool CheckIsUsed(Guid id)
        {
            try
            {
                var entity = Get(space => space.Id == id && space.IsActive).Include(space => space.Floors).ThenInclude(floor => floor.OrderDetails).FirstOrDefault();
                if (entity == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Space id not found");
                var BoxAssignedToOrder = entity.Floors.Where(x => x.OrderDetails.Count > 0 && x.IsActive).ToList();
                if (BoxAssignedToOrder.Count() > 0) return true;
                return false;
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

        public async Task<List<FloorGetByIdViewModel>> GetFloorOfSpace(Guid areaId, int spaceType, DateTime date, bool isMany)
        {
            List<FloorGetByIdViewModel> result = new List<FloorGetByIdViewModel>();
            var spaces = Get(space => space.AreaId == areaId && space.Type == spaceType && space.IsActive).ToList();
            if (spaces.Count == 0) return null;
            bool isSelfStorage = false;
            if (spaceType == 1) isSelfStorage = true;
            foreach(var space in spaces)
            {
                var floor = await _floorsService.GetBySpaceId(space.Id, date, isMany, isSelfStorage);
                if(floor != null) result.Add(floor);
            }
            if (result.Count == 0) return null;
            return result;
        }
    }
}
