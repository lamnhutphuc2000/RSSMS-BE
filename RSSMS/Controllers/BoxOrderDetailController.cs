using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RSSMS.DataService.Responses;
using RSSMS.DataService.Services;
using RSSMS.DataService.ViewModels.BoxOrderDetails;
using RSSMS.DataService.ViewModels.OrderBoxes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RSSMS.API.Controllers
{
    [Route("api/v{version:apiVersion}/box-order-details")]
    [ApiController]
    [ApiVersion("1")]
    public class BoxOrderDetailController : ControllerBase
    {
        private readonly IBoxOrderDetailService _boxOrderDetailService;
        public BoxOrderDetailController(IBoxOrderDetailService boxOrderDetailService)
        {
            _boxOrderDetailService = boxOrderDetailService;
        }
        /// <summary>
        /// Assign order details to boxes
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Manager,Office staff,Delivery Staff")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(BoxOrderDetailCreateViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Add(BoxOrderDetailCreateViewModel model)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            return Ok(await _boxOrderDetailService.Create(model,accessToken));
        }

        /// <summary>
        /// Move order detail to another box
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [Authorize(Roles = "Manager,Office staff,Delivery Staff")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(BoxOrderDetailUpdateViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Update(BoxOrderDetailUpdateViewModel model)
        {
            return Ok(await _boxOrderDetailService.Update(model));
        }
    }
}
