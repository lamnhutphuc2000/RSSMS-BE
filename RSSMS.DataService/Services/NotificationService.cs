using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Constants;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.Responses;
using RSSMS.DataService.UnitOfWorks;
using RSSMS.DataService.Utilities;
using RSSMS.DataService.ViewModels.Notifications;
using RSSMS.DataService.ViewModels.Orders;
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
    }
}
