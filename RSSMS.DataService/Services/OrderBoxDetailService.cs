using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Models;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.UnitOfWorks;
using RSSMS.DataService.ViewModels.OrderBoxes;
using System.Threading.Tasks;

namespace RSSMS.DataService.Services
{
    public interface IOrderBoxDetailService : IBaseService<OrderBoxDetail>
    {
        Task<OrderBoxesDetailViewModel> Create(OrderBoxesDetailViewModel model);

        Task<OrderBoxesMoveViewModel> Update(OrderBoxesMoveViewModel model);
    }
    public class OrderBoxDetailService : BaseService<OrderBoxDetail>, IOrderBoxDetailService

    {
        private readonly IMapper _mapper;
        public OrderBoxDetailService(IUnitOfWork unitOfWork, IOrderBoxDetailRepository repository, IMapper mapper) : base(unitOfWork, repository)
        {
            _mapper = mapper;
        }

        public async Task<OrderBoxesDetailViewModel> Create(OrderBoxesDetailViewModel model)
        {
            var listBoxesId = model.BoxesId;
            foreach (var boxId in listBoxesId)
            {
                var entity = _mapper.Map<OrderBoxDetail>(model);
                entity.BoxId = boxId;
                await CreateAsync(entity);
            }
            return model;
        }

        public async Task<OrderBoxesMoveViewModel> Update(OrderBoxesMoveViewModel model)
        {
            var entity = await Get(x => x.BoxId == model.BoxId && x.OrderId == model.OrderId && x.IsActive == true).FirstOrDefaultAsync();
            entity.IsActive = false;
            await UpdateAsync(entity);
            var entityToCreate = _mapper.Map<OrderBoxDetail>(model);
            await CreateAsync(entityToCreate);
            return model;
        }
    }
}
