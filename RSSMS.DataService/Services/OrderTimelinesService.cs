using AutoMapper;
using AutoMapper.QueryableExtensions;
using RSSMS.DataService.Constants;
using RSSMS.DataService.Models;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.Responses;
using RSSMS.DataService.UnitOfWorks;
using RSSMS.DataService.Utilities;
using RSSMS.DataService.ViewModels.OrderTimelines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
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
            var timelines = Get()
                .ProjectTo<OrderTimelinesViewModel>(_mapper.ConfigurationProvider);


            var result = timelines
                 .DynamicFilter(model)
                .PagingIQueryable(page, size, CommonConstant.LimitPaging, CommonConstant.DefaultPaging);
            if (result.Item2.ToList().Count < 1) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Timelines not found");



            var rs = new DynamicModelResponse<OrderTimelinesViewModel>
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
