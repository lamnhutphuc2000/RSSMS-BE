using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RSSMS.DataService.Constants;
using RSSMS.DataService.Responses;
using RSSMS.DataService.Services;
using RSSMS.DataService.ViewModels.StaffAssignStorage;
using RSSMS.DataService.ViewModels.Storages;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace RSSMS.API.Controllers
{
    [Route("api/v{version:apiVersion}/storages")]
    [ApiController]
    [ApiVersion("1")]
    public class StorageController : ControllerBase
    {
        private readonly IStorageService _storagesService;
        public StorageController(IStorageService storageService)
        {
            _storagesService = storageService;
        }
        /// <summary>
        /// Get all storages
        /// </summary>
        /// <param name="model"></param>
        /// <param name="types"></param>
        /// <param name="fields"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(DynamicModelResponse<StorageViewModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromQuery] StorageViewModel model, [FromQuery] List<int> types, [FromQuery] string[] fields, int page = CommonConstant.DefaultPage, int size = CommonConstant.DefaultPaging)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            return Ok(await _storagesService.GetAll(model, types, fields, page, size, accessToken));
        }
        /// <summary>
        /// Get storage by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Manager,Office Staff")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(StorageDetailViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            return Ok(await _storagesService.GetById(id, accessToken));
        }
        /// <summary>
        /// Create new storage
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(StorageViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Add(StorageCreateViewModel model)
        {
            return Ok(await _storagesService.Create(model));
        }
        /// <summary>
        /// Update storage by Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Update(Guid id, StorageUpdateViewModel model)
        {
            return Ok(await _storagesService.Update(id, model));
        }
        /// <summary>
        /// Delete storage by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _storagesService.Delete(id);
            return Ok("Deleted");
        }

        /// <summary>
        /// Assign staff to storage
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("assign-staff-to-storage")]
        [Authorize(Roles = "Admin,Manager")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Assign(StaffAssignInStorageViewModel model)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            return Ok(await _storagesService.AssignStaffToStorage(model, accessToken));
        }
    }
}
