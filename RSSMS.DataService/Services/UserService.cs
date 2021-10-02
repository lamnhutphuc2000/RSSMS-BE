using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using RSSMS.DataService.Constants;
using RSSMS.DataService.Models;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.Responses;
using RSSMS.DataService.UnitOfWorks;
using RSSMS.DataService.Utilities;
using RSSMS.DataService.ViewModels;
using RSSMS.DataService.ViewModels.JWT;
using RSSMS.DataService.ViewModels.Users;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace RSSMS.DataService.Services
{
    public interface IUserService : IBaseService<User>
    {
        Task<TokenViewModel> Login(UserLoginViewModel model);
        Task<DynamicModelResponse<UserViewModel>> GetAll(UserViewModel model, string[] fields, int page, int size);
        Task<UserViewModel> GetById(int id);
        Task<UserCreateViewModel> Create(UserCreateViewModel model);
        Task<UserViewModel> Update(int id, UserUpdateViewModel model);
        Task<UserViewModel> Delete(int id);
        Task<int> Count(List<UserViewModel> shelves);
  /*      Task<bool> UpdateUserStorageID(UserListStaffViewModel listUser, int storageID);*/
    }
    public class UserService : BaseService<User>, IUserService
    {
        private readonly IMapper _mapper;
        public UserService(IUnitOfWork unitOfWork, IUserRepository repository, IMapper mapper) : base(unitOfWork, repository)
        {
            _mapper = mapper;
        }

        public Task<int> Count(List<UserViewModel> shelves)
        {
            return Count();
        }

        public async Task<UserCreateViewModel> Create(UserCreateViewModel model)
        {
            var user = await Get(x => x.Email == model.Email).FirstOrDefaultAsync();
            if (user != null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Email is existed");
            var userCreate = _mapper.Map<User>(model);
            await CreateAsync(userCreate);
            return model;
        }

        public async Task<UserViewModel> Delete(int id)
        {
            var entity = await Get(x => x.Id == id && x.IsActive == true).FirstOrDefaultAsync();
            if (entity == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "User not found");
            entity.IsActive = false;
            await UpdateAsync(entity);
            return _mapper.Map<UserViewModel>(entity);
        }

        public async Task<DynamicModelResponse<UserViewModel>> GetAll(UserViewModel model, string[] fields, int page, int size)
        {
            var users = Get(x => x.IsActive == true).ProjectTo<UserViewModel>(_mapper.ConfigurationProvider)
                .DynamicFilter(model)
                .PagingIQueryable(page, size, CommonConstant.LimitPaging, CommonConstant.DefaultPaging);
            if (users.Item2.ToList().Count < 1) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Can not found");
            var rs = new DynamicModelResponse<UserViewModel>
            {
                Metadata = new PagingMetaData
                {
                    Page = page,
                    Size = size,
                    Total = users.Item1
                },
                Data = users.Item2.ToList()
            };
            return rs;
        }

        public async Task<UserViewModel> GetById(int id)
        {
            var result = await Get(x => x.Id == id && x.IsActive == true).ProjectTo<UserViewModel>(_mapper.ConfigurationProvider).FirstOrDefaultAsync();
            if (result == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "User id not found");
            return result;
        }

        public async Task<TokenViewModel> Login(UserLoginViewModel model)
        {
            var user = await Get(x => x.Email == model.Email && x.Password == model.Password && x.IsActive == true).Include(x => x.Role).Include(x => x.Images).FirstOrDefaultAsync();
            if (user == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "User not found");
            var result = _mapper.Map<TokenViewModel>(user);
            var token = GenerateToken(user);
            var refreshToken = GenerateRefreshToken(user);
            token.RefreshToken = refreshToken;
            result = _mapper.Map(token, result);
            return result;
        }

        public async Task<UserViewModel> Update(int id, UserUpdateViewModel model)
        {
            if (id != model.Id) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "User Id not matched");

            var entity = await GetAsync(id);
            if (entity == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "User not found");

            var updateEntity = _mapper.Map(model, entity);
            await UpdateAsync(updateEntity);

            return _mapper.Map<UserViewModel>(updateEntity);
        }
        
        private TokenGenerateModel GenerateToken(User user)
        {
            var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim("user_id",user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };
            authClaims.Add(new Claim(ClaimTypes.Role, user.Role.Name));
            string secret = SecretKeyConstant.SECRET_KEY;
            IdentityModelEventSource.ShowPII = true;

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var token = new JwtSecurityToken(
                issuer: SecretKeyConstant.ISSUER,
                audience: SecretKeyConstant.ISSUER,
                expires: DateTime.Now.AddMinutes(60),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );
            TimeSpan expires = DateTime.Now.Subtract(token.ValidTo);
            return new TokenGenerateModel
            {
                IdToken = new JwtSecurityTokenHandler().WriteToken(token),
                ExpiresIn = expires.TotalMinutes,
                TokenType = "Bearer",
            };
        }
        private string GenerateRefreshToken(User user)
        {
            var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim("user_id",user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };
            authClaims.Add(new Claim(ClaimTypes.Role, user.Role.Name));
            string secret = SecretKeyConstant.SECRET_KEY;
            IdentityModelEventSource.ShowPII = true;
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var token = new JwtSecurityToken(
                issuer: SecretKeyConstant.ISSUER,
                audience: SecretKeyConstant.ISSUER,
                expires: DateTime.Now.AddMonths(1),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

/*        public Task<bool> UpdateUserStorageID(UserListStaffViewModel listUser, int storageID)
        {
            return true;
        }*/
    }

}
