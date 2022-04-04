using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Constants;
using RSSMS.DataService.Models;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.Responses;
using RSSMS.DataService.UnitOfWorks;
using RSSMS.DataService.Utilities;
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
            try
            {
                var space = Get(x => x.Name == model.Name && x.AreaId == model.AreaId && x.IsActive == true).FirstOrDefault();
                if (space != null) throw new ErrorResponse((int)HttpStatusCode.Conflict, "Space name is existed");


                var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
                var userId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);


                var spaceToCreate = _mapper.Map<Space>(model);
                spaceToCreate.ModifiedBy = userId;
                await CreateAsync(spaceToCreate);

                await _floorsService.CreateNumberOfFloor(spaceToCreate.Id, model.NumberOfFloor, model.FloorHeight, model.FloorWidth, model.FloorLength, DateTime.Now);

                double areaUsed = 0;
                // Lay space va area cua space vua tao
                var spaceCreated = Get(space => space.Id == spaceToCreate.Id).Include(space => space.Area).ThenInclude(area => area.Spaces).Include(space => space.Floors).First();
                var area = spaceCreated.Area;
                // lay size cua area
                double areaSize = (double)(area.Length * area.Width * area.Height);
                // lay het spaces cua area ra
                var spacesInArea = area.Spaces.Where(space => space.IsActive).ToList();
                // tinh tong used cua area hien tai
                foreach(var spaceInArea in spacesInArea)
                {
                    var floorInSpace = await _floorsService.GetFloorInSpace(spaceInArea.Id, null);
                    areaUsed += floorInSpace.Select(floor => floor.Used).Sum();
                    areaUsed += floorInSpace.Select(floor => floor.Available).Sum();
                }

                if(areaUsed > areaSize)
                {
                    var floors = spaceCreated.Floors.ToList();
                    for (int i = 0; i < floors.Count; i++)
                        await _floorsService.DeleteAsync(floors[i]);
                    await DeleteAsync(spaceCreated);
                    throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Area size is overload");
                }

                
                return _mapper.Map<SpaceViewModel>(spaceToCreate);
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
                var entity = Get(x => x.Id == id && x.IsActive == true).Include(a => a.Floors).FirstOrDefault();
                if (entity == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Space id not found");
                var BoxAssignedToOrder = entity.Floors.Where(x => x.OrderDetails.Count > 0).ToList();
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
    }
}
