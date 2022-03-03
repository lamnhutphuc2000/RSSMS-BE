using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Enums;
using RSSMS.DataService.Models;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.UnitOfWorks;
using RSSMS.DataService.ViewModels.OrderBoxes;
using System;
using System.Threading.Tasks;

namespace RSSMS.DataService.Services
{
    public interface IBoxService : IBaseService<Box>
    {
        Task CreateNumberOfBoxes(Guid shelfId, int num, Guid serviceId, Guid staffId);
        Task Delete(Guid shelfId, Guid staffId);
        Task UpdateBoxSize(Guid serviceId, Guid shelfId);
        Task<OrderBoxesDetailViewModel> Create(OrderBoxesDetailViewModel model);

        Task<OrderBoxesMoveViewModel> Update(OrderBoxesMoveViewModel model);
    }
    public class BoxService : BaseService<Box>, IBoxService
    {
        private readonly IOrderService _orderSerivce;
        public BoxService(IUnitOfWork unitOfWork,IOrderService orderSerivce, IBoxRepository repository) : base(unitOfWork, repository)
        {
            _orderSerivce = orderSerivce;
        }

        public async Task CreateNumberOfBoxes(Guid shelfId, int num, Guid serviceId, Guid staffId)
        {
            for (int i = 0; i < num; i++)
            {
                Box box = new Box();
                box.IsActive = true;
                box.ServiceId = serviceId;
                box.ShelfId = shelfId;
                box.Status = 0;
                box.ModifiedBy = staffId;
                await CreateAsync(box);
            }
        }

        public async Task Delete(Guid shelfId, Guid staffId)
        {
            var listBoxes = await Get(x => x.ShelfId == shelfId && x.IsActive == true).ToListAsync();
            if (listBoxes == null) return;
            foreach (var box in listBoxes)
            {
                box.IsActive = false;
                box.ModifiedBy = staffId;
                await UpdateAsync(box);
            }
        }

        public async Task UpdateBoxSize(Guid serviceId, Guid shelfId)
        {
            var listBoxes = await Get(x => x.ShelfId == shelfId && x.IsActive == true && x.ServiceId != serviceId).ToListAsync();
            if (listBoxes == null) return;
            foreach (var box in listBoxes)
            {
                box.ServiceId = serviceId;
                await UpdateAsync(box);
            }
        }

        public async Task<OrderBoxesDetailViewModel> Create(OrderBoxesDetailViewModel model)
        {
            //var order = await _orderSerivce.Get(x => x.Id == model.OrderId && x.IsActive == true).Include(x => x.OrderDetails).FirstOrDefaultAsync();
            //order.Status = 4;
            //_orderSerivce.Update(order);
            //var listBoxesId = model.BoxesId;
            //foreach (var boxId in listBoxesId)
            //{
            //    var box = await Get(x => x.Id == boxId && x.IsActive == true).FirstOrDefaultAsync();
            //    box.Status = (int)StorageStatus.Available;
            //    box.OrderDetailId
            //    Update(box);
            //}
            //return model;
            return null;
        }

        public async Task<OrderBoxesMoveViewModel> Update(OrderBoxesMoveViewModel model)
        {
            //var entity = await Get(x => x.Id == model.BoxId && x.OrderDetailId == model.OrderId && x.IsActive == true).FirstOrDefaultAsync();
            //if (entity == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Box not found");
            //entity.IsActive = false;
            //var oldBox = await _boxService.Get(x => x.Id == model.BoxId && x.IsActive == true).FirstOrDefaultAsync();
            //oldBox.Status = 0;
            //_boxService.Update(oldBox);
            //var newBox = await _boxService.Get(x => x.Id == model.NewBoxId && x.IsActive == true).FirstOrDefaultAsync();
            //newBox.Status = 1;
            //_boxService.Update(newBox);
            //await UpdateAsync(entity);
            //var entityToCreate = _mapper.Map<OrderBoxDetail>(model);
            //await CreateAsync(entityToCreate);
            //return model;
            return null;
        }
    }
}
