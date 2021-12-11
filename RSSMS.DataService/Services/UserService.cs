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
using RSSMS.DataService.ViewModels.JWT;
using RSSMS.DataService.ViewModels.StaffManageUser;
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
        Task<UserViewModel> ChangePassword(UserChangePasswordViewModel model);
        Task<DynamicModelResponse<UserViewModel>> GetAll(UserViewModel model, int? storageId, int? orderId, string[] fields, int page, int size, string accessToken);
        Task<UserViewModel> GetById(int id);
        Task<UserViewModel> GetByPhone(string phone);
        Task<UserViewModel> Create(UserCreateViewModel model);
        Task<UserViewModel> Update(int id, UserUpdateViewModel model);
        Task<UserViewModel> Delete(int id);
        Task<int> Count(List<UserViewModel> shelves);
    }
    public class UserService : BaseService<User>, IUserService
    {
        private readonly IMapper _mapper;
        private readonly IStaffManageStorageService _staffManageStorageService;
        private readonly IOrderService _orderService;
        public UserService(IUnitOfWork unitOfWork, IUserRepository repository, IMapper mapper, IStaffManageStorageService staffManageStorageService, IOrderService orderService) : base(unitOfWork, repository)
        {
            _mapper = mapper;
            _staffManageStorageService = staffManageStorageService;
            _orderService = orderService;
        }

        public Task<int> Count(List<UserViewModel> shelves)
        {
            return Count();
        }

        public async Task<UserViewModel> Create(UserCreateViewModel model)
        {
            var user = await Get(x => x.Email == model.Email).FirstOrDefaultAsync();
            if (user != null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Email is existed");
            var userCreate = _mapper.Map<User>(model);
            await CreateAsync(userCreate);
            if (model.StorageIds != null)
            {
                for (int i = 0; i < model.StorageIds.Count; i++)
                {
                    var staffAssignModel = _mapper.Map<StaffManageStorageCreateViewModel>(userCreate);
                    staffAssignModel.StorageId = model.StorageIds.ElementAt(i);
                    await _staffManageStorageService.Create(staffAssignModel);
                }
            }
            return await GetById(userCreate.Id);
        }

        public async Task<UserViewModel> Delete(int id)
        {
            var entity = await Get(x => x.Id == id && x.IsActive == true).FirstOrDefaultAsync();
            if (entity == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "User not found");
            entity.IsActive = false;
            await UpdateAsync(entity);
            return _mapper.Map<UserViewModel>(entity);
        }

        public async Task<DynamicModelResponse<UserViewModel>> GetAll(UserViewModel model, int? storageId, int? orderId, string[] fields, int page, int size, string accessToken)
        {
            var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
            var uid = secureToken.Claims.First(claim => claim.Type == "user_id").Value;

            var user = Get(x => x.Id == Int32.Parse(uid)).Include(x => x.Role).FirstOrDefault();

            var users = Get(x => x.IsActive == true && !x.Role.Name.Equals("Admin")).ProjectTo<UserViewModel>(_mapper.ConfigurationProvider)
                .DynamicFilter(model);
            if (storageId == 0)
            {
                var staff = Get(x => x.IsActive == true && !x.Role.Name.Equals("Admin") && !x.Role.Name.Equals("Customer") && !x.Role.Name.Equals("Manager") && x.StaffManageStorages.Count == 0);
                var manager = Get(x => x.IsActive == true && x.Role.Name.Equals("Manager"));
                users = staff.Union(manager).ProjectTo<UserViewModel>(_mapper.ConfigurationProvider)
                .DynamicFilter(model);
            }
            if (storageId != null && storageId != 0)
            {
                users = Get(x => x.IsActive == true && !x.Role.Name.Equals("Admin") && !x.Role.Name.Equals("Customer"))
                    .Where(x => x.StaffManageStorages.Any(a => a.StorageId == storageId)).ProjectTo<UserViewModel>(_mapper.ConfigurationProvider)
                    .DynamicFilter(model);
            }
            if (orderId != null && orderId != 0)
            {
                var order = _orderService.Get(a => a.Id == orderId).FirstOrDefault();
                var deliveryTime = order.DeliveryTime;
                var deliveryDate = order.DeliveryDate;
                users = Get(x => x.IsActive == true && !x.Role.Name.Equals("Admin") && !x.Role.Name.Equals("Customer"))
                    .Where(x => !x.Schedules.Any(a => a.OrderId == orderId && a.IsActive == true) &&( x.Schedules.Count == 0 || !x.Schedules.Any(a => a.OrderId != orderId && a.IsActive == true && a.DeliveryTime == deliveryTime && a.SheduleDay == deliveryDate))).ProjectTo<UserViewModel>(_mapper.ConfigurationProvider)
                    .DynamicFilter(model);
            }
            if (user.Role.Name == "Manager")
            {
                var storageIds = _staffManageStorageService.Get(x => x.UserId == user.Id).Select(x => x.StorageId).ToList();
                users = users.Where(x => x.StaffManageStorages.Any(x => storageIds.Contains((int)x.StorageId)) || x.StaffManageStorages.Count == 0 || x.RoleName.Equals("Manager"));
            }
            var result = users.PagingIQueryable(page, size, CommonConstant.LimitPaging, CommonConstant.DefaultPaging);
            if (result.Item2.ToList().Count < 1) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Can not found");
            var rs = new DynamicModelResponse<UserViewModel>
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

        public async Task<UserViewModel> GetById(int id)
        {
            var result = await Get(x => x.Id == id && x.IsActive == true).ProjectTo<UserViewModel>(_mapper.ConfigurationProvider).FirstOrDefaultAsync();
            if (result == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "User id not found");
            return result;
        }

        public async Task<TokenViewModel> Login(UserLoginViewModel model)
        {
            var user = await Get(x => x.Email == model.Email && x.Password == model.Password && x.IsActive == true).Include(x => x.Role).Include(x => x.Images).Include(x => x.StaffManageStorages).FirstOrDefaultAsync();
            if (user == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "User not found");
            var result = _mapper.Map<TokenViewModel>(user);
            var token = GenerateToken(user);
            var refreshToken = GenerateRefreshToken(user);
            token.RefreshToken = refreshToken;
            result = _mapper.Map(token, result);
            result.StorageId = null;
            var storageId = user.StaffManageStorages.FirstOrDefault()?.StorageId;
            if (storageId != null && user.Role.Name == "Office staff")
            {
                result.StorageId = storageId;
            }
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
            var storageId = user.StaffManageStorages.FirstOrDefault()?.StorageId.ToString();
            if (storageId == null) storageId = "-1";
            var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim("user_id",user.Id.ToString()),
                    new Claim("storage_id",storageId),
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
            var storageId = user.StaffManageStorages.FirstOrDefault()?.StorageId.ToString();
            if (storageId == null) storageId = "-1";
            var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim("user_id",user.Id.ToString()),
                    new Claim("storage_id",storageId),
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

        public async Task<UserViewModel> ChangePassword(UserChangePasswordViewModel model)
        {
            if (!model.ConfirmPassword.Equals(model.Password)) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Confirm password not matched");
            var user = await Get(x => x.Id == model.Id && x.IsActive == true).FirstOrDefaultAsync();
            if (user == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "User not found");
            user.Password = model.Password;
            await UpdateAsync(user);
            return _mapper.Map<UserViewModel>(user);
        }

        public async Task<UserViewModel> GetByPhone(string phone)
        {
            var result = await Get(x => x.Phone == phone && x.IsActive == true).ProjectTo<UserViewModel>(_mapper.ConfigurationProvider).FirstOrDefaultAsync();
            if (result == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "User id not found");
            return result;
        }
    }

}
