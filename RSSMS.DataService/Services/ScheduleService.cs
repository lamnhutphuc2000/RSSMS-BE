using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Constants;
using RSSMS.DataService.Models;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.Responses;
using RSSMS.DataService.UnitOfWorks;
using RSSMS.DataService.Utilities;
using RSSMS.DataService.ViewModels.Schedules;
using RSSMS.DataService.ViewModels.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RSSMS.DataService.Services
{
    public interface IScheduleService : IBaseService<Schedule>
    {
        Task<DynamicModelResponse<ScheduleViewModel>> Get(ScheduleSearchViewModel model, string[] fields, int page, int size);
        Task<List<ScheduleOrderViewModel>> Create(ScheduleCreateViewModel model);
    }
    public class ScheduleService : BaseService<Schedule>, IScheduleService

    {
        private readonly IMapper _mapper;
        public ScheduleService(IUnitOfWork unitOfWork, IScheduleRepository repository, IMapper mapper) : base(unitOfWork, repository)
        {
            _mapper = mapper;
        }

        public async Task<List<ScheduleOrderViewModel>> Create(ScheduleCreateViewModel model)
        {
            var userIds = model.UserIds;
            if (userIds.Count <= 0) throw new ErrorResponse((int)HttpStatusCode.NotFound, "User id null");
            var orderIds = model.OrderIds;
            if (orderIds.Count <= 0) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Order id null");
            var schedules = Get(x => orderIds.Contains(x.Id) && x.IsActive == true).ToList();
            foreach (var schedule in schedules)
            {
                schedule.IsActive = false;
                await UpdateAsync(schedule);
            }
            for(int i = 0; i < userIds.Count; i++)
            {
                for(int j = 0; j < orderIds.Count; j ++)
                {
                    var scheduleToCreate = _mapper.Map<Schedule>(model);
                    scheduleToCreate.UserId = userIds[i];
                    scheduleToCreate.OrderId = orderIds[j];
                    await CreateAsync(scheduleToCreate);
                }
            }
            //var result = Get(x => x.OrderId == model.OrderId && x.IsActive == true).Include(x => x.User).ThenInclude(x => x.Images)
            //            .AsEnumerable().GroupBy(p => (int)p.OrderId)
            //                        .Select(g => new ScheduleOrderViewModel
            //                        {
            //                            OrderId = g.Key,
            //                            DeliveryTime = model.DeliveryTime,
            //                            SheduleDay = model.SheduleDay,
            //                            Users = Get(x => x.OrderId == g.Key && x.IsActive == true).Select(x => x.User).ProjectTo<UserViewModel>(_mapper.ConfigurationProvider).ToList()
            //                        }).ToList();
            return null;
        }

        public async Task<DynamicModelResponse<ScheduleViewModel>> Get(ScheduleSearchViewModel model, string[] fields, int page, int size)
        {
            var schedules = Get(x => x.IsActive == true)
                        .Where(x => x.SheduleDay >= model.DateFrom && x.SheduleDay <= model.DateTo).Include(x => x.User).ThenInclude(x => x.Images)
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
