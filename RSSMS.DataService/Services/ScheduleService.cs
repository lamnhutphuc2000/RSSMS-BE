using AutoMapper;
using AutoMapper.QueryableExtensions;
using RSSMS.DataService.Constants;
using RSSMS.DataService.Models;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.Responses;
using RSSMS.DataService.UnitOfWorks;
using RSSMS.DataService.Utilities;
using RSSMS.DataService.ViewModels.Schedules;
using RSSMS.DataService.ViewModels.Users;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RSSMS.DataService.Services
{
    public interface IScheduleService : IBaseService<Schedule>
    {
        Task<DynamicModelResponse<ScheduleViewModel>> Get(ScheduleSearchViewModel model, string[] fields, int page, int size);
        Task<ScheduleOrderViewModel> Create(ScheduleOrderViewModel model);
    }
    public class ScheduleService : BaseService<Schedule>, IScheduleService

    {
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        public ScheduleService(IUnitOfWork unitOfWork, IUserService userService, IScheduleRepository repository, IMapper mapper) : base(unitOfWork, repository)
        {
            _mapper = mapper;
            _userService = userService;
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

        public async Task<DynamicModelResponse<ScheduleViewModel>> Get(ScheduleSearchViewModel model, string[] fields, int page, int size)
        {
            var schedules = Get(x => x.IsActive == true)
                        .Where(x => x.SheduleDay >= model.DateFrom && x.SheduleDay <= model.DateTo)
                        .AsEnumerable().GroupBy(p => (int)p.OrderId)
                                    .Select(g => new ScheduleViewModel
                                    {
                                        OrderId = g.Key,
                                        Address = g.First().Address,
                                        Note = g.First().Note,
                                        Status = g.First().Status,
                                        IsActive = g.First().IsActive,
                                        Users = Get(x => x.OrderId == g.Key && x.IsActive == true).Select(x => x.User).ProjectTo<UserViewModel>(_mapper.ConfigurationProvider).ToList()
                                    }).AsQueryable();

            var result = schedules.PagingIQueryable(page, size, CommonConstant.LimitPaging, CommonConstant.DefaultPaging);
            if (result.Item2.ToList().Count < 1) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Schedul not found");

            var rs = new DynamicModelResponse<ScheduleViewModel>
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
