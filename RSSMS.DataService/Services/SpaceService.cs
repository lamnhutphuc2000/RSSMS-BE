using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Constants;
using RSSMS.DataService.Models;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.Responses;
using RSSMS.DataService.UnitOfWorks;
using RSSMS.DataService.Utilities;
using RSSMS.DataService.ViewModels.Boxes;
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
        Task<DynamicModelResponse<SpaceViewModel>> GetAll(SpaceViewModel model, string[] fields, int page, int size);
        Task<SpaceViewModel> GetById(Guid id);
        Task<SpaceViewModel> Create(SpaceCreateViewModel model, string accessToken);
        Task<SpaceViewModel> Delete(Guid id);
        Task<SpaceViewModel> Update(Guid id, SpaceUpdateViewModel model, string accessToken);
        List<BoxUsageViewModel> GetBoxUsageByAreaId(Guid areaId);
        bool CheckIsUsed(Guid id);

    }
    public class SpaceService : BaseService<Space>, ISpaceService
    {
        private readonly IMapper _mapper;
        private readonly IFloorsService _floorsService;
        public SpaceService(IUnitOfWork unitOfWork, IFloorsService floorsService, ISpaceRepository repository, IMapper mapper) : base(unitOfWork, repository)
        {
            _mapper = mapper;
            _floorsService = floorsService;
        }

        public async Task<SpaceViewModel> Create(SpaceCreateViewModel model, string accessToken)
        {
            var space = Get(x => x.Name == model.Name && x.AreaId == model.AreaId && x.IsActive == true).FirstOrDefault();
            if (space != null) throw new ErrorResponse((int)HttpStatusCode.Conflict, "Shelf name is existed");


            var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
            var userId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);


            var spaceToCreate = _mapper.Map<Space>(model);
            spaceToCreate.ModifiedBy = userId;
            await CreateAsync(spaceToCreate);

            await _floorsService.CreateNumberOfFloor(spaceToCreate.Id, model.NumberOfFloor, model.FloorHeight, model.FloorWidth, model.FloorHeight, DateTime.Now);
            return _mapper.Map<SpaceViewModel>(spaceToCreate);
        }

        public async Task<SpaceViewModel> Delete(Guid id)
        {
            var entity = await Get(x => x.Id == id && x.IsActive == true).Include(a => a.Floors).ThenInclude(Floors => Floors.OrderDetails).FirstOrDefaultAsync();
            if (entity == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Shelf id not found");
            var shelfIsUsed = CheckIsUsed(id);
            if (shelfIsUsed) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Shelf is in used");
            entity.IsActive = false;
            await UpdateAsync(entity);
            return _mapper.Map<SpaceViewModel>(entity);
        }

        public async Task<SpaceViewModel> GetById(Guid id)
        {
            var space = await Get(x => x.Id == id && x.IsActive == true)
            .Include(x => x.Floors.Where(a => a.IsActive == true))
            .ThenInclude(x => x.OrderDetails).ThenInclude(x => x.Order).FirstOrDefaultAsync();
            if (space == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Space id not found");
            var result = _mapper.Map<SpaceViewModel>(space);
            result.Floors = _floorsService.GetFloorInSpace(id);
            return result;
        }

        public async Task<SpaceViewModel> Update(Guid id, SpaceUpdateViewModel model, string accessToken)
        {
            if (id != model.Id) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Space Id not matched");

            var entity = await Get(x => x.Id == id && x.IsActive == true).Include(a => a.Floors).ThenInclude(floor => floor.OrderDetails).FirstOrDefaultAsync();
            if (entity == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Space not found");

            var space = Get(x => x.Name == model.Name && x.AreaId == entity.AreaId && x.Id != id && x.IsActive == true).Include(x => x.Floors.Where(x => x.IsActive == true)).FirstOrDefault();
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
            
            if(oldNumberOfFloor != model.NumberOfFloor || floor.Height != model.FloorHeight || floor.Width != model.FloorWidth || floor.Length != model.FloorLength)
            {
                await _floorsService.RemoveFloors(id);
                await _floorsService.CreateNumberOfFloor(id, model.NumberOfFloor, model.FloorHeight, model.FloorWidth, model.FloorLength, DateTime.Now);
            }


            spaceToUpdate = _mapper.Map(model, entity);
            spaceToUpdate.ModifiedBy = userId;
            await UpdateAsync(spaceToUpdate);
            return _mapper.Map<SpaceViewModel>(spaceToUpdate);
        }
        public async Task<DynamicModelResponse<SpaceViewModel>> GetAll(SpaceViewModel model, string[] fields, int page, int size)
        {
            var spaces = Get(x => x.IsActive == true)
                .Include(x => x.Floors.Where(a => a.IsActive == true))
                .ThenInclude(x => x.OrderDetails)
                .ProjectTo<SpaceViewModel>(_mapper.ConfigurationProvider)
                .DynamicFilter(model)
                .PagingIQueryable(page, size, CommonConstant.LimitPaging, CommonConstant.DefaultPaging);

            var result = spaces.Item2.ToList();
            if (result.Count < 1) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Space not found");
            foreach (var space in result)
            {
                space.Floors = _floorsService.GetFloorInSpace((Guid)space.Id);
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

        public List<BoxUsageViewModel> GetBoxUsageByAreaId(Guid areaId)
        {
            //var shelves = Get(x => x.AreaId == areaId && x.IsActive == true).Include(x => x.Floors).ToList();
            //var result = new List<BoxUsageViewModel>();
            //var shelfSelfStorage = shelves.Where(x => x.Type == 2).FirstOrDefault();
            //var services = _servicesService.Get(x => (x.Type == 2 || x.Type == 4) && x.IsActive == true).ToList();
            //if (shelfSelfStorage != null)
            //{
            //    services = _servicesService.Get(x => x.Type == 0 && x.IsActive == true).ToList();
            //}
            //if(services == null)
            //{
            //    return null;
            //}
            //foreach (var service in services)
            //{
            //    result.Add(GetBoxUsageBySizeType(service.Name, (int)service.Type, shelves));
            //}

            //return result;
            return null;
        }

        private BoxUsageViewModel GetBoxUsageBySizeType(string sizeName, int productType, List<Shelf> shelves)
        {
            int totalBox = 0;
            int boxRemaining = 0;
            double usage = 0;
            BoxUsageViewModel result = new BoxUsageViewModel();
            result.ProductType = productType;
            result.SizeType = sizeName;
            result.TotalBox = 0;
            result.Usage = 0;
            result.BoxRemaining = 0;

            if (shelves == null)
            {
                return result;
            }
            foreach (var shelf in shelves)
            {
                if (shelf.Floors != null)
                {
                    var boxes = shelf.Floors.Where(x => x.IsActive == true);
                    totalBox += boxes.ToList().Count;

                    var boxesNotUsed = boxes.ToList().Count;
                    boxRemaining += boxesNotUsed;
                }
            }

            if (totalBox - boxRemaining != 0)
            {
                usage = Math.Ceiling((double)(totalBox - boxRemaining) / totalBox * 100);
            }


            result.TotalBox = totalBox;
            result.BoxRemaining = boxRemaining;
            result.Usage = usage;
            return result;
        }

        public bool CheckIsUsed(Guid id)
        {
            var entity = Get(x => x.Id == id && x.IsActive == true).Include(a => a.Floors).FirstOrDefault();
            if (entity == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Shelf id not found");
            var BoxAssignedToOrder = entity.Floors.Where(x => x.OrderDetails.Count > 0).ToList();
            if (BoxAssignedToOrder.Count() > 0) return true;
            return false;
        }

    }
}
