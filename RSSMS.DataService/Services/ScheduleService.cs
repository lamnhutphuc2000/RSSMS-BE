using AutoMapper;
using RSSMS.DataService.Constants;
using RSSMS.DataService.Models;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.Responses;
using RSSMS.DataService.UnitOfWorks;
using RSSMS.DataService.Utilities;
using RSSMS.DataService.ViewModels.Schedules;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RSSMS.DataService.Services
{
    public interface IScheduleService : IBaseService<Schedule>
    {
        Task<DynamicModelResponse<Schedule>> Get(ScheduleViewModel model, string[] fields, int page, int size);
        Task<ScheduleOrderViewModel> Create(ScheduleOrderViewModel model);
    }
    public class ScheduleService : BaseService<Schedule>, IScheduleService

    {
        private readonly IMapper _mapper;
        public ScheduleService(IUnitOfWork unitOfWork, IShelfService shelfService, IScheduleRepository repository, IMapper mapper) : base(unitOfWork, repository)
        {
            _mapper = mapper;
        }

        public async Task<ScheduleOrderViewModel> Create(ScheduleOrderViewModel model)
        {
            var userIds = model.UserIds;
            if (userIds.Count <= 0) throw new ErrorResponse((int)HttpStatusCode.NotFound, "User id null");
            foreach (var userId in userIds)
            {
                var scheduleToCreate = _mapper.Map<Schedule>(model);
                scheduleToCreate.UserId = userId;
                await CreateAsync(scheduleToCreate);
            }
            return model;
        }

        public async Task<DynamicModelResponse<Schedule>> Get(ScheduleViewModel model, string[] fields, int page, int size)
        {
            var result = Get(x => x.IsActive == true)
                    .Where(x => x.SheduleDay >= model.DateFrom && x.SheduleDay <= model.DateTo)
                .PagingIQueryable(page, size, CommonConstant.LimitPaging, CommonConstant.DefaultPaging);
            if (result.Item2.ToList().Count < 1) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Schedul not found");

            var rs = new DynamicModelResponse<Schedule>
            {

                Metadata = new PagingMetaData
                {
                    Page = page,
                    Size = size,
                    Total = result.Item1,
                    TotalPage = (int)Math.Ceiling((double)result.Item1 / size)
                },
                Data = result.Item2.ToList()
            };

            return rs;
        }
    }
}
