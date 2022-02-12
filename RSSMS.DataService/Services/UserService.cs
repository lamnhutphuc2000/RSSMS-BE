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
    public interface IUserService : IBaseService<Models.User>
    {
        Task<TokenViewModel> Login(UserLoginViewModel model);
        Task<UserViewModel> ChangePassword(UserChangePasswordViewModel model);
        Task<DynamicModelResponse<UserViewModel>> GetAll(UserViewModel model, int? storageId, int? orderId, string[] fields, int page, int size, string accessToken);
        Task<UserViewModel> GetById(int id);
        Task<UserViewModel> GetByPhone(string phone);
        Task<TokenViewModel> Create(UserCreateViewModel model);
        Task<UserViewModel> Update(int id, UserUpdateViewModel model);
        Task<UserViewModel> Delete(int id, string firebaseID);
        Task<int> Count(List<UserViewModel> shelves);
        Task<TokenViewModel> CheckLogin(string firebaseID, string deviceToken);
        Task<TokenViewModel> LoginWithFirebaseID(string firebaseID);
        Task<UserViewModel> CreateWithoutFirebase(UserCreateThirdPartyViewModel model, string fireBaseID);
    }
    public class UserService : BaseService<Models.User>, IUserService
    {
        private readonly IMapper _mapper;
        private readonly IStaffManageStorageService _staffManageStorageService;
        private readonly IOrderService _orderService;
        private readonly IScheduleService _scheduleService;
        private readonly IFirebaseService _firebaseService;
        private static string apiKEY = "AIzaSyCbxMnxwCfJgCJtvaBeRdvvZ3y1Ucuyv2s";
        private static string Bucket = "rssms-5fcc8.appspot.com";
        public UserService(IUnitOfWork unitOfWork, IUserRepository repository, IMapper mapper, IStaffManageStorageService staffManageStorageService, IOrderService orderService, IScheduleService scheduleService, IFirebaseService firebaseService) : base(unitOfWork, repository)
        {
            _mapper = mapper;
            _staffManageStorageService = staffManageStorageService;
            _orderService = orderService;
            _scheduleService = scheduleService;
            _firebaseService = firebaseService;
        }

        public Task<int> Count(List<UserViewModel> shelves)
        {
            return Count();
        }

        public async Task<TokenViewModel> Create(UserCreateViewModel model)
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
            var userCreate = _mapper.Map<Models.User>(model);
            var images = model.Images;
            userCreate.Images = null;
            userCreate.FirebaseId = us.LocalId;
            userCreate.DeviceTokenId = model.DeviceToken;
            await CreateAsync(userCreate);

            // Upload image to firebase
            if (images != null)
            {
                foreach (var avatar in images)
                {
                    var url = await _firebaseService.UploadImageToFirebase(avatar.File, "users", userCreate.Id, "avatar");
                    if (url != null) avatar.Url = url;
                }
                userCreate.Images = images.AsQueryable().ProjectTo<Image>(_mapper.ConfigurationProvider).ToList();
            }


            // Assign user to storages
            if (model.StorageIds != null)
            {
                for (int i = 0; i < model.StorageIds.Count; i++)
                {
                    var staffAssignModel = _mapper.Map<StaffManageStorageCreateViewModel>(userCreate);
                    staffAssignModel.StorageId = model.StorageIds.ElementAt(i);
                    await _staffManageStorageService.Create(staffAssignModel);
                }
            }
            var newUser = await Get(x => x.Email == model.Email && us.LocalId == x.FirebaseId && x.Password == model.Password && x.IsActive == true).Include(x => x.Role).Include(x => x.Images).Include(x => x.StaffManageStorages).FirstOrDefaultAsync();
            var result = _mapper.Map<TokenViewModel>(newUser);
            var token = GenerateToken(newUser);
            var refreshToken = GenerateRefreshToken(newUser);
            token.RefreshToken = refreshToken;
            result = _mapper.Map(token, result);
            result.StorageId = null;
            var storageId = userCreate.StaffManageStorages.FirstOrDefault()?.StorageId;
            if (storageId != null && userCreate.Role.Name == "Office staff")
            {
                result.StorageId = storageId;
            }
            userCreate.DeviceTokenId = model.DeviceToken;
            await UpdateAsync(userCreate);
            return result;
        }

        public async Task<UserViewModel> CreateWithoutFirebase(UserCreateThirdPartyViewModel model, string fireBaseID)
        {
            var userCreate = _mapper.Map<Models.User>(model);
            userCreate.DeviceTokenId = model.DeviceToken;
            userCreate.IsActive = true;
            userCreate.FirebaseId = fireBaseID;
            userCreate.RoleId = 3;
            await CreateAsync(userCreate);
            return await GetById(userCreate.Id);
        }

        public async Task<UserViewModel> Delete(int id, string firebaseID)
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
            DateTime? scheduleDay = null;
            ICollection<string> deliveryTimes = null;
            if (model.SheduleDay != null && model.DeliveryTimes != null)
            {
                scheduleDay = model.SheduleDay;
                deliveryTimes = model.DeliveryTimes;
            }
            model.SheduleDay = null;
            model.DeliveryTimes = null;
            var users = Get(x => x.IsActive == true && !x.Role.Name.Equals("Admin")).ProjectTo<UserViewModel>(_mapper.ConfigurationProvider)
                .DynamicFilter(model);
            if (storageId == 0)
            {
                var staff = Get(x => x.IsActive == true && !x.Role.Name.Equals("Admin") && !x.Role.Name.Equals("Customer") && !x.Role.Name.Equals("Manager") && x.StaffManageStorages.Count == 0);
                users = staff.ProjectTo<UserViewModel>(_mapper.ConfigurationProvider)
                    .DynamicFilter(model);
                if (user.Role.Name == "Admin")
                {
                    var manager = Get(x => x.IsActive == true && x.Role.Name.Equals("Manager"));
                    users = staff.Union(manager).ProjectTo<UserViewModel>(_mapper.ConfigurationProvider)
                    .DynamicFilter(model);
                }

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
                var returnDate = order.ReturnDate;
                var returnTime = order.ReturnTime;
                users = Get(x => x.IsActive == true && !x.Role.Name.Equals("Admin") && !x.Role.Name.Equals("Customer"))
                    .Where(x => x.Schedules.Count == 0 || (!x.Schedules.Any(a => a.OrderId == orderId && a.IsActive == true) && (!x.Schedules.Any(a => a.OrderId != orderId && a.IsActive == true && a.DeliveryTime == deliveryTime && a.SheduleDay == deliveryDate) && (!x.Schedules.Any(a => a.OrderId != orderId && a.IsActive == true && a.DeliveryTime == returnTime && a.SheduleDay == returnDate))))).ProjectTo<UserViewModel>(_mapper.ConfigurationProvider)
                    .DynamicFilter(model);
            }
            if (scheduleDay != null && deliveryTimes != null)
            {
                var usersInDelivery = _scheduleService.Get(x => scheduleDay.Value.Date == x.SheduleDay.Value.Date && deliveryTimes.Contains(x.DeliveryTime) && x.IsActive == true).Select(schedule => schedule.UserId).Distinct().ToList();
                users = users.Where(x => !usersInDelivery.Contains(x.Id));
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
            Firebase.Auth.User us = null;
            try
            {
                var auth = new FirebaseAuthProvider(new FirebaseConfig(apiKEY));
                var a = await auth.SignInWithEmailAndPasswordAsync(model.Email, model.Password);

                string tok = a.FirebaseToken;
                us = a.User;

                //var info = auth.GetUserAsync(tok);
            }
            catch (Exception ex)
            {
                throw new ErrorResponse((int)HttpStatusCode.NotFound, "User not found");
            }

            var acc = await Get(x => x.Email == model.Email && us.LocalId == x.FirebaseId && x.Password == model.Password && x.IsActive == true).Include(x => x.Role).Include(x => x.Images).Include(x => x.StaffManageStorages).FirstOrDefaultAsync();
            if (acc == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "User not found");
            var result = _mapper.Map<TokenViewModel>(acc);
            var token = GenerateToken(acc);
            var refreshToken = GenerateRefreshToken(acc);
            token.RefreshToken = refreshToken;
            result = _mapper.Map(token, result);
            result.StorageId = null;
            var storageId = acc.StaffManageStorages.FirstOrDefault()?.StorageId;
            if (storageId != null && acc.Role.Name == "Office staff")
            {
                result.StorageId = storageId;
            }
            acc.DeviceTokenId = model.DeviceToken;
            await UpdateAsync(acc);
            return result;
        }
        public async Task<TokenViewModel> LoginWithFirebaseID(string firebaseID)
        {
            var acc = await Get(x => x.FirebaseId == firebaseID && x.IsActive == true).Include(x => x.Role).Include(x => x.Images).Include(x => x.StaffManageStorages).FirstOrDefaultAsync();
            //  if (acc == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "User not found");
            var result = _mapper.Map<TokenViewModel>(acc);
            var token = GenerateToken(acc);
            var refreshToken = GenerateRefreshToken(acc);
            token.RefreshToken = refreshToken;
            result = _mapper.Map(token, result);
            result.StorageId = null;
            var storageId = acc.StaffManageStorages.FirstOrDefault()?.StorageId;
            if (storageId != null && acc.Role.Name == "Office staff")
            {
                result.StorageId = storageId;
            }
            await UpdateAsync(acc);
            return result;
        }


        public async Task<UserViewModel> Update(int id, UserUpdateViewModel model)
        {
            if (id != model.Id) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "User Id not matched");

            var entity = await Get(x => x.Id == id && x.IsActive == true).FirstOrDefaultAsync();
            if (entity == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "User not found");

            var images = model.Images;
            foreach (var avatar in images)
            {
                var url = await _firebaseService.UploadImageToFirebase(avatar.File, "users", id, "avatar");
                if (url != null) avatar.Url = url;
            }
            model.Images = images;
            var updateEntity = _mapper.Map(model, entity);
            await UpdateAsync(updateEntity);

            return _mapper.Map<UserViewModel>(updateEntity);
        }

        private TokenGenerateModel GenerateToken(Models.User user)
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
        private string GenerateRefreshToken(Models.User user)
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
            return _mapper.Map<UserViewModel>(user);
        }

        public async Task<UserViewModel> GetByPhone(string phone)
        {
            var result = await Get(x => x.Phone == phone && x.IsActive == true).ProjectTo<UserViewModel>(_mapper.ConfigurationProvider).FirstOrDefaultAsync();
            if (result == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "User id not found");
            return result;
        }


        public async Task<TokenViewModel> CheckLogin(string firebaseID, string deviceToken)
        {
            // var convert = _mapper.Map<UserLoginViewModel>(model);
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
                    UserCreateThirdPartyViewModel model = new UserCreateThirdPartyViewModel(checkFirebase.DisplayName,
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
    }

}
