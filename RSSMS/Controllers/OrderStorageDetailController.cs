﻿using Microsoft.AspNetCore.Mvc;
using RSSMS.DataService.Responses;
using RSSMS.DataService.Services;
using RSSMS.DataService.ViewModels.OrderStorages;
using System.Net;
using System.Threading.Tasks;

namespace RSSMS.API.Controllers
{
    [Route("api/v{version:apiVersion}/orderstoragedetails")]
    [ApiController]
    [ApiVersion("1")]
    public class OrderStorageDetailController : ControllerBase
    {
        private readonly IOrderStorageDetailService _orderStorage;
        public OrderStorageDetailController(IOrderStorageDetailService orderStorage)
        {
            _orderStorage = orderStorage;
        }
        /// <summary>
        /// Assign storage to order
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(OrderStorageDetailViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Add(OrderStorageDetailViewModel model)
        {
            return Ok(await _orderStorage.Create(model));
        }
    }
}