//using Microsoft.EntityFrameworkCore;
//using RSSMS.DataService.Models;
//using RSSMS.DataService.Repositories;
//using RSSMS.DataService.UnitOfWorks;
//using RSSMS.DataService.ViewModels.OrderBoxes;
//using System;
//using System.Threading.Tasks;

//namespace RSSMS.DataService.Services
//{
//    public interface IBoxService : IBaseService<Box>
//    {
//        Task CreateNumberOfBoxes(Guid shelfId, int num, Guid serviceId, string serviceName, Guid staffId);
//        Task Delete(Guid shelfId, Guid staffId);
//        Task UpdateBoxSize(Guid serviceId, string serviceName, Guid shelfId);
//    }
//    public class BoxService : BaseService<Box>, IBoxService
//    {
//        public BoxService(IUnitOfWork unitOfWork, IBoxRepository repository) : base(unitOfWork, repository)
//        {
//        }

//        public async Task CreateNumberOfBoxes(Guid shelfId, int num, Guid serviceId, string serviceName, Guid staffId)
//        {
//            for (int i = 0; i < num; i++)
//            {
//                int boxName = i + 1;
//                Box box = new Box()
//                {
//                    IsActive = true,
//                    ServiceId = serviceId,
//                    ShelfId = shelfId,
//                    Name = serviceName + " - " + boxName,
//                    Status = 0,
//                    ModifiedBy = staffId,
//                    CreatedDate = DateTime.Now
//                };
//                await CreateAsync(box);
//            }
//        }

//        public async Task Delete(Guid shelfId, Guid staffId)
//        {
//            var listBoxes = await Get(x => x.ShelfId == shelfId && x.IsActive == true).ToListAsync();
//            if (listBoxes == null) return;
//            foreach (var box in listBoxes)
//            {
//                box.IsActive = false;
//                box.ModifiedBy = staffId;
//                await UpdateAsync(box);
//            }
//        }

//        public async Task UpdateBoxSize(Guid serviceId, string serviceName, Guid shelfId)
//        {
//            var listBoxes = await Get(x => x.ShelfId == shelfId && x.IsActive == true && x.ServiceId != serviceId).ToListAsync();
//            if (listBoxes == null) return;
//            for (int i = 0; i < listBoxes.Count; i++)
//            {
//                int boxName = i + 1;
//                listBoxes[i].Name = serviceName + " - " + boxName;
//                listBoxes[i].ServiceId = serviceId;
//                await UpdateAsync(listBoxes[i]);
//            }
//        }



//    }
//}
