using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Models;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.Responses;
using RSSMS.DataService.UnitOfWorks;
using RSSMS.DataService.ViewModels.OrderStorages;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RSSMS.DataService.Services
{
    public interface IOrderStorageDetailService : IBaseService<OrderStorageDetail>
    {
        Task<OrderStorageDetailViewModel> Create(OrderStorageDetailViewModel model, string accessToken);
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

        public async Task<OrderStorageDetailViewModel> Create(OrderStorageDetailViewModel model, string accessToken)
        {
            var storageId = model.StorageId;
            var storages = await _storageService.Get(x => x.Id == storageId && x.IsActive == false).FirstOrDefaultAsync();
            if (storages != null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Storage not found");
            var order = await _orderService.Get(x => x.Id == model.OrderId && x.IsActive == true).Include(x => x.Customer).FirstOrDefaultAsync();
            if (order == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Order not found");
            if (order.Status > 1) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Order had assigned to storage");

            var entity = _mapper.Map<OrderStorageDetail>(model);
            entity.StorageId = storageId;
            entity.IsActive = true;
            await CreateAsync(entity);


            var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
            var userId = Int32.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);
            order.ManagerId = userId;
            order.Status = 2;
            await _orderService.UpdateAsync(order);
            return model;
        }

    }
}
