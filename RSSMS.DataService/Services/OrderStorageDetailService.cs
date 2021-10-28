using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Models;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.Responses;
using RSSMS.DataService.UnitOfWorks;
using RSSMS.DataService.ViewModels.OrderStorages;
using System.Net;
using System.Threading.Tasks;

namespace RSSMS.DataService.Services
{
    public interface IOrderStorageDetailService : IBaseService<OrderStorageDetail>
    {
        Task<OrderStorageDetailViewModel> Create(OrderStorageDetailViewModel model);
    }
    public class OrderStorageDetailService : BaseService<OrderStorageDetail>, IOrderStorageDetailService

    {
        private readonly IMapper _mapper;
        private readonly IStorageService _storageService;
        private readonly IOrderService _orderService;
        public OrderStorageDetailService(IUnitOfWork unitOfWork, IStorageService storageService, IOrderService orderService, IOrderStorageDetailRepository repository, IMapper mapper) : base(unitOfWork, repository)
        {
            _mapper = mapper;
            _storageService = storageService;
            _orderService = orderService;
        }

        public async Task<OrderStorageDetailViewModel> Create(OrderStorageDetailViewModel model)
        {
            var storageIds = model.StorageIds;
            var orderStorages = await Get(x => storageIds.Contains(x.StorageId) && x.IsActive == true).FirstOrDefaultAsync();
            if (orderStorages != null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Storage had been used");
            var storages = await _storageService.Get(x => storageIds.Contains(x.Id) && x.IsActive == false).FirstOrDefaultAsync();
            if (storages != null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Storage not found");
            var order = await _orderService.Get(x => x.Id == model.OrderId  && x.IsActive == true).FirstOrDefaultAsync();
            if(order == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Order not found");
            if(order.Status != 0) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Order had assigned to storage");
            foreach (var storageId in storageIds)
            {
                var storage = await _storageService.Get(x => x.Id == storageId).FirstOrDefaultAsync();
                storage.Status = 1;
                await _storageService.UpdateAsync(storage);
                var entity = _mapper.Map<OrderStorageDetail>(model);
                entity.StorageId = storageId;
                entity.IsActive = true;
                await CreateAsync(entity);
            }
            order.Status = 1;
            await _orderService.UpdateAsync(order);
            return model;
        }

    }
}
