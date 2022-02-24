//using AutoMapper;
//using Microsoft.EntityFrameworkCore;
//using RSSMS.DataService.Enums;
//using RSSMS.DataService.Models;
//using RSSMS.DataService.Repositories;
//using RSSMS.DataService.Responses;
//using RSSMS.DataService.UnitOfWorks;
//using RSSMS.DataService.ViewModels.BoxOrderDetails;
//using System;
//using System.IdentityModel.Tokens.Jwt;
//using System.Linq;
//using System.Net;
//using System.Threading.Tasks;

//namespace RSSMS.DataService.Services
//{
//    public interface IBoxOrderDetailService : IBaseService<BoxOrderDetail>
//    {
//        Task<BoxOrderDetailCreateViewModel> Create(BoxOrderDetailCreateViewModel model, string accessToken);

//        Task<BoxOrderDetailUpdateViewModel> Update(BoxOrderDetailUpdateViewModel model);
//    }
//    public class BoxOrderDetailService : BaseService<BoxOrderDetail>, IBoxOrderDetailService
//    {
//        private readonly IMapper _mapper;
//        private readonly IBoxService _boxService;
//        private readonly IOrderService _orderSerivce;
//        private readonly INotificationDetailService _notificationDetailService;
//        private readonly INotificationService _notificationService;
//        public BoxOrderDetailService(IUnitOfWork unitOfWork, IBoxService boxService, IOrderService orderSerivce, INotificationDetailService notificationDetailService, INotificationService notificationService, IBoxOrderDetailRepository repository, IMapper mapper) : base(unitOfWork, repository)
//        {
//            _mapper = mapper;
//            _boxService = boxService;
//            _orderSerivce = orderSerivce;
//            _notificationDetailService = notificationDetailService;
//            _notificationService = notificationService;
//        }

//        public async Task<BoxOrderDetailCreateViewModel> Create(BoxOrderDetailCreateViewModel model, string accessToken)
//        {
//            var order = await _orderSerivce.Get(x => x.Id == model.OrderId && x.IsActive == true).Include(x => x.Customer).FirstOrDefaultAsync();
//            order.Status = 4;
//            var customer = order.Customer;
//            _orderSerivce.Update(order);

//            var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
//            var userId = Int32.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);

//            var listBoxes = model.Boxes;


//            foreach (var box in listBoxes)
//            {
//                var boxUpdated = await _boxService.Get(x => x.Id == box.BoxId && x.IsActive == true).FirstOrDefaultAsync();
//                boxUpdated.Status = (int)StorageStatus.Available;
//                _boxService.Update(boxUpdated);
//                var entity = _mapper.Map<BoxOrderDetail>(box);
//                entity.BoxId = box.BoxId;
//                entity.OrderDetailId = box.OrderDetailId;
//                entity.IsActive = true;
//                await CreateAsync(entity);
//            }

//            Notification noti = new Notification
//            {
//                Description = "Don cua ban da duoc luu kho",
//                CreatedDate = DateTime.Now,
//                Type = 0,
//                OrderId = order.Id
//            };
//            await _notificationService.CreateAsync(noti);

//            // noti cho user box da duoc luu kho
//            await _notificationDetailService.SendNoti("Don cua ban da duoc luu kho", userId, customer.Id, customer.DeviceTokenId, noti.Id, order.Id, null, null);
//            return model;
//        }

//        public async Task<BoxOrderDetailUpdateViewModel> Update(BoxOrderDetailUpdateViewModel model)
//        {
//            var entity = await Get(x => x.BoxId == model.BoxId && x.OrderDetailId == model.OrderDetailId && x.IsActive == true).FirstOrDefaultAsync();
//            if (entity == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Box not found");
//            entity.IsActive = false;
//            var oldBox = await _boxService.Get(x => x.Id == model.BoxId && x.IsActive == true).FirstOrDefaultAsync();
//            oldBox.Status = 0;
//            _boxService.Update(oldBox);
//            var newBox = await _boxService.Get(x => x.Id == model.NewBoxId && x.IsActive == true).FirstOrDefaultAsync();
//            newBox.Status = 1;
//            _boxService.Update(newBox);
//            await UpdateAsync(entity);
//            var entityToCreate = _mapper.Map<BoxOrderDetail>(model);
//            await CreateAsync(entityToCreate);
//            return model;
//        }
//    }
//}
