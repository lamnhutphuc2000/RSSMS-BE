using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Constants;
using RSSMS.DataService.Models;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.Responses;
using RSSMS.DataService.UnitOfWorks;
using RSSMS.DataService.Utilities;
using RSSMS.DataService.ViewModels.Roles;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RSSMS.DataService.Services
{
    public interface IRolesService : IBaseService<Role>
    {
        Task<DynamicModelResponse<RolesViewModel>> GetAll(string[] fields, int page, int size, string accessToken);
    }
    public class RoleService : BaseService<Role>, IRolesService

    {
        private readonly IMapper _mapper;
        public RoleService(IUnitOfWork unitOfWork, IRolesRepository repository, IMapper mapper) : base(unitOfWork, repository)
        {
            _mapper = mapper;
        }
        public async Task<DynamicModelResponse<RolesViewModel>> GetAll(string[] fields, int page, int size, string accessToken)
        {

            var roles = Get(x => x.Name != "Admin" && x.IsActive == true)
                .ProjectTo<RolesViewModel>(_mapper.ConfigurationProvider);
            (int, IQueryable<RolesViewModel>) result;
            DynamicModelResponse<RolesViewModel> rs;
            if (accessToken == null)
            {
                roles = roles.Where(x => x.Name == "Customer");
            }
            else
            {
                var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
                var userId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);
                var role = secureToken.Claims.First(claim => claim.Type.Contains("role")).Value;

                if (role == "Manager") roles = roles.Where(x => x.Name != "Manager");
                if (role == "Office Staff") roles = roles.Where(x => x.Name == "Office Staff");
                if (role == "Customer") roles = roles.Where(x => x.Name == "Customer");
            }

            result = roles.PagingIQueryable(page, size, CommonConstant.LimitPaging, CommonConstant.DefaultPaging);
            if (result.Item2.ToList().Count < 1) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Roles not found");
            rs = new DynamicModelResponse<RolesViewModel>
            {

                Metadata = new PagingMetaData
                {
                    Page = page,
                    Size = size,
                    Total = result.Item1,
                    TotalPage = (int)Math.Ceiling((double)result.Item1 / size)
                },
                Data = await result.Item2.ToListAsync()
            };

            return rs;
        }

    }
}
