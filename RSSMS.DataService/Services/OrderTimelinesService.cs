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
    public interface IOrderTimelinesService : IBaseService<OrderTimeline>
    {
        Task<DynamicModelResponse<OrderTimelinesViewModel>> Get(OrderTimelinesViewModel model, string[] fields, int page, int size);
    }
    public class OrderTimelinesService : BaseService<OrderTimeline>, IOrderTimelinesService
    {
        private readonly IMapper _mapper;
        public OrderTimelinesService(IUnitOfWork unitOfWork, IOrderTimelinesRepository repository, IMapper mapper) : base(unitOfWork, repository)
        {
            _mapper = mapper;
        }


        public async Task<DynamicModelResponse<OrderTimelinesViewModel>> Get(OrderTimelinesViewModel model, string[] fields, int page, int size)
        {
            var orderId = model.OrderId;
            model.OrderId = null;
            var timelines = Get(x => x.Request.OrderId == orderId).OrderByDescending(x => x.Datetime)
                                .ProjectTo<OrderTimelinesViewModel>(_mapper.ConfigurationProvider)
                                .DynamicFilter(model)
                                .PagingIQueryable(page, size, CommonConstant.LimitPaging, CommonConstant.DefaultPaging);
            if (timelines.Item2.ToList().Count < 1) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Timelines not found");



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

    }
}
