using FCM.Net;
using Firebase.Auth;
using Firebase.Storage;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RSSMS.DataService.Services
{
    public interface IFirebaseService
    {
        Task<string> UploadImageToFirebase(string image, string type, Guid id, string name);
        Task<ResponseContent> SendNoti(string description, Guid receiverId, string registrationId, Guid? requestId, object data);
        Task<ResponseContent> PushOrderNoti(string description, Guid? orderId, Guid? requestId); // noti to manager when receive another order

        Task<ResponseContent> PushCancelRequestNoti(string description, Guid senderId);
    }
    public class FirebaseService : IFirebaseService
    {
        private static string apiKEY = "AIzaSyCbxMnxwCfJgCJtvaBeRdvvZ3y1Ucuyv2s";
        private static string Bucket = "rssms-5fcc8.appspot.com";
        private readonly IStaffAssignStorageService _staffAssignStoragesService;
        private readonly INotificationService _notificationService;
        public FirebaseService(IStaffAssignStorageService staffAssignStoragesService, INotificationService notificationService)
        {
            _staffAssignStoragesService = staffAssignStoragesService;
            _notificationService = notificationService;
        }

        public async Task<string> UploadImageToFirebase(string image, string type, Guid id, string name)
        {
            if (image == null) return null;
            if (image.Length <= 0) return null;

            byte[] data = System.Convert.FromBase64String(image);
            MemoryStream ms = new MemoryStream(data);

            var auth = new FirebaseAuthProvider(new FirebaseConfig(apiKEY));
            var a = await auth.SignInWithEmailAndPasswordAsync("toadmin@gmail.com", "123456");

            var cancellation = new CancellationTokenSource();

            var upload = new FirebaseStorage(
                        Bucket,
                        new FirebaseStorageOptions
                        {
                            AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),
                            ThrowOnCancel = true
                        }).Child("assets")
                        .Child($"{type}")
                        .Child($"{id}")
                        .Child($"{name}.jpg")
                        .PutAsync(ms, cancellation.Token);
            string url = await upload;

            return url;
        }

        public async Task<ResponseContent> PushOrderNoti(string description, Guid? orderId, Guid? requestId)
        {
            var managers = _staffAssignStoragesService.Get(x => x.RoleName == "Manager").Include(x => x.Staff).Select(x => x.Staff).ToList();
            if (managers.Count == 0) return null;

            List<string> registrationIds = managers.Where(x => x.DeviceTokenId != null && x.IsActive == true).Select(a => a.DeviceTokenId).ToList();
            if (registrationIds.Count == 0) return null;
            registrationIds = registrationIds.Distinct().ToList();
            var managerIds = managers.Where(x => x.DeviceTokenId != null && x.IsActive == true).Select(a => a.Id).ToList();
            DateTime now = DateTime.Now;
            foreach (var managerId in managerIds)
            {
                Models.Notification noti = new Models.Notification
                {
                    ReceiverId = managerId,
                    Description = description,
                    CreatedDate = DateTime.Now,
                    IsRead = false
                };
                await _notificationService.CreateAsync(noti);
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
                        RequestId = requestId
                    }
                };
                var result = await sender.SendAsync(message);
                return result;
            }
        }

        //public static void CopyTo(Stream src, Stream dest)
        //{
        //    byte[] bytes = new byte[4096];

        //    int cnt;

        //    while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
        //    {
        //        dest.Write(bytes, 0, cnt);
        //    }
        //}

        public static byte[] Compress(byte[] input)
        {
            using (var result = new MemoryStream())
            {
                var lengthBytes = BitConverter.GetBytes(input.Length);
                result.Write(lengthBytes, 0, 4);

                using (var compressionStream = new GZipStream(result,
                    CompressionMode.Compress))
                {
                    compressionStream.Write(input, 0, input.Length);
                    compressionStream.Flush();

                }
                return result.ToArray();
            }
        }

        public async Task<ResponseContent> SendNoti(string description, Guid receiverId, string registrationId, Guid? requestId, object data)
        {
            var now = DateTime.Now;
            Models.Notification noti = new Models.Notification
            {
                ReceiverId = receiverId,
                Description = description,
                CreatedDate = DateTime.Now,
                IsRead = false
            };
            await _notificationService.CreateAsync(noti);

            string jsonConvert = JsonConvert.SerializeObject(data);

            byte[] encoded = Encoding.UTF8.GetBytes(jsonConvert);
            byte[] compressed = Compress(encoded);
            string compressString = Convert.ToBase64String(compressed);
            //string compressString;
            //var bytes = Encoding.Unicode.GetBytes(jsonConvert);
            //using (var msi = new MemoryStream(bytes))
            //using (var mso = new MemoryStream())
            //{
            //    using (var gs = new GZipStream(mso, CompressionMode.Compress))
            //    {
            //        msi.CopyTo(gs);
            //    }
            //    compressString = Convert.ToBase64String(mso.ToArray());
            //}

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
                    Data = new Dictionary<string, object>
                    {
                        {"data",compressString },
                        {"NotiId",noti.Id.ToString() }
                    }
                };
                var result = await sender.SendAsync(message);
                return result;
            }
        }

        public async Task<ResponseContent> PushCancelRequestNoti(string description, Guid senderId)
        {
            var storageSenderIn = _staffAssignStoragesService.Get(x => x.StaffId == senderId).Select(a => a.StorageId).First();
            var manager = _staffAssignStoragesService.Get(x => x.RoleName == "Manager" && x.StorageId == storageSenderIn).Include(x => x.Staff).Select(x => x.Staff).FirstOrDefault();
            if (manager == null) return null;

            if (manager.DeviceTokenId == null) return null;
            string registrationId = manager.DeviceTokenId;
            var managerId = manager.Id;
            DateTime now = DateTime.Now;

            Models.Notification noti = new Models.Notification
            {
                ReceiverId = managerId,
                Description = description,
                CreatedDate = DateTime.Now,
                IsRead = false
            };
            await _notificationService.CreateAsync(noti);


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
                        NotiId = noti.Id
                    }
                };
                var result = await sender.SendAsync(message);
                return result;
            }
        }
    }
}
