//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using RSSMS.DataService.Responses;
//using RSSMS.DataService.Services;
//using RSSMS.DataService.ViewModels.OrderBoxes;
//using System.Net;
//using System.Threading.Tasks;

//namespace RSSMS.API.Controllers
//{
//    [Route("api/v{version:apiVersion}/orderboxdetails")]
//    [ApiController]
//    [ApiVersion("1")]
//    public class OrderBoxDetailController : ControllerBase
//    {
//        private readonly IOrderBoxDetailService _orderBoxService;
//        public OrderBoxDetailController(IOrderBoxDetailService orderBoxService)
//        {
//            _orderBoxService = orderBoxService;
//        }
//        /// <summary>
//        /// Assign boxes to order
//        /// </summary>
//        /// <param name="model"></param>
//        /// <returns></returns>
//        [HttpPost]
//        [Authorize(Roles = "Manager,Office staff,Delivery Staff")]
//        [MapToApiVersion("1")]
//        [ProducesResponseType(typeof(OrderBoxesDetailViewModel), (int)HttpStatusCode.OK)]
//        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
//        public async Task<IActionResult> Add(OrderBoxesDetailViewModel model)
//        {
//            return Ok(await _orderBoxService.Create(model));
//        }

//        /// <summary>
//        /// Move box to another box
//        /// </summary>
//        /// <param name="model"></param>
//        /// <returns></returns>
//        [HttpPut]
//        [Authorize(Roles = "Manager,Office staff,Delivery Staff")]
//        [MapToApiVersion("1")]
//        [ProducesResponseType(typeof(OrderBoxesMoveViewModel), (int)HttpStatusCode.OK)]
//        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
//        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
//        public async Task<IActionResult> Update(OrderBoxesMoveViewModel model)
//        {
//            return Ok(await _orderBoxService.Update(model));
//        }
//    }
//}
