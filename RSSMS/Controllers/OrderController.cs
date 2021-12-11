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
        /// Get All Order
        /// </summary>
        /// <param name="model"></param>
        /// <param name="fields"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Manager,Office staff")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(OrderViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetAll([FromQuery] OrderViewModel model, [FromQuery] IList<int> OrderStatuses, [FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo, [FromQuery] string[] fields, int page = CommonConstant.DefaultPage, int size = CommonConstant.DefaultPaging)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            return Ok(await _orderService.GetAll(model, OrderStatuses, dateFrom, dateTo, fields, page, size, accessToken));
        }
        /// <summary>
        /// Get Order By ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(OrderViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetById(int id)
        {
            return Ok(await _orderService.GetById(id));
        }
        /// <summary>
        /// Create Order
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Manager,Office staff")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(OrderCreateViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Add(OrderCreateViewModel model)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            return Ok(await _orderService.Create(model, accessToken));
        }

        /// <summary>
        /// Update Order
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Update(int id, OrderUpdateViewModel model)
        {
            return Ok(await _orderService.Update(id, model));
        }
        /// <summary>
        /// Cancel order
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut("cancel/{id}")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(OrderViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Cancel(int id, OrderCancelViewModel model)
        {
            return Ok(await _orderService.Cancel(id, model));
        }
    }
}
