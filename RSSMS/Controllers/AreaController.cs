using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RSSMS.DataService.Constants;
using RSSMS.DataService.Responses;
using RSSMS.DataService.Services;
using RSSMS.DataService.ViewModels.Areas;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace RSSMS.API.Controllers
{

    [Route("api/v{version:apiVersion}/areas")]
    [ApiController]
    [ApiVersion("1")]
    public class AreaController : Controller
    {
        private readonly IAreaService _areaService;
        public AreaController(IAreaService areaService)
        {
            _areaService = areaService;
        }
        /// <summary>
        /// Get list of area by storage Id
        /// </summary>
        /// <param name="storageid"></param>
        /// <param name="model"></param>
        /// <param name="fields"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Delivery Staff,Office Staff")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(DynamicModelResponse<AreaViewModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetAreaByStorageID([FromQuery] Guid storageid, [FromQuery] AreaViewModel model, [FromQuery] List<int> types, [FromQuery] string[] fields, int page = CommonConstant.DefaultPage, int size = -1)
        {
            return Ok(await _areaService.GetByStorageId(storageid, model, types, fields, page, size));
        }
        /// <summary>
        /// Get area by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Manager,Delivery Staff,Office Staff")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(AreaDetailViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetById(Guid id)
        {
            return Ok(await _areaService.GetById(id));
        }
        /// <summary>
        /// Create new area
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Manager,Office Staff")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(AreaViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Add(AreaCreateViewModel model)
        {
            return Ok(await _areaService.Create(model));
        }
        /// <summary>
        /// Delete area by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager,Office Staff")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _areaService.Delete(id);
            return Ok("Deleted");
        }
        /// <summary>
        /// Update area by Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Manager,Office Staff")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Update(Guid id, AreaUpdateViewModel model)
        {
            return Ok(await _areaService.Update(id, model));
        }

    }
}
