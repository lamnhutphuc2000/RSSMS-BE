﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RSSMS.DataService.Responses;
using RSSMS.DataService.Services;
using RSSMS.DataService.ViewModels.Products;
using System.Net;
using System.Threading.Tasks;

namespace RSSMS.API.Controllers
{

    [Route("api/v{version:apiVersion}/products")]
    [ApiController]
    [ApiVersion("1")]
    public class ProductController : Controller
    {
        private readonly IProductService _productsService;
        public ProductController(IProductService productService)
        {
            _productsService = productService;
        }
        /// <summary>
        /// Get all products
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(DynamicModelResponse<ProductViewAllModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromQuery] ProductViewAllModel model)
        {
            return Ok(await _productsService.GetAll(model));
        }
        /// <summary>
        /// Get product by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Authorize]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(ProductViewAllModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetById(int id)
        {
            return Ok(await _productsService.GetById(id));
        }
        /// <summary>
        /// Create new product
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(ProductCreateViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Add(ProductCreateViewModel model)
        {
            return Ok(await _productsService.Create(model));
        }
        /// <summary>
        /// Update product by Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Update(int id, ProductUpdateViewModel model)
        {
            return Ok(await _productsService.Update(id, model));
        }
        /// <summary>
        /// Delete product by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Delete(int id)
        {
            await _productsService.Delete(id);
            return Ok("Deleted");
        }
    }
}
