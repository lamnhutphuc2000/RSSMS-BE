using AutoMapper;
using RSSMS.DataService.Models;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.UnitOfWorks;
using System.Threading.Tasks;

namespace RSSMS.DataService.Services
{
    public interface IBoxService : IBaseService<Box>
    {
        Task CreateNumberOfBoxes(int shelfId, int num, int size);
    }
    public class BoxService : BaseService<Box>, IBoxService
    {
        private readonly IMapper _mapper;
        public BoxService(IUnitOfWork unitOfWork, IBoxRepository repository, IMapper mapper) : base(unitOfWork, repository)
        {
            _mapper = mapper;
        }

        public async Task CreateNumberOfBoxes(int shelfId, int num, int size)
        {
            for (int i = 0; i < num; i++)
            {
                Box box = new Box();
                box.IsActive = true;
                box.SizeType = size;
                box.ShelfId = shelfId;
                box.Status = 0;
                await CreateAsync(box);
            }
        }
    }
}
