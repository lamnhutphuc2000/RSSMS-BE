using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Models;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.Responses;
using RSSMS.DataService.UnitOfWorks;
using RSSMS.DataService.ViewModels.StaffAssignStorage;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RSSMS.DataService.Services
{
    public interface IStaffAssignStorageService : IBaseService<StaffAssignStorage>
    {
    }
    public class StaffAssignStorageService : BaseService<StaffAssignStorage>, IStaffAssignStorageService
    {
        public StaffAssignStorageService(IUnitOfWork unitOfWork, IStaffAssignStorageRepository repository) : base(unitOfWork, repository)
        {
        }

    }
}
