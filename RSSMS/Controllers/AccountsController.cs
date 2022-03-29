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
    public class AccountsController : ControllerBase
    {
        private readonly IAccountsService _accountsService;
        public AccountsController(IAccountsService accountsService)
        {
            _accountsService = accountsService;
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
        public async Task<IActionResult> Login(AccountsLoginViewModel model)
        {
            return Ok(await _accountsService.Login(model));
        }

        /// <summary>
        /// Login by third party
        /// </summary>
        /// <param name="firebaseID"></param>
        /// <param name="deviceToken"></param>
        /// <returns></returns>
        [HttpPost("thirdparty")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(TokenViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CheckLogin(string firebaseID, string deviceToken)
        {
            return Ok(await _accountsService.CheckLogin(firebaseID, deviceToken));
        }

        /// <summary>
        /// Change password
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("changepassword")]
        [MapToApiVersion("1")]
        [Authorize]
        [ProducesResponseType(typeof(AccountsViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ChangePassword(AccountsChangePasswordViewModel model)
        {
            return Ok(await _accountsService.ChangePassword(model));
        }

        /// <summary>
        /// Get all accounts
        /// </summary>
        /// <param name="model"></param>
        /// <param name="storageId"></param>
        /// <param name="orderId"></param>
        /// <param name="fields"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet]
        [MapToApiVersion("1")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(DynamicModelResponse<AccountsViewModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Get([FromQuery] AccountsViewModel model, [FromQuery] Guid? storageId, [FromQuery] Guid? orderId, [FromQuery] string[] fields, int page = CommonConstant.DefaultPage, int size = CommonConstant.DefaultPaging)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            return Ok(await _accountsService.GetAll(model, storageId, orderId, fields, page, size, accessToken));
        }

        /// <summary>
        /// Get staffs
        /// </summary>
        /// <param name="storageId"></param>
        /// <param name="roleName"></param>
        /// <returns></returns>
        [HttpGet("staffs")]
        [MapToApiVersion("1")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(List<AccountsViewModel>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetStaffs([FromQuery] Guid? storageId, [FromQuery] List<string> roleName)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            return Ok(await _accountsService.GetStaff(storageId, accessToken, roleName));
        }

        /// <summary>
        /// Get account by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(AccountsViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetById(Guid id)
        {
            return Ok(await _accountsService.GetById(id));
        }

        /// <summary>
        /// Get account by phone number
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        [HttpGet("account/{phone}")]
        [Authorize(Roles = "Admin,Manager,Office Staff")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(AccountsViewModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetByPhone(string phone)
        {
            return Ok(await _accountsService.GetByPhone(phone));
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
        public async Task<IActionResult> Add(AccountsCreateViewModel model)
        {
            return Ok(await _accountsService.Create(model));
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
        public async Task<IActionResult> Update(Guid id, AccountsUpdateViewModel model)
        {
            return Ok(await _accountsService.Update(id, model));
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
            await _accountsService.Delete(id);
            return Ok("Deleted successfully");
        }
    }
}
