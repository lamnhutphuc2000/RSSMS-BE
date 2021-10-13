using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RSSMS.DataService.Constants;
using RSSMS.DataService.Responses;
using RSSMS.DataService.Services;
using RSSMS.DataService.ViewModels.Shelves;

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
        [MapToApiVersion("1")]
        //[Authorize]
        [ProducesResponseType(typeof(DynamicModelResponse<ShelfViewModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromQuery] ShelfViewModel model,[FromQuery] string[] fields, int page = CommonConstant.DefaultPage, int size = CommonConstant.DefaultPaging)
        {
            return Ok(await _shelfService.GetAll(model, fields, page, size));
        }

        [HttpGet("{id}")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(ShelfViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetById(int id)
        {
            return Ok(await _shelfService.GetById(id));
        }

        [HttpPost]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(ShelfViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Add(ShelfCreateViewModel model)
        {
            return Ok(await _shelfService.Create(model));
        }

        [HttpPut("{id}")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(ShelfViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Update(int id, ShelfUpdateViewModel model)
        {
            return Ok(await _shelfService.Update(id, model));
        }

        [HttpDelete("{id}")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Delete(int id)
        {
            await _shelfService.Delete(id);
            return Ok("Deleted successfully");
        }
    }
}
