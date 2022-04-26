using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Constants;
using RSSMS.DataService.Models;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.Responses;
using RSSMS.DataService.UnitOfWorks;
using RSSMS.DataService.Utilities;
using RSSMS.DataService.ViewModels.OrderTimelines;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RSSMS.DataService.Services
{
    public interface IOrderTimelineService : IBaseService<OrderTimeline>
    {
        Task<DynamicModelResponse<OrderTimelinesViewModel>> Get(OrderTimelinesViewModel model, string[] fields, int page, int size);
    }
    public class OrderTimelineService : BaseService<OrderTimeline>, IOrderTimelineService
    {
        private readonly IMapper _mapper;
        private readonly IRequestTimelineService _requestTimelineService;
        public OrderTimelineService(IUnitOfWork unitOfWork, IOrderTimelineRepository repository, IMapper mapper, IRequestTimelineService requestTimelineService) : base(unitOfWork, repository)
        {
            _mapper = mapper;
            _requestTimelineService = requestTimelineService;
        }


        public async Task<DynamicModelResponse<OrderTimelinesViewModel>> Get(OrderTimelinesViewModel model, string[] fields, int page, int size)
        {
            try
            {
                var orderId = model.OrderId;
                if (orderId == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Không tìm thấy đơn");
                model.OrderId = null;
                var requestTimelines = _requestTimelineService.Get(timeline => timeline.Request.OrderId == orderId).ProjectTo<OrderTimelinesViewModel>(_mapper.ConfigurationProvider);
                var orderTimelines = Get(timeline => timeline.OrderId == orderId).ProjectTo<OrderTimelinesViewModel>(_mapper.ConfigurationProvider);
                requestTimelines.Union(orderTimelines);
                var timelines = requestTimelines.OrderByDescending(timeline => timeline.Datetime).DynamicFilter(model)
                                    .PagingIQueryable(page, size, CommonConstant.LimitPaging, CommonConstant.DefaultPaging);
                if (timelines.Item2.ToList().Count < 1) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Không tìm thấy dòng thời gian");


                var rs = new DynamicModelResponse<OrderTimelinesViewModel>
                {

                    Metadata = new PagingMetaData
                    {
                        Page = page,
                        Size = size,
                        Total = timelines.Item1,
                        TotalPage = (int)Math.Ceiling((double)timelines.Item1 / size)
                    },
                    Data = await timelines.Item2.ToListAsync()
                };

                return rs;
            }
            catch (ErrorResponse e)
            {
                throw new ErrorResponse((int)e.Error.Code, e.Error.Message);
            }
            catch (Exception ex)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, ex.Message);
            }

        }

    }
}
