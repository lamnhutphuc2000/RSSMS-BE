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
    }
    public class ShelfService : BaseService<Shelf>, IShelfService

    {
        private readonly IMapper _mapper;
        private readonly IBoxService _boxService;
        public ShelfService(IUnitOfWork unitOfWork, IBoxService boxService, IShelfRepository repository, IMapper mapper) : base(unitOfWork, repository)
        {
            _mapper = mapper;
            _boxService = boxService;
        }

        public async Task<ShelfViewModel> Create(ShelfCreateViewModel model)
        {
            var shelf = Get(x => x.Name == model.Name && x.AreaId == model.AreaId && x.IsActive == true).FirstOrDefault();
            if (shelf != null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Shelf name is existed");
            var shelfToCreate = _mapper.Map<Shelf>(model);
            await CreateAsync(shelfToCreate);
            int numberOfShelve = model.BoxesInHeight * model.BoxesInWidth;
            await _boxService.CreateNumberOfBoxes(shelfToCreate.Id, numberOfShelve, model.BoxSize);
            return _mapper.Map<ShelfViewModel>(shelfToCreate);
        }

        public async Task<ShelfViewModel> Delete(int id)
        {
            var entity = await GetAsync<int>(id);
            if (entity == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Shelf id not found");
            entity.IsActive = false;
            await UpdateAsync(entity);
            return _mapper.Map<ShelfViewModel>(entity);
        }

        public async Task<ShelfViewModel> GetById(int id)
        {
            var shelf = await Get(x => x.Id == id && x.IsActive == true).Include(x => x.Boxes.Where(a => a.IsActive == true)).FirstOrDefaultAsync();
            if (shelf == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Shelf id not found");
            var result = _mapper.Map<ShelfViewModel>(shelf);
            return result;
        }

        public async Task<ShelfViewModel> Update(int id, ShelfUpdateViewModel model)
        {
            if (id != model.Id) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Shelf Id not matched");

            var entity = await GetAsync(id);
            var shelf = Get(x => x.Name == model.Name && x.AreaId == entity.AreaId && x.Id != id && x.IsActive == true).Include(x => x.Boxes.Where(x => x.IsActive == true)).FirstOrDefault();
            if (shelf != null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Shelf name is existed");
            if (entity == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Shelf not found");

            if (entity.BoxesInHeight != model.BoxesInHeight || entity.BoxesInWidth != model.BoxesInWidth)
            {
                await _boxService.Delete(id);
                await _boxService.CreateNumberOfBoxes(id, model.BoxesInWidth * model.BoxesInHeight, model.BoxSize);
            }

            var updateEntity = _mapper.Map(model, entity);
            await UpdateAsync(updateEntity);
            await _boxService.UpdateBoxType(model.BoxSize, id);
            return _mapper.Map<ShelfViewModel>(updateEntity);
        }
        public async Task<DynamicModelResponse<ShelfViewModel>> GetAll(ShelfViewModel model, string[] fields, int page, int size)
        {
            var shelves = Get(x => x.IsActive == true).Include(x => x.Boxes.Where(a => a.IsActive == true)).ToList().AsQueryable()
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
            var shelves = Get(x => x.AreaId == areaId && x.IsActive == true).Include(x => x.Boxes).ToList();

            var result = new List<BoxUsageViewModel>();

            result.Add(GetBoxUsageBySizeType(0, shelves));
            result.Add(GetBoxUsageBySizeType(1, shelves));
            result.Add(GetBoxUsageBySizeType(2, shelves));
            result.Add(GetBoxUsageBySizeType(3, shelves));
            result.Add(GetBoxUsageBySizeType(4, shelves));
            result.Add(GetBoxUsageBySizeType(5, shelves));
            result.Add(GetBoxUsageBySizeType(6, shelves));
            result.Add(GetBoxUsageBySizeType(7, shelves));
            result.Add(GetBoxUsageBySizeType(8, shelves));

            return result;
        }

        private BoxUsageViewModel GetBoxUsageBySizeType(int sizeType, List<Shelf> shelves)
        {
            int totalBox = 0;
            int boxRemaining = 0;
            double usage = 0;
            BoxUsageViewModel result = new BoxUsageViewModel();
            result.SizeType = sizeType;
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
                    var boxes = shelf.Boxes.Where(x => x.IsActive == true && x.SizeType == sizeType);
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
    }
}
