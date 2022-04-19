using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RSSMS.DataService.Constants;
using RSSMS.DataService.Responses;
using RSSMS.DataService.Services;
using RSSMS.DataService.ViewModels.Orders;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace RSSMS.API.Controllers
{
    [Route("api/v{version:apiVersion}/orders")]
    [ApiController]
    [ApiVersion("1")]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Get list orders
        /// </summary>
        /// <param name="model"></param>
        /// <param name="OrderStatuses"></param>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <param name="fields"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Office Staff,Customer")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(OrderViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetAll([FromQuery] OrderViewModel model, [FromQuery] IList<int> OrderStatuses, [FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo, [FromQuery] string[] fields, int page = CommonConstant.DefaultPage, int size = CommonConstant.DefaultPaging)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            return Ok(await _orderService.GetAll(model, OrderStatuses, dateFrom, dateTo, fields, page, size, accessToken));
        }
        /// <summary>
        /// Get order by Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="requestTypes"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Manager,Office Staff, Delivery Staff, Customer")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(OrderByIdViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetById(Guid id, [FromQuery] IList<int> requestTypes)
        {
            return Ok(await _orderService.GetById(id, requestTypes));
        }
        /// <summary>
        /// Create new order
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Manager,Office Staff, Delivery Staff, Customer")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(OrderCreateViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Add(OrderCreateViewModel model)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            return Ok(await _orderService.Create(model, accessToken));
        }

        /// <summary>
        /// Update order by Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Manager,Office Staff, Customer, Delivery Staff")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Update(Guid id, OrderUpdateViewModel model)
        {
            return Ok(await _orderService.Update(id, model));
        }
        /// <summary>
        /// Update list orders status by id
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [Authorize(Roles = "Manager,Office Staff, Customer, Delivery Staff")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateOrders(List<OrderUpdateStatusViewModel> model)
        {
            return Ok(await _orderService.UpdateOrders(model));
        }
        /// <summary>
        /// Done the order by order Id and request id
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("done/order/{orderId}/request/{requestId}")]
        [Authorize(Roles = "Manager,Office Staff, Delivery Staff")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Done(OrderDoneViewModel model)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            return Ok(await _orderService.Done(model, accessToken));
        }
        /// <summary>
        /// Cancel order by Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("cancel/{id}")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(OrderViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Cancel(Guid id, OrderCancelViewModel model)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            return Ok(await _orderService.Cancel(id, model, accessToken));
        }
        /// <summary>
        /// Assign order to storage
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("assign to storage")]
        [Authorize(Roles = "Manager")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(OrderViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> AssignOrderToStorage(OrderAssignStorageViewModel model)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            return Ok(await _orderService.AssignStorage(model, accessToken));
        }

        /// <summary>
        /// Assign order to floor
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("assign to floor")]
        [Authorize(Roles = "Office Staff")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(OrderViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> AssignOrderToFloor(OrderAssignFloorViewModel model)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            return Ok(await _orderService.AssignFloor(model, accessToken));
        }

        /// <summary>
        /// Assign order to another floor
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("assign to another floor")]
        [Authorize(Roles = "Manager, Office Staff")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(OrderViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> AssignOrderToAnotherFloor(OrderAssignAnotherFloorViewModel model)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            return Ok(await _orderService.AssignAnotherFloor(model, accessToken));
        }
    }
}
