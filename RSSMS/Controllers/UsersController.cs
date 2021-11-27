using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RSSMS.DataService.Constants;
using RSSMS.DataService.Responses;
using RSSMS.DataService.Services;
using RSSMS.DataService.ViewModels.JWT;
using RSSMS.DataService.ViewModels.Users;
using System.Net;
using System.Threading.Tasks;

namespace RSSMS.API.Controllers
{
    [Route("api/v{version:apiVersion}/users")]
    [ApiController]
    [ApiVersion("1")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        public UsersController(IUserService userService)
        {
            _userService = userService;
        }
        /// <summary>
        /// Login by email and password
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("login")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(TokenViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Login(UserLoginViewModel model)
        {
            return Ok(await _userService.Login(model));
        }
        /// <summary>
        /// Change password
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("changepassword")]
        [MapToApiVersion("1")]
        //[Authorize]
        [ProducesResponseType(typeof(UserViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ChangePassword(UserChangePasswordViewModel model)
        {
            return Ok(await _userService.ChangePassword(model));
        }
        /// <summary>
        /// Get all users
        /// </summary>
        /// <param name="model"></param>
        /// <param name="fields"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet]
        [MapToApiVersion("1")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(DynamicModelResponse<UserViewModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromQuery] UserViewModel model, [FromQuery] int? storageId, [FromQuery] int? orderId, [FromQuery] string[] fields, int page = CommonConstant.DefaultPage, int size = CommonConstant.DefaultPaging)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            return Ok(await _userService.GetAll(model, storageId, orderId, fields, page, size, accessToken));
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(UserViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetById(int id)
        {
            return Ok(await _userService.GetById(id));
        }
        /// <summary>
        /// Get User By Phone Number
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        [HttpGet("user/{phone}")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(UserViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetByPhone(string phone)
        {
            return Ok(await _userService.GetByPhone(phone));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(UserCreateViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Add(UserCreateViewModel model)
        {
            return Ok(await _userService.Create(model));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Update(int id, UserUpdateViewModel model)
        {
            return Ok(await _userService.Update(id, model));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Delete(int id)
        {
            await _userService.Delete(id);
            return Ok("Deleted successfully");
        }
    }
}
