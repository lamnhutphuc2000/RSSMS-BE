using AutoMapper;
using AutoMapper.QueryableExtensions;
using RSSMS.DataService.Constants;
using RSSMS.DataService.Models;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.Responses;
using RSSMS.DataService.UnitOfWorks;
using RSSMS.DataService.Utilities;
using RSSMS.DataService.ViewModels.Shelves;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
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
    }
    public class ShelfService : BaseService<Shelf>, IShelfService

    {
        private readonly IMapper _mapper;
        public ShelfService(IUnitOfWork unitOfWork, IShelfRepository repository, IMapper mapper) : base(unitOfWork, repository)
        {
            _mapper = mapper;
        }

        public async Task<ShelfViewModel> Create(ShelfCreateViewModel model)
        {
            var shelf = _mapper.Map<Shelf>(model);
            await CreateAsync(shelf);
            return _mapper.Map<ShelfViewModel>(shelf); ;
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
            var shelf = await GetAsync(id);
            if (shelf == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Shelf id not found");
            var result = _mapper.Map<ShelfViewModel>(shelf);
            return result;
        }

        public async Task<ShelfViewModel> Update(int id, ShelfUpdateViewModel model)
        {
            if (id != model.Id) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Shelf Id not matched");

            var entity = await GetAsync(id);
            if (entity == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Shelf not found");

            var updateEntity = _mapper.Map(model, entity);
            await UpdateAsync(updateEntity);

            return _mapper.Map<ShelfViewModel>(updateEntity);
        }
        public async Task<DynamicModelResponse<ShelfViewModel>> GetAll(ShelfViewModel model, string[] fields, int page, int size)
        {
            var shelves = Get(x => x.IsActive == true).ProjectTo<ShelfViewModel>(_mapper.ConfigurationProvider)
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
    }
}
