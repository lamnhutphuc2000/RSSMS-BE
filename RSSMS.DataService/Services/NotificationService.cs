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
        Task<DynamicModelResponse<NotificationViewModel>> GetAll(Guid userId, string[] fields, int page, int size);
        Task<NotificationUpdateViewModel> Update(NotificationUpdateViewModel model, string accessToken);
    }
    public class NotificationService : BaseService<Models.Notification>, INotificationService
    {
        private readonly IMapper _mapper;
        public NotificationService(IUnitOfWork unitOfWork, INotificationRepository repository, IMapper mapper) : base(unitOfWork, repository)
        {
            _mapper = mapper;
        }


        public async Task<DynamicModelResponse<NotificationViewModel>> GetAll(Guid userId, string[] fields, int page, int size)
        {
            try
            {
                var notifications = Get(notification => notification.ReceiverId == userId)
                .ToList().AsQueryable()
                .OrderByDescending(x => x.CreatedDate).ProjectTo<NotificationViewModel>(_mapper.ConfigurationProvider)
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
            catch (ErrorResponse e)
            {
                throw new ErrorResponse((int)e.Error.Code, e.Error.Message);
            }
            catch (Exception ex)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, "" + ex.Message);
            }
            
        }

        public async Task<NotificationUpdateViewModel> Update(NotificationUpdateViewModel model, string accessToken)
        {
            try
            {
                // Get list noti
                var listNotiIds = model.Ids;
                var listNoti = Get(notification => listNotiIds.Contains(notification.Id)).ToList();
                if (listNoti.Count < listNotiIds.Count) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Notification not found");

                var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
                var userId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);

                foreach (var noti in listNoti)
                {
                    if (noti.ReceiverId == userId)
                    {
                        noti.IsRead = true;
                        await UpdateAsync(noti);
                    }
                }

                return model;
            }
            catch (ErrorResponse e)
            {
                throw new ErrorResponse((int)e.Error.Code, e.Error.Message);
            }
            catch (Exception ex)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, "" + ex.Message);
            }
            
        }
    }
}
