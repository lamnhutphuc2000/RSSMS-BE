using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Models;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.Responses;
using RSSMS.DataService.UnitOfWorks;
using RSSMS.DataService.Utilities;
using RSSMS.DataService.ViewModels.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RSSMS.DataService.Services
{
    public interface IServiceService : IBaseService<Service>
    {
        Task<Dictionary<string, List<ServicesViewModel>>> GetAll(ServicesViewModel model);
        Task<ServicesViewModel> GetById(Guid id);
        Task<ServicesViewModel> Create(ServicesCreateViewModel model);
        Task<ServicesUpdateViewModel> Update(Guid id, ServicesUpdateViewModel model);
        Task<ServicesViewModel> Delete(Guid id);
    }
    public class ServiceService : BaseService<Service>, IServiceService
    {
        private readonly IMapper _mapper;
        private readonly IFirebaseService _firebaseService;
        public ServiceService(IUnitOfWork unitOfWork, IServiceRepository repository, IMapper mapper, IFirebaseService firebaseService) : base(unitOfWork, repository)
        {
            _mapper = mapper;
            _firebaseService = firebaseService;
        }

        public async Task<ServicesViewModel> Create(ServicesCreateViewModel model)
        {
            try
            {
                var service = _mapper.Map<Service>(model);
                await CreateAsync(service);
                var image = model.Image;
                if (image != null)
                {
                    var url = await _firebaseService.UploadImageToFirebase(image.File, "services", service.Id, "avatar");
                    if (url != null) service.ImageUrl = url;
                }
                await UpdateAsync(service);


                return _mapper.Map<ServicesViewModel>(service);
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

        public async Task<ServicesViewModel> Delete(Guid id)
        {
            try
            {
                var entity = await Get(x => x.Id == id && x.IsActive == true).FirstOrDefaultAsync();
                if (entity == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Service id not found");
                entity.IsActive = false;
                await UpdateAsync(entity);
                return _mapper.Map<ServicesViewModel>(entity);
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

        public async Task<Dictionary<string, List<ServicesViewModel>>> GetAll(ServicesViewModel model)
        {
            try
            {
                var services = Get(x => x.IsActive == true).OrderBy(x => x.Type)
                    .ProjectTo<ServicesViewModel>(_mapper.ConfigurationProvider)
                    .DynamicFilter(model);
                if (services.ToList().Count == 0) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Service not found");


                Dictionary<string, List<ServicesViewModel>> result = new Dictionary<string, List<ServicesViewModel>>();
                var serviceLists = await services.ToListAsync();
                var distinctServiceList = serviceLists.GroupBy(m => new { m.Type })
                    .Select(group => group.First()).ToList();

                foreach (var distinctService in distinctServiceList)
                {
                    result.Add(distinctService.Type.ToString(), services.Where(x => x.Type == distinctService.Type).ToList());
                }
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

        public async Task<ServicesViewModel> GetById(Guid id)
        {
            try
            {

                var result = await Get(x => x.Id == id && x.IsActive == true)
                    .ProjectTo<ServicesViewModel>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync();
                if (result == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Service id not found");
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

        public async Task<ServicesUpdateViewModel> Update(Guid id, ServicesUpdateViewModel model)
        {
            try
            {

                if (id != model.Id) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Service Id not matched");

                var entity = await Get(x => x.Id == id && x.IsActive == true).FirstOrDefaultAsync();
                if (entity == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Service not found");
                entity.IsActive = false;
                await UpdateAsync(entity);

                var updateEntity = _mapper.Map(model, entity);

                var image = model.Image;
                if (image != null)
                {
                    if (image.Url != null) updateEntity.ImageUrl = image.Url;
                    else if (image.File != null)
                    {
                        var url = await _firebaseService.UploadImageToFirebase(image.File, "services", id, "avatar");
                        if (url != null) updateEntity.ImageUrl = url;
                    }
                }
                updateEntity.Id = Guid.NewGuid();
                updateEntity.IsActive = true;
                await CreateAsync(updateEntity);

                return _mapper.Map<ServicesUpdateViewModel>(updateEntity);
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
