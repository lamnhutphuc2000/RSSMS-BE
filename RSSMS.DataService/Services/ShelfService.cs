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
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RSSMS.DataService.Services
{
    public interface IShelfService : IBaseService<Shelf>
    {
        Task<DynamicModelResponse<ShelfViewModel>> GetAll(ShelfViewModel model, string[] fields, int page, int size);
        Task<ShelfViewModel> GetById(int id);
        Task<ShelfViewModel> Create(ShelfCreateViewModel model);
        Task<ShelfViewModel> Delete(int id);
        Task<ShelfViewModel> Update(int id, ShelfUpdateViewModel model);
        List<BoxUsageViewModel> GetBoxUsageByAreaId(int areaId);
        bool CheckIsUsed(int id);
    }
    public class ShelfService : BaseService<Shelf>, IShelfService

    {
        private readonly IMapper _mapper;
        private readonly IBoxService _boxService;
        private readonly IProductService _productService;
        public ShelfService(IUnitOfWork unitOfWork, IBoxService boxService, IShelfRepository repository, IProductService productService, IMapper mapper) : base(unitOfWork, repository)
        {
            _mapper = mapper;
            _boxService = boxService;
            _productService = productService;
        }

        public async Task<ShelfViewModel> Create(ShelfCreateViewModel model)
        {
            var shelf = Get(x => x.Name == model.Name && x.AreaId == model.AreaId && x.IsActive == true).FirstOrDefault();
            if (shelf != null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Shelf name is existed");
            var shelfToCreate = _mapper.Map<Shelf>(model);
            await CreateAsync(shelfToCreate);
            int numberOfShelve = model.BoxesInHeight * model.BoxesInWidth;
            await _boxService.CreateNumberOfBoxes(shelfToCreate.Id, numberOfShelve, model.ProductId);
            return _mapper.Map<ShelfViewModel>(shelfToCreate);
        }

        public async Task<ShelfViewModel> Delete(int id)
        {
            var entity = await Get(x => x.Id == id && x.IsActive == true).Include(a => a.Boxes).ThenInclude(boxes => boxes.OrderBoxDetails).FirstOrDefaultAsync();
            if (entity == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Shelf id not found");
            var shelfIsUsed = CheckIsUsed(id);
            if(shelfIsUsed) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Shelf is in used");
            entity.IsActive = false;
            await UpdateAsync(entity);
            return _mapper.Map<ShelfViewModel>(entity);
        }

        public async Task<ShelfViewModel> GetById(int id)
        {
            var shelf = await Get(x => x.Id == id && x.IsActive == true)
                .Include(x => x.Boxes.Where(a => a.IsActive == true))
                .ThenInclude(x => x.Product)
                .Include(x => x.Boxes.Where(a => a.IsActive == true))
                .ThenInclude(x => x.OrderBoxDetails.Where(a => a.IsActive == true)).FirstOrDefaultAsync();
            if (shelf == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Shelf id not found");
            var result = _mapper.Map<ShelfViewModel>(shelf);
            return result;
        }

        public async Task<ShelfViewModel> Update(int id, ShelfUpdateViewModel model)
        {
            if (id != model.Id) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Shelf Id not matched");

            var entity = await Get(x => x.Id == id && x.IsActive == true).Include(a => a.Boxes).ThenInclude(boxes => boxes.OrderBoxDetails).FirstOrDefaultAsync();
            var shelf = Get(x => x.Name == model.Name && x.AreaId == entity.AreaId && x.Id != id && x.IsActive == true).Include(x => x.Boxes.Where(x => x.IsActive == true)).FirstOrDefault();
            if (shelf != null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Shelf name is existed");
            if (entity == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Shelf not found");

            var shelfSize = entity.BoxesInHeight * entity.BoxesInWidth;
            var newShelfSize = model.BoxesInWidth * model.BoxesInHeight;
            var shelfIsUsed = CheckIsUsed(id);
            if (shelfIsUsed)
            {
                if (entity.Type != model.Type) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Shelf is in used");
                if (shelfSize != newShelfSize) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Shelf is in used");
                if(model.ProductId != entity.Boxes.FirstOrDefault().ProductId) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Shelf is in used");
            }

            if (entity.BoxesInHeight != model.BoxesInHeight || entity.BoxesInWidth != model.BoxesInWidth)
            {
                await _boxService.Delete(id);
                await _boxService.CreateNumberOfBoxes(id, model.BoxesInWidth * model.BoxesInHeight, model.ProductId);
            }

            var updateEntity = _mapper.Map(model, entity);
            await UpdateAsync(updateEntity);
            await _boxService.UpdateBoxSize(model.ProductId, id);
            return _mapper.Map<ShelfViewModel>(updateEntity);
        }
        public async Task<DynamicModelResponse<ShelfViewModel>> GetAll(ShelfViewModel model, string[] fields, int page, int size)
        {
            var shelves = Get(x => x.IsActive == true)
                .Include(x => x.Boxes.Where(a => a.IsActive == true))
                .ThenInclude(x => x.Product)
                .Include(x => x.Boxes.Where(a => a.IsActive == true))
                .ThenInclude(x => x.OrderBoxDetails.Where(a => a.IsActive == true))
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

        public List<BoxUsageViewModel> GetBoxUsageByAreaId(int areaId)
        {
            var shelves = Get(x => x.AreaId == areaId && x.IsActive == true).Include(x => x.Boxes).ThenInclude(x => x.Product).ToList();
            var result = new List<BoxUsageViewModel>();
            var shelfSelfStorage = shelves.Where(x => x.Type == 2).FirstOrDefault();
            var products = _productService.Get(x => (x.Type == 2 || x.Type == 4) && x.IsActive == true).ToList();
            if (shelfSelfStorage != null)
            {
                products = _productService.Get(x => x.Type == 0 && x.IsActive == true).ToList();
            }
            foreach (var product in products)
            {
                result.Add(GetBoxUsageBySizeType(product.Name,(int)product.Type, shelves));
            }

            return result;
        }

        private BoxUsageViewModel GetBoxUsageBySizeType(string sizeName,int productType, List<Shelf> shelves)
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
                    var boxes = shelf.Boxes.Where(x => x.IsActive == true && x.Product.Name == sizeName);
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

        public bool CheckIsUsed(int id)
        {
            var entity = Get(x => x.Id == id && x.IsActive == true).Include(a => a.Boxes).ThenInclude(boxes => boxes.OrderBoxDetails).FirstOrDefault();
            if (entity == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Shelf id not found");
            var BoxAssignedToOrder = entity.Boxes.Where(x => x.OrderBoxDetails.Any(a => a.IsActive == true)).ToList();
            if (BoxAssignedToOrder.Count() > 0) return true;
            return false;
        }
    }
}
