using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Models;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.UnitOfWorks;
using System;
using System.Threading.Tasks;

namespace RSSMS.DataService.Services
{
    public interface IBoxService : IBaseService<Box>
    {
        Task CreateNumberOfBoxes(Guid shelfId, int num, Guid serviceId, Guid staffId);
        Task Delete(Guid shelfId, Guid staffId);
        Task UpdateBoxSize(Guid serviceId, Guid shelfId);
    }
    public class BoxService : BaseService<Box>, IBoxService
    {
        public BoxService(IUnitOfWork unitOfWork, IBoxRepository repository) : base(unitOfWork, repository)
        {
        }

        public async Task CreateNumberOfBoxes(Guid shelfId, int num, Guid serviceId, Guid staffId)
        {
            for (int i = 0; i < num; i++)
            {
                Box box = new Box();
                box.IsActive = true;
                box.ServiceId = serviceId;
                box.ShelfId = shelfId;
                box.Status = 0;
                box.ModifiedBy = staffId;
                await CreateAsync(box);
            }
        }

        public async Task Delete(Guid shelfId, Guid staffId)
        {
            var listBoxes = await Get(x => x.ShelfId == shelfId && x.IsActive == true).ToListAsync();
            if (listBoxes == null) return;
            foreach (var box in listBoxes)
            {
                box.IsActive = false;
                box.ModifiedBy = staffId;
                await UpdateAsync(box);
            }
        }

        public async Task UpdateBoxSize(Guid serviceId, Guid shelfId)
        {
            var listBoxes = await Get(x => x.ShelfId == shelfId && x.IsActive == true && x.ServiceId != serviceId).ToListAsync();
            if (listBoxes == null) return;
            foreach (var box in listBoxes)
            {
                box.ServiceId = serviceId;
                await UpdateAsync(box);
            }
        }
    }
}
