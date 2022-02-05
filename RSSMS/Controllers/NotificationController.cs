using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RSSMS.DataService.Constants;
using RSSMS.DataService.Responses;
using RSSMS.DataService.Services;
using RSSMS.DataService.ViewModels.Notifications;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace RSSMS.API.Controllers
{
    [Route("api/v{version:apiVersion}/notifications")]
    [ApiController]
    [ApiVersion("1")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notifService;
        public NotificationController(INotificationService notifService)
        {
            _notifService = notifService;
        }
        /// <summary>
        /// Get notification by user id
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="fields"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Manager,Office staff,Delivery Staff,Customer")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(DynamicModelResponse<NotificationViewModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromQuery] int userId, [FromQuery] string[] fields, int page = CommonConstant.DefaultPage, int size = -1)
        {
            return Ok(await _notifService.GetAll(userId, fields, page, size));
        }

        /// <summary>
        /// Update notification to readed by list of id
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [Authorize]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(NotificationUpdateViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Update(NotificationUpdateViewModel model)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            return Ok(await _notifService.Update(model, accessToken));
        }
    }
}
