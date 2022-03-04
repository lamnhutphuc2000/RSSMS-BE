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
using RSSMS.DataService.ViewModels.Shelves;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RSSMS.DataService.Services
{
    public interface IShelfService : IBaseService<Shelf>
    {
        Task<DynamicModelResponse<ShelfViewModel>> GetAll(ShelfViewModel model, string[] fields, int page, int size);
        Task<ShelfViewModel> GetById(Guid id);
        Task<ShelfViewModel> Create(ShelfCreateViewModel model, string accessToken);
        Task<ShelfViewModel> Delete(Guid id);
        Task<ShelfViewModel> Update(Guid id, ShelfUpdateViewModel model, string accessToken);
        List<BoxUsageViewModel> GetBoxUsageByAreaId(Guid areaId);
        bool CheckIsUsed(Guid id);
    }
    public class ShelfService : BaseService<Shelf>, IShelfService

    {
        private readonly IMapper _mapper;
        private readonly IBoxService _boxService;
        private readonly IServicesService _servicesService;
        public ShelfService(IUnitOfWork unitOfWork, IBoxService boxService, IShelfRepository repository, IServicesService servicesService, IMapper mapper) : base(unitOfWork, repository)
        {
            _mapper = mapper;
            _boxService = boxService;
            _servicesService = servicesService;
        }

        public async Task<ShelfViewModel> Create(ShelfCreateViewModel model, string accessToken)
        {
            var shelf = Get(x => x.Name == model.Name && x.AreaId == model.AreaId && x.IsActive == true).FirstOrDefault();
            if (shelf != null) throw new ErrorResponse((int)HttpStatusCode.Conflict, "Shelf name is existed");

            var service = _servicesService.Get(x => x.Id == model.ServiceId && x.IsActive == true).FirstOrDefault();
            if(service == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Service not found");

            var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
            var userId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);


            var shelfToCreate = _mapper.Map<Shelf>(model);
            await CreateAsync(shelfToCreate);
            int numberOfShelve = model.BoxesInHeight * model.BoxesInWidth;
            await _boxService.CreateNumberOfBoxes(shelfToCreate.Id, numberOfShelve, model.ServiceId, service.Name, userId);
            return _mapper.Map<ShelfViewModel>(shelfToCreate);
        }

        public async Task<ShelfViewModel> Delete(Guid id)
        {
            var entity = await Get(x => x.Id == id && x.IsActive == true).Include(a => a.Boxes).ThenInclude(boxes => boxes.OrderDetail).FirstOrDefaultAsync();
            if (entity == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Shelf id not found");
            var shelfIsUsed = CheckIsUsed(id);
            if (shelfIsUsed) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Shelf is in used");
            entity.IsActive = false;
            await UpdateAsync(entity);
            return _mapper.Map<ShelfViewModel>(entity);
        }

        public async Task<ShelfViewModel> GetById(Guid id)
        {
            var shelf = await Get(x => x.Id == id && x.IsActive == true)
            .Include(x => x.Boxes.Where(a => a.IsActive == true))
            .ThenInclude(x => x.Service)
            .Include(x => x.Boxes.Where(a => a.IsActive == true))
            .ThenInclude(x => x.OrderDetail).ThenInclude(x => x.Order).FirstOrDefaultAsync();
            if (shelf == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Shelf id not found");
            var result = _mapper.Map<ShelfViewModel>(shelf);
            var boxes = result.Boxes;
            DateTime now = DateTime.Now;
            List<BoxViewModel> newBoxes = new List<BoxViewModel>();
            foreach (var box in boxes)
            {
                box.Status = null;
                if (box.ReturnDate != null)
                {
                    var dayGap = box.ReturnDate - now;
                    if (dayGap.Value.Days <= 3 && dayGap.Value.Days > 0)
                    {
                        // close out of date
                        box.Status = 1;
                    }
                    if (dayGap.Value.Days <= 0)
                    {
                        // out of date
                        box.Status = 0;
                    }
                    if (dayGap.Value.Days > 3)
                    {
                        // not close out of date
                        box.Status = 2;
                    }
                }
                newBoxes.Add(box);
            }
            result.Boxes = newBoxes;
            return result;
        }

        public async Task<ShelfViewModel> Update(Guid id, ShelfUpdateViewModel model, string accessToken)
        {
            if (id != model.Id) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Shelf Id not matched");

            var entity = await Get(x => x.Id == id && x.IsActive == true).Include(a => a.Boxes).ThenInclude(boxes => boxes.OrderDetail).FirstOrDefaultAsync();
            var shelf = Get(x => x.Name == model.Name && x.AreaId == entity.AreaId && x.Id != id && x.IsActive == true).Include(x => x.Boxes.Where(x => x.IsActive == true)).FirstOrDefault();
            if (shelf != null) throw new ErrorResponse((int)HttpStatusCode.Conflict, "Shelf name is existed");
            if (entity == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Shelf not found");
            var service = _servicesService.Get(x => x.Id == model.ServiceId && x.IsActive == true).FirstOrDefault();
            if (service == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Service not found");


            var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
            var userId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);


            var shelfSize = entity.BoxesInHeight * entity.BoxesInWidth;
            var newShelfSize = model.BoxesInWidth * model.BoxesInHeight;
            var shelfIsUsed = CheckIsUsed(id);
            if (shelfIsUsed)
            {
                if (entity.Type != model.Type) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Shelf is in used");
                if (shelfSize != newShelfSize) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Shelf is in used");
                if (model.ServiceId != entity.Boxes.FirstOrDefault().ServiceId) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Shelf is in used");
            }

            if (entity.BoxesInHeight != model.BoxesInHeight || entity.BoxesInWidth != model.BoxesInWidth)
            {
                await _boxService.Delete(id, userId);
                await _boxService.CreateNumberOfBoxes(id, model.BoxesInWidth * model.BoxesInHeight, model.ServiceId, service.Name, userId);
            }

            var updateEntity = _mapper.Map(model, entity);
            await UpdateAsync(updateEntity);
            return _mapper.Map<ShelfViewModel>(updateEntity);
        }
        public async Task<DynamicModelResponse<ShelfViewModel>> GetAll(ShelfViewModel model, string[] fields, int page, int size)
        {
                var shelves = Get(x => x.IsActive == true)
                .Include(x => x.Boxes.Where(a => a.IsActive == true))
                .ThenInclude(x => x.Service)
                .Include(x => x.Boxes.Where(a => a.IsActive == true))
                .ThenInclude(x => x.OrderDetail)
                .ProjectTo<ShelfViewModel>(_mapper.ConfigurationProvider)
                .DynamicFilter(model)
                .PagingIQueryable(page, size, CommonConstant.LimitPaging, CommonConstant.DefaultPaging);
                var rs = new DynamicModelResponse<ShelfViewModel>
                {
                    Metadata = new PagingMetaData
                    {
                        Page = page,
                        Size = size,
                        Total = shelves.Item1,
                        TotalPage = (int)Math.Ceiling((double)shelves.Item1 / size)
                    },
                    Data = shelves.Item2.ToList()
                };
                return rs;
        }

        public List<BoxUsageViewModel> GetBoxUsageByAreaId(Guid areaId)
        {
            var shelves = Get(x => x.AreaId == areaId && x.IsActive == true).Include(x => x.Boxes).ThenInclude(x => x.Service).ToList();
            var result = new List<BoxUsageViewModel>();
            var shelfSelfStorage = shelves.Where(x => x.Type == 2).FirstOrDefault();
            var services = _servicesService.Get(x => (x.Type == 2 || x.Type == 4) && x.IsActive == true).ToList();
            if (shelfSelfStorage != null)
            {
                services = _servicesService.Get(x => x.Type == 0 && x.IsActive == true).ToList();
            }
            if(services == null)
            {
                return null;
            }
            foreach (var service in services)
            {
                result.Add(GetBoxUsageBySizeType(service.Name, (int)service.Type, shelves));
            }

            return result;
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
                if (shelf.Boxes != null)
                {
                    var boxes = shelf.Boxes.Where(x => x.IsActive == true && x.Service.Name == sizeName);
                    totalBox += boxes.ToList().Count;

                    var boxesNotUsed = boxes.Where(x => x.Status == 0).ToList().Count;
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
            var entity = Get(x => x.Id == id && x.IsActive == true).Include(a => a.Boxes).FirstOrDefault();
            if (entity == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Shelf id not found");
            var BoxAssignedToOrder = entity.Boxes.Where(x => x.OrderDetail != null).ToList();
            if (BoxAssignedToOrder.Count() > 0) return true;
            return false;
        }
    }
}
