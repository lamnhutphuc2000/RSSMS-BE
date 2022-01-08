using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RSSMS.DataService.Responses;
using RSSMS.DataService.Services;
using RSSMS.DataService.ViewModels.StaffManageUser;
using System.Net;
using System.Threading.Tasks;

namespace RSSMS.API.Controllers
{
    [Route("api/v{version:apiVersion}/staff-manage-storages")]
    [ApiController]
    [ApiVersion("1")]
    public class StaffManageStorageController : ControllerBase
    {
        private readonly IStaffManageStorageService _staffManageStorage;
        public StaffManageStorageController(IStaffManageStorageService staffManageStorage)
        {
            _staffManageStorage = staffManageStorage;
        }

        /// <summary>
        /// Assign staff to storage
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("assign-staff")]
        [Authorize(Roles = "Admin,Manager")]
        [MapToApiVersion("1")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Assign(StaffAssignViewModel model)
        {
            return Ok(await _staffManageStorage.AssignStaffToStorage(model));
        }

    }
}
