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
        [Authorize(Roles = "Admin,Manager,Office staff,Customer")]
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
        /// <returns></returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Manager,Office staff, Delivery Staff,Customer")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(OrderByIdViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetById(Guid id)
        {
            return Ok(await _orderService.GetById(id));
        }
        /// <summary>
        /// Create new order
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Manager,Office staff,Customer")]
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
        [Authorize(Roles = "Manager,Office staff, Customer, Delivery Staff")]
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
        /// <param name="orders"></param>
        /// <returns></returns>
        [HttpPut]
        [Authorize(Roles = "Manager,Office staff, Customer, Delivery Staff")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateOrders(List<OrderUpdateStatusViewModel> model)
        {
            return Ok(await _orderService.UpdateOrders(model));
        }
        /// <summary>
        /// Done the order by order Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut("done/{id}")]
        [Authorize(Roles = "Manager,Office staff, Delivery Staff")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Done(Guid id)
        {
            return Ok(await _orderService.Done(id));
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
        /// Send order notification to customer by delivery staff
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("{id}")]
        [Authorize(Roles = "Delivery Staff, Office staff")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(OrderViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> SendOrderNoti(Guid id, OrderViewModel model)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            return Ok(await _orderService.SendOrderNoti(model, accessToken));
        }
    }
}
