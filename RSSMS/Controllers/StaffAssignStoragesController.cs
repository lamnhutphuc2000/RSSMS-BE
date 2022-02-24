using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RSSMS.DataService.Responses;
using RSSMS.DataService.Services;
using RSSMS.DataService.ViewModels.StaffAssignStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RSSMS.API.Controllers
{
    [Route("api/v{version:apiVersion}/staff-assign-storages")]
    [ApiController]
    [ApiVersion("1")]
    public class StaffAssignStoragesController : ControllerBase
    {
        private readonly IStaffAssignStoragesService _staffAssignStoragesService;
        public StaffAssignStoragesController(IStaffAssignStoragesService staffAssignStoragesService)
        {
            _staffAssignStoragesService = staffAssignStoragesService;
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
            return Ok(await _staffAssignStoragesService.AssignStaffToStorage(model, accessToken));
        }

    }
}
