using Microsoft.AspNetCore.Mvc;
using RSSMS.DataService.Constants;
using RSSMS.DataService.Responses;
using RSSMS.DataService.Services;
using RSSMS.DataService.ViewModels.Schedules;
using System.Net;
using System.Threading.Tasks;

namespace RSSMS.API.Controllers
{
    [Route("api/v{version:apiVersion}/schedules")]
    [ApiController]
    [ApiVersion("1")]
    public class ShceduleController : ControllerBase
    {
        private readonly IScheduleService _scheduleService;
        public ShceduleController(IScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }
        /// <summary>
        /// Get schedule by date from and date to
        /// </summary>
        /// <param name="model"></param>
        /// <param name="fields"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(DynamicModelResponse<ScheduleViewModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Gets([FromQuery] ScheduleSearchViewModel model, [FromQuery] string[] fields, int page = CommonConstant.DefaultPage, int size = -1)
        {
            return Ok(await _scheduleService.Get(model, fields, page, size));
        }
        /// <summary>
        /// Create new schedule
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(ScheduleOrderViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Add(ScheduleCreateViewModel model)
        {
            return Ok(await _scheduleService.Create(model));
        }
    }
}
