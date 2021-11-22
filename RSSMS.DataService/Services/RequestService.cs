using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Constants;
using RSSMS.DataService.Models;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.Responses;
using RSSMS.DataService.UnitOfWorks;
using RSSMS.DataService.Utilities;
using RSSMS.DataService.ViewModels.Requests;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RSSMS.DataService.Services
{
    public interface IRequestService : IBaseService<Request>
    {
        Task<DynamicModelResponse<RequestViewModel>> GetAll(RequestViewModel model, string[] fields, int page, int size);
        Task<RequestViewModel> GetById(int id);
        Task<RequestCreateViewModel> Create(RequestCreateViewModel model);
        Task<RequestUpdateViewModel> Update(int id, RequestUpdateViewModel model);
        Task<RequestViewModel> Delete(int id);
    }


    public class RequestService : BaseService<Request>, IRequestService
    {
        private readonly IMapper _mapper;
        public RequestService(IUnitOfWork unitOfWork, IRequestRepository repository, IMapper mapper) : base(unitOfWork, repository)
        {
            _mapper = mapper;
        }

        public async Task<RequestViewModel> Delete(int id)
        {
            var entity = await GetAsync<int>(id);
            if (entity == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Request id not found");
            entity.IsActive = false;
            await UpdateAsync(entity);
            return _mapper.Map<RequestViewModel>(entity);
        }

        public async Task<DynamicModelResponse<RequestViewModel>> GetAll(RequestViewModel model, string[] fields, int page, int size)
        {
            var requests = Get(x => x.IsActive == true)
                    .ProjectTo<RequestViewModel>(_mapper.ConfigurationProvider)
                    .PagingIQueryable(page, size, CommonConstant.LimitPaging, CommonConstant.DefaultPaging);


            if (requests.Item2.ToList().Count < 1) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Can not found");
            var rs = new DynamicModelResponse<RequestViewModel>
            {
                Metadata = new PagingMetaData
                {
                    Page = page,
                    Size = size,
                    Total = requests.Item1,
                    TotalPage = (int)Math.Ceiling((double)requests.Item1 / size)
                },
                Data = requests.Item2.ToList()
            };
            return rs;
        }

        public async Task<RequestViewModel> GetById(int id)
        {
            var result = await Get(x => x.Id == id && x.IsActive == true)
               .ProjectTo<RequestViewModel>(_mapper.ConfigurationProvider)
               .FirstOrDefaultAsync();
            if (result == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Request id not found");
            return result;
        }

        public async Task<RequestViewModel> Update(int id, RequestViewModel model)
        {
            if (id != model.Id) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Request Id not matched");

            var entity = await GetAsync(id);
            if (entity == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Request not found");

            var updateEntity = _mapper.Map(model, entity);
            await UpdateAsync(updateEntity);

            return _mapper.Map<RequestViewModel>(updateEntity);
        }

        public async Task<RequestCreateViewModel> Create(RequestCreateViewModel model)
        {
            var request = _mapper.Map<Request>(model);
            await CreateAsync(request);
            return _mapper.Map<RequestCreateViewModel>(request);
        }

        public async Task<RequestUpdateViewModel> Update(int id, RequestUpdateViewModel model)
        {
            if (id != model.Id) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Request Id not matched");

            var entity = await GetAsync(id);
            if (entity == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Request not found");

            var updateEntity = _mapper.Map(model, entity);
            await UpdateAsync(updateEntity);

            return _mapper.Map<RequestUpdateViewModel>(updateEntity);
        }
    }
}
