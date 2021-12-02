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
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RSSMS.DataService.Services
{

    public interface IAreaService : IBaseService<Area>
    {
        Task<DynamicModelResponse<AreaViewModel>> GetByStorageId(int id, AreaViewModel model, string[] fields, int page, int size);
        Task<AreaViewModel> Create(AreaCreateViewModel model);
        Task<AreaViewModel> Delete(int id);
        Task<AreaViewModel> Update(int id, AreaUpdateViewModel model);
        Task<AreaDetailViewModel> GetById(int id);
    }
    public class AreaService : BaseService<Area>, IAreaService

    {
        private readonly IMapper _mapper;
        private readonly IShelfService _shelfService;
        public AreaService(IUnitOfWork unitOfWork, IShelfService shelfService, IAreaRepository repository, IMapper mapper) : base(unitOfWork, repository)
        {
            _mapper = mapper;
            _shelfService = shelfService;
        }

        public async Task<AreaViewModel> Create(AreaCreateViewModel model)
        {
            var area = Get(x => x.StorageId == model.StorageId && x.Name == model.Name && x.IsActive == true).FirstOrDefault();
            if (area != null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Area name existed");
            var areaToCreate = _mapper.Map<Area>(model);
            await CreateAsync(areaToCreate);
            return _mapper.Map<AreaViewModel>(areaToCreate); ;
        }

        public async Task<AreaViewModel> Delete(int id)
        {
            var area = await Get(x => x.Id == id && x.IsActive == true).FirstOrDefaultAsync();
            if (area == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Area id not found");
            area.IsActive = false;
            await UpdateAsync(area);
            return _mapper.Map<AreaViewModel>(area);
        }

        public async Task<AreaDetailViewModel> GetById(int id)
        {
            var area = await Get(x => x.Id == id && x.IsActive == true).FirstOrDefaultAsync();
            if (area == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Area id not found");
            var result = _mapper.Map<AreaDetailViewModel>(area);
            var boxUsage = _shelfService.GetBoxUsageByAreaId(id);
            result.BoxUsage = boxUsage;
            return result;
        }

        public async Task<DynamicModelResponse<AreaViewModel>> GetByStorageId(int id, AreaViewModel model, string[] fields, int page, int size)
        {
            var result = Get(x => x.StorageId == id && x.IsActive == true)
                .ProjectTo<AreaViewModel>(_mapper.ConfigurationProvider)
                 .DynamicFilter(model)
                .PagingIQueryable(page, size, CommonConstant.LimitPaging, CommonConstant.DefaultPaging);
            if (result.Item2.ToList().Count < 1) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Storage id not found");

            var rs = new DynamicModelResponse<AreaViewModel>
            {

                Metadata = new PagingMetaData
                {
                    Page = page,
                    Size = size,
                    Total = result.Item1,
                    TotalPage = (int)Math.Ceiling((double)result.Item1 / size)
                },
                Data = result.Item2.ToList()
            };

            return rs;
        }

        public async Task<AreaViewModel> Update(int id, AreaUpdateViewModel model)
        {
            if (id != model.Id) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Area Id not matched");

            var entity = await Get(x => x.Id == id && x.IsActive == true).FirstOrDefaultAsync();
            if (entity == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Area not found");

            var area = Get(x => x.Id != id && x.Name == model.Name && x.StorageId == entity.StorageId && x.IsActive == true).FirstOrDefault();
            if (area != null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Area name existed");
            var updateEntity = _mapper.Map(model, entity);
            await UpdateAsync(updateEntity);

            return _mapper.Map<AreaViewModel>(updateEntity);
        }
    }
}
