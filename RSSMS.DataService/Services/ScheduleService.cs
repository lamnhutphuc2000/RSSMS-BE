using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Constants;
using RSSMS.DataService.Models;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.Responses;
using RSSMS.DataService.UnitOfWorks;
using RSSMS.DataService.Utilities;
using RSSMS.DataService.ViewModels.Orders;
using RSSMS.DataService.ViewModels.Schedules;
using RSSMS.DataService.ViewModels.Users;
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
            if (schedules.Count <= 0) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Order id null");
            var listSchedules = Get(x => x.IsActive == true).ToList();

            var schedulesAssigned = listSchedules.Where(x => schedules.Any(a => a.OrderIds == x.OrderId));

            foreach (var scheduleAssigned in schedulesAssigned)
            {
                scheduleAssigned.IsActive = false;
                await UpdateAsync(scheduleAssigned);
            }
            for (int i = 0; i < userIds.Count; i++)
            {
                for (int j = 0; j < schedules.Count; j++)
                {
                    var scheduleToCreate = _mapper.Map<Schedule>(model);
                    scheduleToCreate.UserId = userIds[i];
                    scheduleToCreate.OrderId = schedules[j].OrderIds;
                    scheduleToCreate.DeliveryTime = schedules[j].DeliveryTime;
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
            var userId = Int32.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);
            var role = secureToken.Claims.First(claim => claim.Type.Contains("role")).Value;
            (int, IQueryable<ScheduleViewModel>) result = (0,null);

            if (role == "Delivery Staff")
            {
                var schedules = Get(x => x.IsActive == true && x.UserId == userId)
                        .Where(x => x.SheduleDay >= model.DateFrom && x.SheduleDay <= model.DateTo).Include(x => x.Order)
                        .ThenInclude(order => order.OrderStorageDetails)
                        .Include(x => x.Order)
                        .ThenInclude(order => order.Customer)
                        .Include(x => x.Order)
                        .ThenInclude(order => order.OrderDetails)
                        .ThenInclude(orderDetail => orderDetail.Product)
                        .ThenInclude(product => product.Images);
                result = schedules.AsEnumerable().GroupBy(p => (int)p.OrderId)
                                    .Select(g => new ScheduleViewModel
                                    {
                                        OrderId = g.Key,
                                        Order = _mapper.Map<OrderViewModel>(g.First().Order),
                                        Address = g.First().Order.AddressReturn,
                                        Note = g.First().Note,
                                        Status = g.First().Order.Status,
                                        IsActive = g.First().IsActive,
                                        Users = Get(x => x.OrderId == g.Key && x.IsActive == true).Select(x => x.User).ProjectTo<UserViewModel>(_mapper.ConfigurationProvider).ToList()
                                    }).AsQueryable().PagingIQueryable(page, size, CommonConstant.LimitPaging, CommonConstant.DefaultPaging);
            }
            else
            {
                var schedules = Get(x => x.IsActive == true)
                        .Where(x => x.SheduleDay >= model.DateFrom && x.SheduleDay <= model.DateTo).Include(x => x.Order).Include(x => x.User).ThenInclude(x => x.Images);
                result = schedules.AsEnumerable().GroupBy(p => (int)p.OrderId)
                                    .Select(g => new ScheduleViewModel
                                    {
                                        OrderId = g.Key,
                                        Address = g.First().Order.AddressReturn,
                                        Note = g.First().Note,
                                        Status = g.First().Order.Status,
                                        IsActive = g.First().IsActive,
                                        Users = Get(x => x.OrderId == g.Key && x.IsActive == true).Select(x => x.User).ProjectTo<UserViewModel>(_mapper.ConfigurationProvider).ToList()
                                    }).AsQueryable().PagingIQueryable(page, size, CommonConstant.LimitPaging, CommonConstant.DefaultPaging);
            }
            if(result.Item2 == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Schedul not found");
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
