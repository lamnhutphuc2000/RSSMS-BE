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
        Task<NotificationViewModel> Update(int id, NotificationUpdateViewModel model, string accessToken);
    }
    public class NotificationService : BaseService<Models.Notification>, INotificationService
    {
        private readonly IMapper _mapper;
        public NotificationService(IUnitOfWork unitOfWork, INotificationRepository repository, IMapper mapper) : base(unitOfWork, repository)
        {
            _mapper = mapper;
        }


        public async Task<DynamicModelResponse<NotificationViewModel>> GetAll(int userId, string[] fields, int page, int size)
        {
            var notifications = Get(x => x.IsActive == true && x.NotificationDetails.Any(a => a.UserId == userId && a.IsActive == true))
                .Include(a => a.NotificationDetails.Where(s => s.UserId == userId && s.IsActive == true)).ProjectTo<NotificationViewModel>(_mapper.ConfigurationProvider)
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

        public async Task<NotificationViewModel> Update(int id, NotificationUpdateViewModel model, string accessToken)
        {
            if (id != model.Id) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Notification Id not matched");

            var entity = Get(x => x.Id == id && x.IsActive == true).Include(x => x.NotificationDetails);
            if (entity == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Notification not found");

            var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
            var userId = Int32.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);

            var entityToUpdate = await entity.Where(x => x.NotificationDetails.Any(notificationDetail => notificationDetail.UserId == userId && notificationDetail.IsActive == true && notificationDetail.NotificationId == id)).FirstOrDefaultAsync();
            if (entity == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Notification not found");

            var notificationDetail = entityToUpdate.NotificationDetails;
            foreach(var noti in notificationDetail)
            {
                noti.IsRead = model.IsRead;
            }
            entityToUpdate.NotificationDetails = notificationDetail;

            await UpdateAsync(entityToUpdate);

            return _mapper.Map<NotificationViewModel>(entityToUpdate);
        }
    }
}
