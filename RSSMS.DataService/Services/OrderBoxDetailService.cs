//using AutoMapper;
//using Microsoft.EntityFrameworkCore;
//using RSSMS.DataService.Enums;
//using RSSMS.DataService.Models;
//using RSSMS.DataService.Repositories;
//using RSSMS.DataService.Responses;
//using RSSMS.DataService.UnitOfWorks;
//using RSSMS.DataService.ViewModels.OrderBoxes;
//using System.Net;
//using System.Threading.Tasks;

//namespace RSSMS.DataService.Services
//{
//    public interface IOrderBoxDetailService : IBaseService<OrderBoxDetail>
//    {
//        Task<OrderBoxesDetailViewModel> Create(OrderBoxesDetailViewModel model);

//        Task<OrderBoxesMoveViewModel> Update(OrderBoxesMoveViewModel model);
//    }
//    public class OrderBoxDetailService : BaseService<OrderBoxDetail>, IOrderBoxDetailService

//    {
//        private readonly IMapper _mapper;
//        private readonly IBoxService _boxService;
//        private readonly IOrderService _orderSerivce;
//        public OrderBoxDetailService(IUnitOfWork unitOfWork, IBoxService boxService, IOrderService orderSerivce, IOrderBoxDetailRepository repository, IMapper mapper) : base(unitOfWork, repository)
//        {
//            _mapper = mapper;
//            _boxService = boxService;
//            _orderSerivce = orderSerivce;
//        }

//        public async Task<OrderBoxesDetailViewModel> Create(OrderBoxesDetailViewModel model)
//        {
//            var order = await _orderSerivce.Get(x => x.Id == model.OrderId && x.IsActive == true).FirstOrDefaultAsync();
//            order.Status = 4;
//            _orderSerivce.Update(order);
//            var listBoxesId = model.BoxesId;
//            foreach (var boxId in listBoxesId)
//            {
//                var box = await _boxService.Get(x => x.Id == boxId && x.IsActive == true).FirstOrDefaultAsync();
//                box.Status = (int)StorageStatus.Available;
//                _boxService.Update(box);
//                var entity = _mapper.Map<OrderBoxDetail>(model);
//                entity.BoxId = boxId;
//                await CreateAsync(entity);
//            }
//            return model;
//        }

//        public async Task<OrderBoxesMoveViewModel> Update(OrderBoxesMoveViewModel model)
//        {
//            var entity = await Get(x => x.BoxId == model.BoxId && x.OrderId == model.OrderId && x.IsActive == true).FirstOrDefaultAsync();
//            if (entity == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Box not found");
//            entity.IsActive = false;
//            var oldBox = await _boxService.Get(x => x.Id == model.BoxId && x.IsActive == true).FirstOrDefaultAsync();
//            oldBox.Status = 0;
//            _boxService.Update(oldBox);
//            var newBox = await _boxService.Get(x => x.Id == model.NewBoxId && x.IsActive == true).FirstOrDefaultAsync();
//            newBox.Status = 1;
//            _boxService.Update(newBox);
//            await UpdateAsync(entity);
//            var entityToCreate = _mapper.Map<OrderBoxDetail>(model);
//            await CreateAsync(entityToCreate);
//            return model;
//        }
//    }
//}
