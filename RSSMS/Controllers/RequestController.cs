using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RSSMS.DataService.Constants;
using RSSMS.DataService.Responses;
using RSSMS.DataService.Services;
using RSSMS.DataService.ViewModels.Notifications;
using RSSMS.DataService.ViewModels.Requests;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace RSSMS.API.Controllers
{
    [Route("api/v{version:apiVersion}/requests")]
    [ApiController]
    [ApiVersion("1")]
    public class RequestController : Controller
    {
        private readonly IRequestService _requestService;
        public RequestController(IRequestService requestService)
        {
            _requestService = requestService;
        }
        /// <summary>
        /// Get list request
        /// </summary>
        /// <param name="model"></param>
        /// <param name="RequestTypes"></param>
        /// <param name="RequestStatus"></param>
        /// <param name="fields"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Manager,Delivery Staff,Office Staff, Customer")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(DynamicModelResponse<RequestViewModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromQuery] RequestViewModel model, [FromQuery] IList<int> RequestTypes, [FromQuery] IList<int>  RequestStatus, [FromQuery] string[] fields, int page = CommonConstant.DefaultPage, int size = CommonConstant.DefaultPaging)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            return Ok(await _requestService.GetAll(model, RequestTypes, RequestStatus, fields, page, size, accessToken));
        }
        /// <summary>
        /// Get request by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Authorize]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(RequestByIdViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            return Ok(await _requestService.GetById(id, accessToken));
        }
        /// <summary>
        /// Create request
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Delivery Staff, Customer, Manager, Office Staff")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(RequestCreateViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Add(RequestCreateViewModel model)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            return Ok(await _requestService.Create(model, accessToken));
        }
        /// <summary>
        /// Update request
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Update(Guid id, RequestUpdateViewModel model)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            return Ok(await _requestService.Update(id, model, accessToken));
        }
        /// <summary>
        /// Delete request
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _requestService.Delete(id);
            return Ok("Deleted");
        }
        /// <summary>
        /// Assign request to storage
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("assign order")]
        [Authorize(Roles = "Manager, Office Staff")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(RequestByIdViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> AssignRequestToStorage(RequestAssignStorageViewModel model)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            return Ok(await _requestService.AssignStorage(model, accessToken));
        }

        /// <summary>
        /// Cancel request to create order
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("cancel request to order/{id}")]
        [Authorize(Roles = "Manager, Office Staff, Customer")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(RequestByIdViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Cancel(Guid id, RequestCancelViewModel model)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            return Ok(await _requestService.Cancel(id, model, accessToken));
        }

        /// <summary>
        /// Delivery staff send noti to customer
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut("deliver request/{id}")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(RequestByIdViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> DeliverRequest(Guid id)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            return Ok(await _requestService.DeliverRequest(id, accessToken));
        }

        /// <summary>
        /// Delivery staff send noti to staff
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("send request notification/{id}")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(RequestByIdViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> DeliverySendRequestNotification(Guid id, [FromBody] NotificationDeliverySendRequestNotiViewModel model)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            return Ok(await _requestService.DeliverySendRequestNotification(id, model, accessToken));
        }
    }
}
