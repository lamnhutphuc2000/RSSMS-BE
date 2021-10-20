using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.UnitOfWorks;
using RSSMS.DataService.ViewModels.OrderStorages;
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
        public OrderStorageDetailService(IUnitOfWork unitOfWork, IOrderStorageDetailRepository repository, IMapper mapper) : base(unitOfWork, repository)
        {
            _mapper = mapper;
        }

        public async Task<OrderStorageDetailViewModel> Create(OrderStorageDetailViewModel model)
        {
            var entity = _mapper.Map<OrderStorageDetail>(model);
            await CreateAsync(entity);
            return model;
        }

    }
}
