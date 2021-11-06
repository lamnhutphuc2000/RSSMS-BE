using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Constants;
using RSSMS.DataService.Models;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.Responses;
using RSSMS.DataService.UnitOfWorks;
using RSSMS.DataService.Utilities;
using RSSMS.DataService.ViewModels.StaffManageUser;
using RSSMS.DataService.ViewModels.Storages;
using RSSMS.DataService.ViewModels.Users;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
namespace RSSMS.DataService.Services
{
    public interface IStorageService : IBaseService<Storage>
    {
        Task<DynamicModelResponse<StorageViewModel>> GetAll(StorageViewModel model, string[] fields, int page, int size);
        Task<StorageGetIdViewModel> GetById(int id);
        Task<StorageViewModel> Create(StorageCreateViewModel model);
        Task<StorageUpdateViewModel> Update(int id, StorageUpdateViewModel model);
        Task<StorageViewModel> Delete(int id);
        //Task<int> Count(List<StorageViewModel> shelves);
    }
    public class StorageService : BaseService<Storage>, IStorageService
    {
        private readonly IMapper _mapper;
        private readonly IOrderService _orderService;
        private readonly IStaffManageStorageService _staffManageStorageService;
        public StorageService(IUnitOfWork unitOfWork, IStorageRepository repository, IMapper mapper, IOrderService orderService, IStaffManageStorageService staffManageStorageService) : base(unitOfWork, repository)
        {
            _mapper = mapper;
            _orderService = orderService;
            _staffManageStorageService = staffManageStorageService;
        }

        // public Task<int> Count(List<StorageViewModel> shelves)
        // {
        //    throw new NotImplementedException();
        // }

        public async Task<StorageViewModel> Create(StorageCreateViewModel model)
        {
            var storage = _mapper.Map<Storage>(model);
            await CreateAsync(storage);

            foreach (UserListStaffViewModel staffAssigned in model.ListStaff)
            {
                var staffAssignModel = _mapper.Map<StaffManageStorageCreateViewModel>(storage);
                staffAssignModel.UserId = staffAssigned.Id;
                await _staffManageStorageService.Create(staffAssignModel);
            }

            return _mapper.Map<StorageViewModel>(storage); ;

        }

        public async Task<StorageViewModel> Delete(int id)
        {
            var entity = await GetAsync<int>(id);
            if (entity == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Storage id not found");
            entity.IsActive = false;
            await UpdateAsync(entity);
            return _mapper.Map<StorageViewModel>(entity);
        }

        public async Task<DynamicModelResponse<StorageViewModel>> GetAll(StorageViewModel model, string[] fields, int page, int size)
        {

            var storages = Get(x => x.IsActive == true)
                    .Include(a => a.Product)
                    .Include(a => a.StaffManageStorages.Where(s => s.RoleName == "Manager"))
                    .ThenInclude(a => a.User).ProjectTo<StorageViewModel>(_mapper.ConfigurationProvider).DynamicFilter(model)
                    .PagingIQueryable(page, size, CommonConstant.LimitPaging, CommonConstant.DefaultPaging);


            if (storages.Item2.ToList().Count < 1) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Can not found");
            var rs = new DynamicModelResponse<StorageViewModel>
            {
                Metadata = new PagingMetaData
                {
                    Page = page,
                    Size = size,
                    Total = storages.Item1,
                    TotalPage = (int)Math.Ceiling((double)storages.Item1 / size)
                },
                Data = storages.Item2.ToList()
            };
            return rs;
        }

        public async Task<StorageGetIdViewModel> GetById(int id)
        {
            var result = await Get(x => x.Id == id && x.IsActive == true)
                .Include(a => a.OrderStorageDetails.Where(s => s.IsActive == true))
                .Include(a => a.Product)
                .Include(a => a.StaffManageStorages.Where(s => s.RoleName == "Manager"))
                .ThenInclude(a => a.User).ProjectTo<StorageGetIdViewModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            if (result == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Storage id not found");
            if (result.OrderId != null)
            {
                result.OrderInfo = await _orderService.GetSelfStorageOrderInfo((int)result.OrderId);
            }

            return result;
        }

        public async Task<StorageUpdateViewModel> Update(int id, StorageUpdateViewModel model)
        {
            if (id != model.Id) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Storage Id not matched");

            var entity = await GetAsync(id);
            if (entity == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Storage not found");

            var updateEntity = _mapper.Map(model, entity);
            await UpdateAsync(updateEntity);

            return _mapper.Map<StorageUpdateViewModel>(updateEntity);
        }
    }
}
