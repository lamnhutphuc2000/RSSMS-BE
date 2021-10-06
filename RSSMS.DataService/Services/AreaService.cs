using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using RSSMS.DataService.Models;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.UnitOfWorks;
using RSSMS.DataService.ViewModels.Areas;
using RSSMS.DataService.Responses;
using System.Net;
using RSSMS.DataService.Constants;
using RSSMS.DataService.Utilities;
using System.Linq;
using System;

namespace RSSMS.DataService.Services
{

    public interface IAreaService : IBaseService<Area>
    {
        Task<DynamicModelResponse<AreaViewModel>> GetByStorageId(int id, AreaViewModel model, string[] fields, int page, int size);
        Task<AreaViewModel> Create(AreaCreateViewModel model);
        Task<AreaViewModel> Delete(int id);
        Task<AreaViewModel> Update(int id, AreaUpdateViewModel model);
    }
    public class AreaService : BaseService<Area>, IAreaService

    {
        private readonly IMapper _mapper;
        private readonly IStorageService _storageService;
        public AreaService(IUnitOfWork unitOfWork, IStorageService storageSerivce,IAreaRepository repository, IMapper mapper) : base(unitOfWork, repository)
        {
            _mapper = mapper;
            _storageService = storageSerivce;
        }

        public async Task<AreaViewModel> Create(AreaCreateViewModel model)
        {
            var area = _mapper.Map<Area>(model);
            await CreateAsync(area);
            return _mapper.Map<AreaViewModel>(area); ;
        }

        public async Task<AreaViewModel> Delete(int id)
        {
            var entity = await GetAsync<int>(id);
            if (entity == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Area id not found");
            entity.IsActive = false;
            await UpdateAsync(entity);
            return _mapper.Map<AreaViewModel>(entity);
        }

        public async Task<DynamicModelResponse<AreaViewModel>> GetByStorageId(int id, AreaViewModel model, string[] fields, int page, int size)
        {
            var result =  Get(x => x.StorageId == id && x.IsActive == true)
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

            var entity = await GetAsync(id);
            if (entity == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Area not found");

            var updateEntity = _mapper.Map(model, entity);
            await UpdateAsync(updateEntity);

            return _mapper.Map<AreaViewModel>(updateEntity);
        }
    }
}
