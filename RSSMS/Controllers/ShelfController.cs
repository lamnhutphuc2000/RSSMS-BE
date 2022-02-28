using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RSSMS.DataService.Constants;
using RSSMS.DataService.Responses;
using RSSMS.DataService.Services;
using RSSMS.DataService.ViewModels.Shelves;
using System;
using System.Net;
using System.Threading.Tasks;

namespace RSSMS.API.Controllers
{
    [Route("api/v{version:apiVersion}/shelves")]
    [ApiController]
    [ApiVersion("1")]
    public class ShelfController : ControllerBase
    {
        private readonly IShelfService _shelfService;
        public ShelfController(IShelfService shelfService)
        {
            _shelfService = shelfService;
        }
        /// <summary>
        /// Get all shelves
        /// </summary>
        /// <param name="model"></param>
        /// <param name="fields"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize(Roles = "Manager,Delivery Staff,Office staff,Admin")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(DynamicModelResponse<ShelfViewModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromQuery] ShelfViewModel model, [FromQuery] string[] fields, int page = CommonConstant.DefaultPage, int size = CommonConstant.DefaultPaging)
        {
            return Ok(await _shelfService.GetAll(model, fields, page, size));
        }
        /// <summary>
        /// Get shelf by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "Manager,Delivery Staff,Office staff,Admin")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(ShelfViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetById(Guid id)
        {
            return Ok(await _shelfService.GetById(id));
        }
        /// <summary>
        /// Create new shelf
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Manager,Office staff")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(ShelfViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Add(ShelfCreateViewModel model)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            return Ok(await _shelfService.Create(model,accessToken));
        }

        /// <summary>
        /// Update shelf by Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Manager,Office staff")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(ShelfViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Update(Guid id, ShelfUpdateViewModel model)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            return Ok(await _shelfService.Update(id, model, accessToken));
        }

        /// <summary>
        /// Delete shelf by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager,Office staff")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _shelfService.Delete(id);
            return Ok("Deleted successfully");
        }
    }
}
