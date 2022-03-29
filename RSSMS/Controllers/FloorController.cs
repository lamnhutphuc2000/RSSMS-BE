using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RSSMS.DataService.Responses;
using RSSMS.DataService.Services;
using RSSMS.DataService.ViewModels.Floors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RSSMS.API.Controllers
{
    [Route("api/v{version:apiVersion}/floors")]
    [ApiController]
    [ApiVersion("1")]
    public class FloorController : ControllerBase
    {
        private readonly IFloorsService _floorService;
        public FloorController(IFloorsService floorService)
        {
            _floorService = floorService;
        }
        /// <summary>
        /// Get floor by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "Manager,Delivery Staff,Office Staff,Admin")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(FloorGetByIdViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetById(Guid id)
        {
            return Ok(await _floorService.GetById(id));
        }
    }
}
