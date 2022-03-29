using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RSSMS.DataService.Constants;
using RSSMS.DataService.Responses;
using RSSMS.DataService.Services;
using RSSMS.DataService.ViewModels.OrderTimelines;
using System.Net;
using System.Threading.Tasks;

namespace RSSMS.API.Controllers
{
    [Route("api/v{version:apiVersion}/ordertimelines")]
    [ApiController]
    [ApiVersion("1")]
    public class OrderTimelinesController : ControllerBase
    {
        private readonly IOrderTimelinesService _orderTimelinesService;
        public OrderTimelinesController(IOrderTimelinesService orderTimelinesService)
        {
            _orderTimelinesService = orderTimelinesService;
        }
        /// <summary>
        /// Get list of order timelines
        /// </summary>
        /// <param name="model"></param>
        /// <param name="fields"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Delivery Staff,Office Staff, Customer")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(DynamicModelResponse<OrderTimelinesViewModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetAreaByStorageID([FromQuery] OrderTimelinesViewModel model, [FromQuery] string[] fields, int page = CommonConstant.DefaultPage, int size = -1)
        {
            return Ok(await _orderTimelinesService.Get(model, fields, page, size));
        }
    }
}
