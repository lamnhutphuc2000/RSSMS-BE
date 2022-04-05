using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RSSMS.DataService.Constants;
using RSSMS.DataService.Responses;
using RSSMS.DataService.Services;
using RSSMS.DataService.ViewModels.Accounts;
using RSSMS.DataService.ViewModels.JWT;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace RSSMS.API.Controllers
{
    [Route("api/v{version:apiVersion}/accounts")]
    [ApiController]
    [ApiVersion("1")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        public AccountController(IAccountService accountsService)
        {
            _accountService = accountsService;
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
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Login(AccountLoginViewModel model)
        {
            return Ok(await _accountService.Login(model));
        }


        /// <summary>
        /// Change password
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("changepassword")]
        [MapToApiVersion("1")]
        [Authorize]
        [ProducesResponseType(typeof(AccountViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ChangePassword(AccountChangePasswordViewModel model)
        {
            return Ok(await _accountService.ChangePassword(model));
        }

        /// <summary>
        /// Get account list
        /// </summary>
        /// <param name="model"></param>
        /// <param name="storageId"></param>
        /// <param name="fields"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet]
        [MapToApiVersion("1")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(DynamicModelResponse<AccountViewModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromQuery] AccountViewModel model, [FromQuery] Guid? storageId, [FromQuery] string[] fields, int page = CommonConstant.DefaultPage, int size = CommonConstant.DefaultPaging)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            return Ok(await _accountService.GetAll(model, storageId, fields, page, size, accessToken));
        }

        /// <summary>
        /// Get staffs
        /// </summary>
        /// <param name="storageId"></param>
        /// <param name="roleName"></param>
        /// <param name="scheduleDay"></param>
        /// <param name="deliveryTimes"></param>
        /// <returns></returns>
        [HttpGet("staffs")]
        [MapToApiVersion("1")]
        [Authorize(Roles = "Admin,Manager, Office Staff")]
        [ProducesResponseType(typeof(List<AccountViewModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetStaffs([FromQuery] Guid? storageId, [FromQuery] List<string> roleName, [FromQuery] DateTime? scheduleDay, [FromQuery] ICollection<string> deliveryTimes, [FromQuery] bool getFromAllStorage)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            return Ok(await _accountService.GetStaff(storageId, accessToken, roleName, scheduleDay, deliveryTimes, getFromAllStorage));
        }

        /// <summary>
        /// Get account by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(AccountViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetById(Guid id)
        {
            return Ok(await _accountService.GetById(id));
        }

        /// <summary>
        /// Get account by phone number
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        [HttpGet("account/{phone}")]
        [Authorize(Roles = "Admin")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(AccountViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetByPhone(string phone)
        {
            return Ok(await _accountService.GetByPhone(phone));
        }

        /// <summary>
        /// Create new account
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(TokenViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Add(AccountCreateViewModel model)
        {
            return Ok(await _accountService.Create(model));
        }

        /// <summary>
        /// Update account by Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [Authorize]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Update(Guid id, AccountUpdateViewModel model)
        {
            return Ok(await _accountService.Update(id, model));
        }

        /// <summary>
        /// Delete account by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _accountService.Delete(id);
            return Ok("Deleted successfully");
        }
    }
}
