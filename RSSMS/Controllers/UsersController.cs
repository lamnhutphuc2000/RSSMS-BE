//using Microsoft.AspNetCore.Authentication;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using RSSMS.DataService.Constants;
//using RSSMS.DataService.Responses;
//using RSSMS.DataService.Services;
//using RSSMS.DataService.ViewModels.JWT;
//using RSSMS.DataService.ViewModels.Users;
//using System;
//using System.Net;
//using System.Threading.Tasks;

//namespace RSSMS.API.Controllers
//{
//    [Route("api/v{version:apiVersion}/users")]
//    [ApiController]
//    [ApiVersion("1")]
//    public class UsersController : ControllerBase
//    {
//        private readonly IUserService _userService;
//        public UsersController(IUserService userService)
//        {
//            _userService = userService;
//        }
//        /// <summary>
//        /// Login by email and password
//        /// </summary>
//        /// <param name="model"></param>
//        /// <returns></returns>
//        [HttpPost("login")]
//        [MapToApiVersion("1")]
//        [ProducesResponseType(typeof(TokenViewModel), (int)HttpStatusCode.OK)]
//        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
//        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
//        public async Task<IActionResult> Login(UserLoginViewModel model)
//        {
//            return Ok(await _userService.Login(model));
//        }

//        /// <summary>
//        /// Login by third party
//        /// </summary>
//        /// <param name="firebaseID"></param>
//        /// <param name="deviceToken"></param>
//        /// <returns></returns>
//        [HttpPost("thirdparty")]
//        [MapToApiVersion("1")]
//        [ProducesResponseType(typeof(TokenViewModel), (int)HttpStatusCode.OK)]
//        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
//        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
//        public async Task<IActionResult> CheckLogin(string firebaseID, string deviceToken)
//        {
//            return Ok(await _userService.CheckLogin(firebaseID, deviceToken));
//        }

//        /// <summary>
//        /// Change password
//        /// </summary>
//        /// <param name="model"></param>
//        /// <returns></returns>
//        [HttpPost("changepassword")]
//        [MapToApiVersion("1")]
//        [Authorize]
//        [ProducesResponseType(typeof(UserViewModel), (int)HttpStatusCode.OK)]
//        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
//        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
//        public async Task<IActionResult> ChangePassword(UserChangePasswordViewModel model)
//        {
//            return Ok(await _userService.ChangePassword(model));
//        }

//        /// <summary>
//        /// Get all users
//        /// </summary>
//        /// <param name="model"></param>
//        /// <param name="storageId"></param>
//        /// <param name="orderId"></param>
//        /// <param name="fields"></param>
//        /// <param name="page"></param>
//        /// <param name="size"></param>
//        /// <returns></returns>
//        [HttpGet]
//        [MapToApiVersion("1")]
//        [Authorize(Roles = "Admin,Manager")]
//        [ProducesResponseType(typeof(DynamicModelResponse<UserViewModel>), (int)HttpStatusCode.OK)]
//        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
//        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
//        public async Task<IActionResult> Get([FromQuery] UserViewModel model, [FromQuery] int? storageId, [FromQuery] int? orderId, [FromQuery] string[] fields, int page = CommonConstant.DefaultPage, int size = CommonConstant.DefaultPaging)
//        {
//            var accessToken = await HttpContext.GetTokenAsync("access_token");
//            return Ok(await _userService.GetAll(model, storageId, orderId, fields, page, size, accessToken));
//        }

//        /// <summary>
//        /// Get user by Id
//        /// </summary>
//        /// <param name="id"></param>
//        /// <returns></returns>
//        [HttpGet("{id}")]
//        [Authorize(Roles = "Admin,Manager")]
//        [MapToApiVersion("1")]
//        [ProducesResponseType(typeof(UserViewModel), (int)HttpStatusCode.OK)]
//        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
//        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
//        public async Task<IActionResult> GetById(Guid id)
//        {
//            return Ok(await _userService.GetById(id));
//        }

//        /// <summary>
//        /// Get user by phone number
//        /// </summary>
//        /// <param name="phone"></param>
//        /// <returns></returns>
//        [HttpGet("user/{phone}")]
//        [Authorize(Roles = "Admin,Manager,Office staff")]
//        [MapToApiVersion("1")]
//        [ProducesResponseType(typeof(UserViewModel), (int)HttpStatusCode.OK)]
//        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
//        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
//        public async Task<IActionResult> GetByPhone(string phone)
//        {
//            return Ok(await _userService.GetByPhone(phone));
//        }

//        /// <summary>
//        /// Create new user
//        /// </summary>
//        /// <param name="model"></param>
//        /// <returns></returns>
//        [HttpPost]
//        [MapToApiVersion("1")]
//        [ProducesResponseType(typeof(TokenViewModel), (int)HttpStatusCode.OK)]
//        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
//        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
//        public async Task<IActionResult> Add(UserCreateViewModel model)
//        {
//            return Ok(await _userService.Create(model));
//        }

//        /// <summary>
//        /// Update user by Id
//        /// </summary>
//        /// <param name="id"></param>
//        /// <param name="model"></param>
//        /// <returns></returns>
//        [HttpPut("{id}")]
//        [Authorize]
//        [MapToApiVersion("1")]
//        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
//        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
//        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
//        public async Task<IActionResult> Update(Guid id, UserUpdateViewModel model)
//        {
//            return Ok(await _userService.Update(id, model));
//        }

//        /// <summary>
//        /// Delete user by Id
//        /// </summary>
//        /// <param name="id"></param>
//        /// <param name="firebaseID"></param>
//        /// <returns></returns>
//        [HttpDelete("{id}")]
//        [Authorize(Roles = "Admin,Manager")]
//        [MapToApiVersion("1")]
//        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
//        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
//        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
//        public async Task<IActionResult> Delete(Guid id, string firebaseID)
//        {
//            await _userService.Delete(id, firebaseID);
//            return Ok("Deleted successfully");
//        }
//    }
//}
