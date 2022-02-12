using AutoMapper;
using FCM.Net;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RSSMS.DataService.Models;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.UnitOfWorks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RSSMS.DataService.Services
{
    public interface INotificationDetailService : IBaseService<NotificationDetail>
    {
        Task<ResponseContent> PushOrderNoti(string description, int SenderId, int NotificationId, int orderId, int? requestId, int notiId); // noti to manager when receive another order
        Task<ResponseContent> PushCancelRequestNoti(string description, int SenderId, int NotificationId);
        Task<ResponseContent> SendNoti(string description, int SenderId, int receiverId, string registrationId, int NotificationId, int orderId, int? requestId, object data);
    }
    public class NotificationDetailService : BaseService<NotificationDetail>, INotificationDetailService
    {
        private readonly IStaffManageStorageService _staffManageStorageService;
        public NotificationDetailService(IUnitOfWork unitOfWork, IStaffManageStorageService staffManageStorageService, INotificationDetailRepository repository, IMapper mapper) : base(unitOfWork, repository)
        {
            _staffManageStorageService = staffManageStorageService;
        }
        public async Task<ResponseContent> PushOrderNoti(string description, int senderId, int notificationId, int orderId, int? requestId, int notiId)
        {
            var managers = _staffManageStorageService.Get(x => x.RoleName == "Manager").Include(x => x.User).Select(x => x.User).ToList();
            if (managers.Count == 0) return null;

            List<string> registrationIds = managers.Where(x => x.DeviceTokenId != null && x.IsActive == true).Select(a => a.DeviceTokenId).ToList();
            if (registrationIds.Count == 0) return null;
            registrationIds = registrationIds.Distinct().ToList();
            var managerIds = managers.Where(x => x.DeviceTokenId != null && x.IsActive == true).Select(a => a.Id).ToList();
            DateTime now = DateTime.Now;

            NotificationDetail notiSenderDetail = new NotificationDetail
            {
                UserId = senderId,
                NotificationId = notificationId,
                IsOwn = true,
                IsRead = false,
                IsActive = true,
                CreateDate = now
            };

            await CreateAsync(notiSenderDetail);
            foreach (var managerId in managerIds)
            {
                NotificationDetail notiDetail = new NotificationDetail
                {
                    UserId = managerId,
                    NotificationId = notificationId,
                    IsOwn = false,
                    IsRead = false,
                    IsActive = true,
                    CreateDate = now
                };
                await CreateAsync(notiDetail);
            }


            using (var sender = new Sender("AAAAry7VzWE:APA91bEFLYrdoliXt0cRdQtnnRNOdxhYXP0mMTOSrgOvcqhULEGKWwUJQIP7phbTXq54zGYD0pzRpDNXfkaSDwd36q088cKkT-CiQz-IBIdLC2ki9zuyK865yiHMG1G6q403iW9fsaKR"))
            {
                var message = new Message
                {
                    RegistrationIds = registrationIds,
                    Notification = new FCM.Net.Notification
                    {
                        Title = "From RSSMS",
                        Body = description
                    },
                    Data = new
                    {
                        Content = description,
                        OrderId = orderId,
                        RequestId = requestId,
                        NotiId = notiId
                    }
                };
                var result = await sender.SendAsync(message);
                return result;
            }
        }
        public async Task<ResponseContent> PushCancelRequestNoti(string description, int senderId, int notificationId)
        {
            var storageSenderIn = _staffManageStorageService.Get(x => x.UserId == senderId).Select(a => a.StorageId).FirstOrDefault();
            if (storageSenderIn == 0) return null;
            var manager = _staffManageStorageService.Get(x => x.RoleName == "Manager" && x.StorageId == storageSenderIn).Include(x => x.User).Select(x => x.User).FirstOrDefault();
            if (manager == null) return null;

            if (manager.DeviceTokenId == null) return null;
            string registrationId = manager.DeviceTokenId;
            var managerId = manager.Id;
            DateTime now = DateTime.Now;

            NotificationDetail notiSenderDetail = new NotificationDetail
            {
                UserId = senderId,
                NotificationId = notificationId,
                IsOwn = true,
                IsRead = false,
                IsActive = true,
                CreateDate = now
            };

            await CreateAsync(notiSenderDetail);
            NotificationDetail notiDetail = new NotificationDetail
            {
                UserId = managerId,
                NotificationId = notificationId,
                IsOwn = false,
                IsRead = false,
                IsActive = true,
                CreateDate = now
            };
            await CreateAsync(notiDetail);


            using (var sender = new Sender("AAAAry7VzWE:APA91bEFLYrdoliXt0cRdQtnnRNOdxhYXP0mMTOSrgOvcqhULEGKWwUJQIP7phbTXq54zGYD0pzRpDNXfkaSDwd36q088cKkT-CiQz-IBIdLC2ki9zuyK865yiHMG1G6q403iW9fsaKR"))
            {
                var message = new Message
                {
                    To = registrationId,
                    Notification = new FCM.Net.Notification
                    {
                        Title = "From RSSMS",
                        Body = description
                    },
                    Data = new
                    {
                        Content = description,
                        NotiId = notificationId
                    }
                };
                var result = await sender.SendAsync(message);
                return result;
            }
        }

        public async Task<ResponseContent> SendNoti(string description, int SenderId, int receiverId, string registrationId, int notificationId, int orderId, int? requestId, object data)
        {
            string jsonConvert = JsonConvert.SerializeObject(data);
            using (var sender = new Sender("AAAAry7VzWE:APA91bEFLYrdoliXt0cRdQtnnRNOdxhYXP0mMTOSrgOvcqhULEGKWwUJQIP7phbTXq54zGYD0pzRpDNXfkaSDwd36q088cKkT-CiQz-IBIdLC2ki9zuyK865yiHMG1G6q403iW9fsaKR"))
            {
                var message = new Message
                {
                    To = registrationId,
                    Notification = new FCM.Net.Notification
                    {
                        Title = "From RSSMS",
                        Body = description
                    },
                    Data = new Dictionary<string, string>
                    {
                        {"data",jsonConvert },
                        {"NotiId",notificationId.ToString() }
                    }
                };
                var result = await sender.SendAsync(message);
                return result;
            }
        }
    }
}
