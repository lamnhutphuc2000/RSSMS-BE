using AutoMapper;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.UnitOfWorks;

namespace RSSMS.DataService.Services
{
    public interface INotificationService : IBaseService<Models.Notification>
    {

    }
    public class NotificationService : BaseService<Models.Notification>, INotificationService
    {
        public NotificationService(IUnitOfWork unitOfWork, INotificationRepository repository, IMapper mapper) : base(unitOfWork, repository)
        {
        }



    }
}
