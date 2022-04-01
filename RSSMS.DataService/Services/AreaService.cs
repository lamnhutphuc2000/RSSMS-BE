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
        bool CheckIsUsed(Guid id);
    }
    public class AreaService : BaseService<Area>, IAreaService

    {
        private readonly IMapper _mapper;
        private readonly ISpaceService _spaceService;
        private readonly IFloorsService _floorService;
        public AreaService(IUnitOfWork unitOfWork, ISpaceService spaceService, IAreaRepository repository, IMapper mapper) : base(unitOfWork, repository)
        {
            _mapper = mapper;
            _spaceService = spaceService;
        }

        public async Task<AreaViewModel> Create(AreaCreateViewModel model)
        {
            var area = Get(x => x.StorageId == model.StorageId && x.Name == model.Name && x.IsActive == true).FirstOrDefault();
            if (area != null) throw new ErrorResponse((int)HttpStatusCode.Conflict, "Area name existed");
            var areaToCreate = _mapper.Map<Area>(model);
            await CreateAsync(areaToCreate);
            return _mapper.Map<AreaViewModel>(areaToCreate);
        }

        public async Task<AreaViewModel> Delete(Guid id)
        {
            var area = await Get(x => x.Id == id && x.IsActive == true).Include(a => a.Spaces).FirstOrDefaultAsync();
            if (area == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Area id not found");
            var areaIsUsed = CheckIsUsed(id);
            if (areaIsUsed) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Area is in used");
            area.IsActive = false;
            await UpdateAsync(area);
            return _mapper.Map<AreaViewModel>(area);
        }

        public async Task<AreaDetailViewModel> GetById(Guid id)
        {
            var area = await Get(x => x.Id == id && x.IsActive == true).Include(area => area.Spaces).ThenInclude(space => space.Floors).FirstOrDefaultAsync();
            if (area == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Area id not found");
            var result = _mapper.Map<AreaDetailViewModel>(area);
            var spaces = area.Spaces;
            if (spaces.Count == 0) return result;

            double usage = 0;
            double used = 0;
            double available = 0;
            foreach (var space in spaces)
            {
                var spaceById = await _spaceService.GetById(space.Id);
                usage += spaceById.Floors.Select(x => x.Usage).Sum();
            }

            double total = (double)(area.Height * area.Width * area.Length);
            used = usage * total / 100;
            available = total - used;

            result.Usage = usage;
            result.Used = used;
            result.Available = available;

            return result;
        }

        public async Task<DynamicModelResponse<AreaViewModel>> GetByStorageId(Guid id, AreaViewModel model, List<int> types, string[] fields, int page, int size)
        {
            var areas = Get(x => x.StorageId == id && x.IsActive == true)
                .ProjectTo<AreaViewModel>(_mapper.ConfigurationProvider);

            if (types.Count > 0)
            {
                areas = areas.Where(x => types.Contains((int)x.Type));
            }

            var result = areas
                 .DynamicFilter(model)
                .PagingIQueryable(page, size, CommonConstant.LimitPaging, CommonConstant.DefaultPaging);
            if (result.Item2.ToList().Count < 1) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Area not found");



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

        public async Task<AreaViewModel> Update(Guid id, AreaUpdateViewModel model)
        {
            if (id != model.Id) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Area Id not matched");

            var entity = await Get(x => x.Id == id && x.IsActive == true).FirstOrDefaultAsync();
            if (entity == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Area not found");

            var area = Get(x => x.Id != id && x.Name == model.Name && x.StorageId == entity.StorageId && x.IsActive == true).FirstOrDefault();
            if (area != null) throw new ErrorResponse((int)HttpStatusCode.Conflict, "Area name existed");
            var updateEntity = _mapper.Map(model, entity);
            await UpdateAsync(updateEntity);

            return _mapper.Map<AreaViewModel>(updateEntity);
        }

        public bool CheckIsUsed(Guid id)
        {
            var area = Get(x => x.Id == id && x.IsActive == true).Include(a => a.Spaces).FirstOrDefault();
            if (area == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Area id not found");
            var spaces = area.Spaces;
            foreach (var space in spaces)
            {
                if (_spaceService.CheckIsUsed(space.Id)) return true;
            }
            return false;
        }
    }
}
