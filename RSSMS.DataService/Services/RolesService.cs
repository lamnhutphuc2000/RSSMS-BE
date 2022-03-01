using AutoMapper;
using AutoMapper.QueryableExtensions;
using RSSMS.DataService.Constants;
using RSSMS.DataService.Models;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.Responses;
using RSSMS.DataService.UnitOfWorks;
using RSSMS.DataService.Utilities;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RSSMS.DataService.Services
{
    public interface IRolesService : IBaseService<Role>
    {
        Task<DynamicModelResponse<Role>> GetAll(string[] fields, int page, int size, string accessToken);
    }
    public class RolesService : BaseService<Role>, IRolesService

    {
        private readonly IMapper _mapper;
        public RolesService(IUnitOfWork unitOfWork, IRolesRepository repository, IMapper mapper) : base(unitOfWork, repository)
        {
            _mapper = mapper;
        }
        public async Task<DynamicModelResponse<Role>> GetAll(string[] fields, int page, int size, string accessToken)
        {

            var roles = Get(x => x.Name != "Admin" && x.IsActive == true)
                .ProjectTo<Role>(_mapper.ConfigurationProvider);
            (int, IQueryable<Role>) result;
            DynamicModelResponse<Role> rs;
            if (accessToken == null)
            {
                roles = roles.Where(x => x.Name == "Customer");
                result = roles.PagingIQueryable(page, size, CommonConstant.LimitPaging, CommonConstant.DefaultPaging);
                if (result.Item2.ToList().Count < 1) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Roles not found");
                rs = new DynamicModelResponse<Role>
                {

                    Metadata = new PagingMetaData
                    {
                        Page = page,
                        Size = size,
                        Total = result.Item1,
                        TotalPage = (int)Math.Ceiling((double)result.Item1 / size)
                    },
                    Data = result.Item2.ToList()
                };

                return rs;
            }

            var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
            var userId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);




            var role = secureToken.Claims.First(claim => claim.Type.Contains("role")).Value;

            
            if (role == "Manager") roles = roles.Where(x => x.Name != "Manager");
            if (role == "Office staff") roles = roles.Where(x => x.Name == "Office staff");
            if (role == "Customer") roles = roles.Where(x => x.Name == "Customer");
            result = roles.PagingIQueryable(page, size, CommonConstant.LimitPaging, CommonConstant.DefaultPaging);
            if (result.Item2.ToList().Count < 1) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Roles not found");



            rs = new DynamicModelResponse<Role>
            {

                Metadata = new PagingMetaData
                {
                    Page = page,
                    Size = size,
                    Total = result.Item1,
                    TotalPage = (int)Math.Ceiling((double)result.Item1 / size)
                },
                Data = result.Item2.ToList()
            };

            return rs;
        }

    }
}
