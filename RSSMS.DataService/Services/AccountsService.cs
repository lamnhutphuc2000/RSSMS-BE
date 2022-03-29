using AutoMapper;
using AutoMapper.QueryableExtensions;
using Firebase.Auth;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using RSSMS.DataService.Constants;
using RSSMS.DataService.Models;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.Responses;
using RSSMS.DataService.UnitOfWorks;
using RSSMS.DataService.Utilities;
using RSSMS.DataService.ViewModels.Accounts;
using RSSMS.DataService.ViewModels.JWT;
using RSSMS.DataService.ViewModels.StaffAssignStorage;
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
    public interface IAccountsService : IBaseService<Account>
    {
        Task<TokenViewModel> Login(AccountsLoginViewModel model);
        Task<AccountsViewModel> ChangePassword(AccountsChangePasswordViewModel model);
        Task<DynamicModelResponse<AccountsViewModel>> GetAll(AccountsViewModel model, Guid? storageId, Guid? orderId, string[] fields, int page, int size, string accessToken);
        Task<AccountsViewModel> GetById(Guid id);
        Task<AccountsViewModel> GetByPhone(string phone);
        Task<TokenViewModel> Create(AccountsCreateViewModel model);
        Task<AccountsViewModel> Update(Guid id, AccountsUpdateViewModel model);
        Task<AccountsViewModel> Delete(Guid id);
        Task<TokenViewModel> CheckLogin(string firebaseID, string deviceToken);
        Task<List<AccountsViewModel>> GetStaff(Guid? storageId, string accessToken, List<string> roleName, DateTime? scheduleDay, ICollection<string> deliveryTimes);
    }
    public class AccountsService : BaseService<Account>, IAccountsService
    {
        private readonly IMapper _mapper;
        private readonly IStaffAssignStoragesService _staffAssignStoragesService;
        private readonly IFirebaseService _firebaseService;
        private readonly IScheduleService _scheduleService;
        private readonly static string apiKEY = "AIzaSyCbxMnxwCfJgCJtvaBeRdvvZ3y1Ucuyv2s";
        public AccountsService(IUnitOfWork unitOfWork, IAccountsRepository repository, IMapper mapper, IStaffAssignStoragesService staffAssignStoragesService, IFirebaseService firebaseService, IScheduleService scheduleService) : base(unitOfWork, repository)
        {
            _mapper = mapper;
            _staffAssignStoragesService = staffAssignStoragesService;
            _firebaseService = firebaseService;
            _scheduleService = scheduleService;
        }


        public async Task<TokenViewModel> Login(AccountsLoginViewModel model)
        {
            Firebase.Auth.User us = null;
            try
            {
                var auth = new FirebaseAuthProvider(new FirebaseConfig(apiKEY));
                var a = await auth.SignInWithEmailAndPasswordAsync(model.Email, model.Password);

                string tok = a.FirebaseToken;
                us = a.User;

            }
            catch (Exception ex)
            {
                throw new ErrorResponse((int)HttpStatusCode.NotFound, ex.Message);
            }

            var acc = await Get(x => x.Email == model.Email && us.LocalId == x.FirebaseId && x.Password == model.Password && x.IsActive == true).Include(x => x.Role).Include(x => x.StaffAssignStorages).FirstOrDefaultAsync();
            if (acc == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "User not found");
            var result = _mapper.Map<TokenViewModel>(acc);
            var token = GenerateToken(acc);
            var refreshToken = GenerateRefreshToken(acc);
            token.RefreshToken = refreshToken;
            result = _mapper.Map(token, result);
            result.StorageId = null;
            var storageId = acc.StaffAssignStorages.Where(x => x.IsActive == true).FirstOrDefault()?.StorageId;
            if (storageId != null && acc.Role.Name == "Office Staff")
            {
                result.StorageId = storageId;
            }
            acc.DeviceTokenId = model.DeviceToken;
            await UpdateAsync(acc);
            return result;
        }

        public async Task<AccountsViewModel> ChangePassword(AccountsChangePasswordViewModel model)
        {
            if (!model.ConfirmPassword.Equals(model.Password)) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Confirm password not matched");
            var user = await Get(x => x.Id == model.Id && x.IsActive == true).FirstOrDefaultAsync();
            if (user == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "User not found");
            if (user.Password != model.OldPassword) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Password not matched");

            if (FirebaseApp.DefaultInstance == null)
            {
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile("privatekey.json"),
                });
            }

            if (user.FirebaseId == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "FirebaseID null");
            var firebaseUser = await FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance.GetUserAsync(user.FirebaseId);
            UserRecordArgs args = new UserRecordArgs()
            {
                Uid = firebaseUser.Uid,
                PhoneNumber = firebaseUser.PhoneNumber,
                Password = model.Password
            };
            UserRecord userRecordUpdate = await FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance.UpdateUserAsync(args);

            user.Password = model.Password;
            await UpdateAsync(user);
            return _mapper.Map<AccountsViewModel>(user);
        }

        public async Task<DynamicModelResponse<AccountsViewModel>> GetAll(AccountsViewModel model, Guid? storageId, Guid? orderId, string[] fields, int page, int size, string accessToken)
        {
            var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
            var uid = secureToken.Claims.First(claim => claim.Type == "user_id").Value;
            var role = secureToken.Claims.First(claim => claim.Type.Contains("role")).Value;

            var user = await Get(x => x.Id == Guid.Parse(uid)).Include(x => x.Role).FirstOrDefaultAsync();

            var users = Get(x => x.IsActive == true && !x.Role.Name.Equals("Admin")).Include(x => x.Role);
            if (user.Role.Name == "Manager")
            {
                var storageIds = _staffAssignStoragesService.Get(x => x.StaffId == user.Id && x.IsActive == true).Select(x => x.StorageId).ToList();
                users = users.Where(x => x.Role.Name != "Manager").Include(x => x.Role);
                users = users.Where(x => x.StaffAssignStorages.Any(x => storageIds.Contains((Guid)x.StorageId)) || x.StaffAssignStorages.Count == 0)
                    .Include(x => x.Role);
            }
            if (role == "Office Staff")
                users = users.Where(x => x.Role.Name == "Customer").Include(x => x.Role);


            var result = users.ProjectTo<AccountsViewModel>(_mapper.ConfigurationProvider)
                .DynamicFilter(model).PagingIQueryable(page, size, CommonConstant.LimitPaging, CommonConstant.DefaultPaging);
            if (result.Item2.ToList().Count < 1) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Can not found");
            var rs = new DynamicModelResponse<AccountsViewModel>
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

        public async Task<AccountsViewModel> GetById(Guid id)
        {
            var result = await Get(x => x.Id == id && x.IsActive == true).ProjectTo<AccountsViewModel>(_mapper.ConfigurationProvider).FirstOrDefaultAsync();
            if (result == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "User id not found");
            return result;
        }

        public async Task<AccountsViewModel> GetByPhone(string phone)
        {
            var result = await Get(x => x.Phone == phone && x.IsActive == true).ProjectTo<AccountsViewModel>(_mapper.ConfigurationProvider).FirstOrDefaultAsync();
            if (result == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "User phone not found");
            return result;
        }

        public async Task<TokenViewModel> Create(AccountsCreateViewModel model)
        {
            var autho = new FirebaseAuthProvider(new FirebaseConfig(apiKEY));
            Firebase.Auth.User us = null;
            try
            {
                var a = await autho.CreateUserWithEmailAndPasswordAsync(model.Email, model.Password, model.Name, false);
                us = a.User;
            }
            catch (Exception e)
            {
                throw new ErrorResponse((int)HttpStatusCode.BadRequest, e.Message);
            }
            var user = await Get(x => x.Email == model.Email).FirstOrDefaultAsync();
            if (user != null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Email is existed");

            // Create user
            var userCreate = _mapper.Map<Account>(model);
            var image = model.Image;
            userCreate.ImageUrl = null;
            userCreate.FirebaseId = us.LocalId;
            userCreate.DeviceTokenId = model.DeviceToken;
            await CreateAsync(userCreate);

            // Upload image to firebase
            if (image != null)
            {
                var url = await _firebaseService.UploadImageToFirebase(image.File, "users", userCreate.Id, "avatar");
                if (url != null)
                {
                    userCreate.ImageUrl = url;
                }
            }


            // Assign user to storages
            if (model.StorageIds != null)
            {
                for (int i = 0; i < model.StorageIds.Count; i++)
                {
                    StaffAssignStorageCreateViewModel staffAssignModel = new StaffAssignStorageCreateViewModel
                    {
                        UserId = userCreate.Id,
                        StorageId = model.StorageIds.ElementAt(i),
                        RoleName = userCreate.Role.Name
                    };
                    await _staffAssignStoragesService.Create(staffAssignModel);
                }
            }
            var newUser = await Get(x => x.Email == model.Email && us.LocalId == x.FirebaseId && x.Password == model.Password && x.IsActive == true).Include(x => x.Role).Include(x => x.StaffAssignStorages).FirstOrDefaultAsync();
            var result = _mapper.Map<TokenViewModel>(newUser);
            var token = GenerateToken(newUser);
            var refreshToken = GenerateRefreshToken(newUser);
            token.RefreshToken = refreshToken;
            result = _mapper.Map(token, result);
            result.StorageId = null;
            var storageId = userCreate.StaffAssignStorages.Where(x => x.IsActive == true).FirstOrDefault()?.StorageId;
            if (storageId != null && userCreate.Role.Name == "Office Staff")
            {
                result.StorageId = storageId;
            }
            userCreate.DeviceTokenId = model.DeviceToken;
            await UpdateAsync(userCreate);
            return result;
        }

        public async Task<AccountsViewModel> Update(Guid id, AccountsUpdateViewModel model)
        {
            if (id != model.Id) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "User Id not matched");

            var entity = await Get(x => x.Id == id && x.IsActive == true).FirstOrDefaultAsync();
            if (entity == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "User not found");

            var image = model.Image;

            string url = entity.ImageUrl;
            var updateEntity = _mapper.Map(model, entity);
            updateEntity.ImageUrl = url;
            if (image != null)
            {
                url = await _firebaseService.UploadImageToFirebase(image.File, "users", id, "avatar");
                if (url != null)
                {
                    updateEntity.ImageUrl = url;
                }
            }

            await UpdateAsync(updateEntity);

            return _mapper.Map<AccountsViewModel>(updateEntity);
        }

        public async Task<AccountsViewModel> Delete(Guid id)
        {
            var entity = await Get(x => x.Id == id && x.IsActive == true).FirstOrDefaultAsync();
            if (entity == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "User not found");
            entity.IsActive = false;
            await UpdateAsync(entity);
            return _mapper.Map<AccountsViewModel>(entity);
        }

        public async Task<TokenViewModel> CheckLogin(string firebaseID, string deviceToken)
        {
            if (FirebaseApp.DefaultInstance == null)
            {
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile("privatekey.json"),
                });
            }

            if (firebaseID == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "FirebaseID null");
            var checkFirebase = await FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance.GetUserAsync(firebaseID);

            if (checkFirebase == null)
            {
                throw new ErrorResponse((int)HttpStatusCode.NotFound, "FirebaseID not found");
            }
            else
            {
                var checkLocalID = await Get(x => x.FirebaseId == firebaseID).FirstOrDefaultAsync();

                if (checkLocalID == null)
                {
                    AccountsCreateThirdPartyViewModel model = new AccountsCreateThirdPartyViewModel(checkFirebase.DisplayName,
                        checkFirebase.PhotoUrl, checkFirebase.PhoneNumber, deviceToken);
                    await CreateWithoutFirebase(model, firebaseID);
                    await LoginWithFirebaseID(firebaseID);
                }
                else
                {
                    await LoginWithFirebaseID(firebaseID);
                }
            }
            return await LoginWithFirebaseID(firebaseID);
        }

        public async Task<TokenViewModel> LoginWithFirebaseID(string firebaseID)
        {
            var acc = await Get(x => x.FirebaseId == firebaseID && x.IsActive == true).Include(x => x.Role).Include(x => x.StaffAssignStorages).FirstOrDefaultAsync();
            if (acc == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "User not found");
            var result = _mapper.Map<TokenViewModel>(acc);
            var token = GenerateToken(acc);
            var refreshToken = GenerateRefreshToken(acc);
            token.RefreshToken = refreshToken;
            result = _mapper.Map(token, result);
            result.StorageId = null;
            var storageId = acc.StaffAssignStorages.Where(x => x.IsActive == true).FirstOrDefault()?.StorageId;
            if (storageId != null && acc.Role.Name == "Office Staff")
            {
                result.StorageId = storageId;
            }
            await UpdateAsync(acc);
            return result;
        }

        public async Task<AccountsViewModel> CreateWithoutFirebase(AccountsCreateThirdPartyViewModel model, string fireBaseID)
        {
            var userCreate = _mapper.Map<Account>(model);
            userCreate.DeviceTokenId = model.DeviceToken;
            userCreate.IsActive = true;
            userCreate.FirebaseId = fireBaseID;
            //userCreate.RoleId = 3;
            await CreateAsync(userCreate);
            return await GetById(userCreate.Id);
        }


        private TokenGenerateViewModel GenerateToken(Account acc)
        {
            var storageId = acc.StaffAssignStorages.Where(x => x.IsActive == true).FirstOrDefault()?.StorageId.ToString();
            if (storageId == null) storageId = "-1";
            var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, acc.Name),
                    new Claim("user_id",acc.Id.ToString()),
                    new Claim("storage_id",storageId),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };
            authClaims.Add(new Claim(ClaimTypes.Role, acc.Role.Name));
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
            return new TokenGenerateViewModel
            {
                IdToken = new JwtSecurityTokenHandler().WriteToken(token),
                ExpiresIn = expires.TotalMinutes,
                TokenType = "Bearer",
            };
        }
        private string GenerateRefreshToken(Account acc)
        {
            var storageId = acc.StaffAssignStorages.Where(x => x.IsActive == true).FirstOrDefault()?.StorageId.ToString();
            if (storageId == null) storageId = "-1";
            var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, acc.Name),
                    new Claim("user_id",acc.Id.ToString()),
                    new Claim("storage_id",storageId),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };
            authClaims.Add(new Claim(ClaimTypes.Role, acc.Role.Name));
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

        public async Task<List<AccountsViewModel>> GetStaff(Guid? storageId, string accessToken, List<string> roleName, DateTime? scheduleDay, ICollection<string> deliveryTimes)
        {
            var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
            var uid = secureToken.Claims.First(claim => claim.Type == "user_id").Value;
            var role = secureToken.Claims.First(claim => claim.Type.Contains("role")).Value;


            var staffs = Get(x => x.IsActive == true && x.Role.Name != "Admin" && x.Role.Name != "Customer").Include(x => x.StaffAssignStorages).Include(x => x.Schedules);
            if (roleName.Count > 0) staffs = Get(x => x.IsActive == true && roleName.Contains(x.Role.Name)).Include(x => x.StaffAssignStorages).Include(x => x.Schedules);
            if (role == "Manager")
                staffs = staffs.Where(x => x.Role.Name != "Manager").Include(x => x.StaffAssignStorages).Include(x => x.Schedules);

            if (storageId == null)
                staffs = staffs.Where(x => x.StaffAssignStorages.Where(staffAssignStorage => staffAssignStorage.IsActive == true).Count() == 0).Include(x => x.StaffAssignStorages).Include(x => x.Schedules);

            if (storageId != null)
                staffs = staffs.Where(x => x.StaffAssignStorages.Any(staffAssignStorage => staffAssignStorage.StorageId == storageId && staffAssignStorage.IsActive == true)).Include(x => x.StaffAssignStorages).Include(x => x.Schedules);

            if (scheduleDay != null)
            {
                var usersInDelivery = _scheduleService.Get(x => scheduleDay.Value.Date == x.ScheduleDay.Date && deliveryTimes.Contains(x.ScheduleTime) && x.IsActive == true).Select(x => x.UserId).Distinct().ToList();
                staffs = staffs.Where(x => !usersInDelivery.Contains(x.Id)).Include(x => x.StaffAssignStorages).Include(x => x.Schedules);
            }
            var result = await staffs.ProjectTo<AccountsViewModel>(_mapper.ConfigurationProvider).ToListAsync();
            return result;
        }
    }
}
