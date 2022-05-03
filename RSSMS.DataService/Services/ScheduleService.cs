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
        Task<List<ScheduleOrderViewModel>> Create(ScheduleCreateViewModel model, string accessToken);
    }
    public class ScheduleService : BaseService<Schedule>, IScheduleService

    {
        private readonly IMapper _mapper;
        private readonly IUtilService _utilService;
        private readonly IAccountService _accountService;
        public ScheduleService(IUnitOfWork unitOfWork, IScheduleRepository repository, IUtilService utilService,
            IAccountService accountService,
            IMapper mapper) : base(unitOfWork, repository)
        {
            _mapper = mapper;
            _utilService = utilService;
            _accountService = accountService;
        }

        public async Task<List<ScheduleOrderViewModel>> Create(ScheduleCreateViewModel model, string accessToken)
        {
            try
            {
                var userIds = model.UserIds;
                if (userIds.Count <= 0) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Không tìm thấy nhân viên");
                var schedules = model.Schedules;
                if (schedules.Count <= 0) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Không tìm thấy giờ lịch");
                var listSchedules = Get(schedule => schedule.IsActive && schedule.ScheduleDay.Date == model.ScheduleDay.Value.Date).Include(x => x.Request).ThenInclude(request => request.Order).ToList();

                var schedulesAssigned = listSchedules.Where(x => schedules.Any(a => a.RequestId == x.RequestId));


                if (userIds.Count() > 0)
                {
                    HashSet<Guid> hashset = new HashSet<Guid>();
                    IEnumerable<Guid> duplicates = userIds.Where(e => !hashset.Add(e));
                    if (duplicates.Count() > 0)
                        throw new ErrorResponse((int)HttpStatusCode.NotFound, "Trùng nhân viên");
                }
                var storageId = _accountService.Get(account => account.Id == userIds.FirstOrDefault()).Select(account => account.StaffAssignStorages.Where(staffAssign => staffAssign.IsActive).FirstOrDefault().StorageId).FirstOrDefault();
                foreach (var schedule in schedules)
                    if (model.AvailableStaffs < schedule.RequestRemain) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Số nhân viên còn lại không đủ để đi thực hiện yêu cầu");


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
                        scheduleToCreate.StaffId = userIds[i];
                        scheduleToCreate.RequestId = schedules[j].RequestId;
                        scheduleToCreate.ScheduleTime = _utilService.StringToTime(schedules[j].ScheduleTime);
                        scheduleToCreate.Status = 1;
                        await CreateAsync(scheduleToCreate);
                        var schedule = Get(x => x.Id == scheduleToCreate.Id).Include(x => x.Request).FirstOrDefault();
                        schedule.Request.Status = 2;
                        await UpdateAsync(schedule);
                    }
                }
                return null;
            }
            catch (ErrorResponse e)
            {
                throw new ErrorResponse((int)e.Error.Code, e.Error.Message);
            }
            catch (Exception e)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, e.Message);
            }


        }

        public async Task<DynamicModelResponse<ScheduleViewModel>> Get(ScheduleSearchViewModel model, string[] fields, int page, int size, string accessToken)
        {
            try
            {
                var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
                var userId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);
                var role = secureToken.Claims.First(claim => claim.Type.Contains("role")).Value;
                (int, IQueryable<ScheduleViewModel>) result = (0, null);
                var account = _accountService.Get(account => account.Id == userId && account.IsActive).Include(account => account.StaffAssignStorages).Include(account => account.Role).FirstOrDefault();
                if (account == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Không tìm thấy tài khoản");
                if (model.DateFrom == null || model.DateTo == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Ngày bắt đầu và ngày kết thúc không thể để trống");

                if (account.Role.Name == "Delivery Staff")
                {
                    var schedules = await Get(x => x.IsActive == true && x.StaffId == userId)
                            .Where(x => x.ScheduleDay.Date >= model.DateFrom.Value.Date && x.ScheduleDay.Date <= model.DateTo.Value.Date).Include(x => x.Request)
                            .ThenInclude(request => request.Order)
                            .ThenInclude(order => order.Customer)
                            .Include(x => x.Request).ThenInclude(request => request.CreatedByNavigation).ThenInclude(createdBy => createdBy.Role)
                            .Include(x => x.Request)
                            .ThenInclude(request => request.Order)
                            .ThenInclude(order => order.OrderDetails)
                            .ThenInclude(orderDetail => orderDetail.Images).ToListAsync();
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
                                            ScheduleTime = _utilService.TimeToString(g.First().ScheduleTime),
                                            Accounts = Get(x => x.RequestId == g.Key && x.IsActive == true).Select(x => x.Staff).ProjectTo<AccountViewModel>(_mapper.ConfigurationProvider).ToList()
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
                    var storageIds = account.StaffAssignStorages.Where(staffAssign => staffAssign.IsActive).Select(account => account.StorageId).ToList();
                    if (storageIds == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Nhân viên chưa được phân công vào kho");
                    var schedules = Get(x => x.IsActive && (storageIds.Contains((Guid)x.Request.StorageId) || storageIds.Contains((Guid)x.Request.Order.StorageId)))
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
                                            ScheduleTime = _utilService.TimeToString(g.First().ScheduleTime),
                                            Accounts = Get(x => x.RequestId == g.Key && x.IsActive == true).Select(x => x.Staff).ProjectTo<AccountViewModel>(_mapper.ConfigurationProvider).ToList()
                                        }).AsQueryable().PagingIQueryable(page, size, CommonConstant.LimitPaging, CommonConstant.DefaultPaging);
                }
                if (result.Item2 == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Schedule not found");
                if (result.Item2.ToList().Count < 1) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Schedule not found");
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
            catch (ErrorResponse e)
            {
                throw new ErrorResponse((int)e.Error.Code, e.Error.Message);
            }
            catch (Exception e)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, e.Message);
            }

        }
    }
}
