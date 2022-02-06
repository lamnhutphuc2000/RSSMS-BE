using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Constants;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.Responses;
using RSSMS.DataService.UnitOfWorks;
using RSSMS.DataService.Utilities;
using RSSMS.DataService.ViewModels.Notifications;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RSSMS.DataService.Services
{
    public interface INotificationService : IBaseService<Models.Notification>
    {
        Task<DynamicModelResponse<NotificationViewModel>> GetAll(int userId, string[] fields, int page, int size);
        Task<NotificationUpdateViewModel> Update(NotificationUpdateViewModel model, string accessToken);
    }
    public class NotificationService : BaseService<Models.Notification>, INotificationService
    {
        private readonly IMapper _mapper;
        private readonly INotificationDetailService _notificationDetailService;
        public NotificationService(IUnitOfWork unitOfWork, INotificationDetailService notificationDetailService, INotificationRepository repository, IMapper mapper) : base(unitOfWork, repository)
        {
            _mapper = mapper;
            _notificationDetailService = notificationDetailService;
        }


        public async Task<DynamicModelResponse<NotificationViewModel>> GetAll(int userId, string[] fields, int page, int size)
        {
            var notifications = Get(x => x.IsActive == true && x.NotificationDetails.Any(a => a.UserId == userId && a.IsActive == true))
                .Include(a => a.NotificationDetails.Where(r => r.UserId == userId)).ToList().AsQueryable()
                .OrderByDescending(x => x.CreateDate).ProjectTo<NotificationViewModel>(_mapper.ConfigurationProvider)
                .PagingIQueryable(page, size, CommonConstant.LimitPaging, CommonConstant.DefaultPaging);

            if (notifications.Item2.ToList().Count < 1) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Can not found");
            var rs = new DynamicModelResponse<NotificationViewModel>
            {
                Metadata = new PagingMetaData
                {
                    Page = page,
                    Size = size,
                    Total = notifications.Item1,
                    TotalPage = (int)Math.Ceiling((double)notifications.Item1 / size)
                },
                Data = notifications.Item2.ToList()
            };
            return rs;
        }

        public async Task<NotificationUpdateViewModel> Update(NotificationUpdateViewModel model, string accessToken)
        {
            var listNotiIds = model.Ids;
            var listNoti = Get(x => listNotiIds.Contains(x.Id) && x.IsActive == true).Include(x => x.NotificationDetails).ToList();
            if (listNoti.Count < listNotiIds.Count) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Notification not found");

            var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
            var userId = Int32.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);

            foreach(var noti in listNoti)
            {
                var notificationDetails = noti.NotificationDetails;
                foreach(var notificationDetail in notificationDetails)
                {
                    if(notificationDetail.UserId == (int)userId && notificationDetail.IsActive == true)
                    {
                        notificationDetail.IsRead = true;
                        await _notificationDetailService.UpdateAsync(notificationDetail);
                    }
                }
            }

            return model;
        }
    }
}
