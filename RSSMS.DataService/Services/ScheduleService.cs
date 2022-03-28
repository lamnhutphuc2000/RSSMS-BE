using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Constants;
using RSSMS.DataService.Models;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.Responses;
using RSSMS.DataService.UnitOfWorks;
using RSSMS.DataService.Utilities;
using RSSMS.DataService.ViewModels.Accounts;
using RSSMS.DataService.ViewModels.Orders;
using RSSMS.DataService.ViewModels.Requests;
using RSSMS.DataService.ViewModels.Schedules;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RSSMS.DataService.Services
{
    public interface IScheduleService : IBaseService<Schedule>
    {
        Task<DynamicModelResponse<ScheduleViewModel>> Get(ScheduleSearchViewModel model, string[] fields, int page, int size, string accessToken);
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
            var schedules = model.Schedules;
            if (schedules.Count <= 0) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Schedules null");
            var listSchedules = Get().Include(x => x.Request).ThenInclude(request => request.Order).ToList();

            var schedulesAssigned = listSchedules.Where(x => schedules.Any(a => a.RequestId == x.RequestId) &&  x.ScheduleDay.Date == model.ScheduleDay.Value.Date);



            foreach (var scheduleAssigned in schedulesAssigned)
            {
                var request = scheduleAssigned.Request;
                if (request != null)
                {
                    if (request.Type == 0) request.Status = 1;
                    scheduleAssigned.Request.Status = 1;
                }
                scheduleAssigned.IsActive = false;
                scheduleAssigned.Status = null;
                await UpdateAsync(scheduleAssigned);
            }
            for (int i = 0; i < userIds.Count; i++)
            {
                for (int j = 0; j < schedules.Count; j++)
                {
                    var scheduleToCreate = _mapper.Map<Schedule>(model);
                    scheduleToCreate.Address = schedules[j].DeliveryAddress;
                    scheduleToCreate.ScheduleDay = (DateTime)model.ScheduleDay;
                    scheduleToCreate.UserId = userIds[i];
                    scheduleToCreate.RequestId = schedules[j].RequestId;
                    scheduleToCreate.ScheduleTime = schedules[j].ScheduleTime;
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

        public async Task<DynamicModelResponse<ScheduleViewModel>> Get(ScheduleSearchViewModel model, string[] fields, int page, int size, string accessToken)
         {
            var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
            var userId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);
            var role = secureToken.Claims.First(claim => claim.Type.Contains("role")).Value;
            (int, IQueryable<ScheduleViewModel>) result = (0, null);

            if (role == "Delivery Staff")
            {
                var schedules = Get(x => x.IsActive == true && x.UserId == userId)
                        .Where(x => x.ScheduleDay.Date >= model.DateFrom.Value.Date && x.ScheduleDay.Date <= model.DateTo.Value.Date).Include(x => x.Request)
                        .ThenInclude(request => request.Order)
                        .ThenInclude(order => order.Customer)
                        .Include(x => x.Request)
                        .ThenInclude(request => request.Order)
                        .ThenInclude(order => order.OrderDetails)
                        .ThenInclude(orderDetail => orderDetail.Images);
                var tmp = schedules.AsEnumerable().GroupBy(p => (Guid)p.RequestId)
                                    .Select(g => new ScheduleViewModel
                                    {
                                        Id = g.First().Id,
                                        OrderId = g.First().Request.OrderId,
                                        RequestId = g.Key,
                                        RequestType = g.First().Request.Type,
                                        Request = _mapper.Map<RequestScheduleViewModel>(g.First().Request),
                                        Address = g.First().Address,
                                        Note = g.First().Note,
                                        Status = g.First().Request.Order != null ? g.First().Request.Order.Status : null,
                                        IsActive = g.First().IsActive,
                                        ScheduleDay = (DateTime)g.First().ScheduleDay,
                                        ScheduleTime = g.First().ScheduleTime,
                                        Accounts = Get(x => x.RequestId == g.Key && x.IsActive == true).Select(x => x.User).ProjectTo<AccountsViewModel>(_mapper.ConfigurationProvider).ToList()
                                    });
                result = tmp.AsEnumerable().GroupBy(p => (DateTime)p.ScheduleDay)
                    .Select(g => new ScheduleViewModel
                    {
                        ScheduleDay = g.Key,
                        Requests = g.Where(x => x.ScheduleDay == g.Key).Select(x => x.Request).ToList()
                    }).AsQueryable().PagingIQueryable(page, size, CommonConstant.LimitPaging, CommonConstant.DefaultPaging);
            }
            else
            {
                var schedules = Get(x => x.IsActive == true)
                        .Where(x => x.ScheduleDay.Date >= model.DateFrom.Value.Date && x.ScheduleDay.Date <= model.DateTo.Value.Date).Include(x => x.Request)
                        .ThenInclude(request => request.Order)
                        .ThenInclude(order => order.Customer)
                        .Include(x => x.Request)
                        .ThenInclude(request => request.Order)
                        .ThenInclude(order => order.OrderDetails)
                        .ThenInclude(orderDetail => orderDetail.Images);
                result = schedules.AsEnumerable().GroupBy(p => (Guid)p.RequestId)
                                    .Select(g => new ScheduleViewModel
                                    {
                                        Id = g.First().Id,
                                        OrderId = g.First().Request.OrderId,
                                        RequestId = g.Key,
                                        Address = g.First().Address,
                                        Note = g.First().Note,
                                        Request = _mapper.Map<RequestScheduleViewModel>(g.First().Request),
                                        Status = g.First().Request.Order != null ? g.First().Request.Order.Status : null,
                                        IsActive = g.First().IsActive,
                                        ScheduleDay = (DateTime)g.First().ScheduleDay,
                                        ScheduleTime = g.First().ScheduleTime,
                                        Accounts = Get(x => x.RequestId == g.Key && x.IsActive == true).Select(x => x.User).ProjectTo<AccountsViewModel>(_mapper.ConfigurationProvider).ToList()
                                    }).AsQueryable().PagingIQueryable(page, size, CommonConstant.LimitPaging, CommonConstant.DefaultPaging);
            }
            if (result.Item2 == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Schedule not found");
            if (result.Item2.ToList().Count < 1) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Schedule not found");
            var meo = result.Item2.ToList();
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
