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
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RSSMS.DataService.Services
{
    public interface IAccountService : IBaseService<Account>
    {
        Task<TokenViewModel> Login(AccountLoginViewModel model);
        Task<AccountViewModel> ChangePassword(AccountChangePasswordViewModel model);
        Task<DynamicModelResponse<AccountViewModel>> GetAll(AccountViewModel model, Guid? storageId, string[] fields, int page, int size, string accessToken);
        Task<AccountViewModel> GetById(Guid id);
        Task<AccountViewModel> GetByPhone(string phone);
        Task<TokenViewModel> Create(AccountCreateViewModel model);
        Task<AccountViewModel> Update(Guid id, AccountUpdateViewModel model);
        Task<AccountViewModel> Delete(Guid id);
        Task<List<AccountViewModel>> GetStaff(Guid? storageId, string accessToken, List<string> roleName, DateTime? scheduleDay, ICollection<string> deliveryTimes, bool getFromAllStorage);
    }
    public class AccountService : BaseService<Account>, IAccountService
    {
        private readonly IMapper _mapper;
        private readonly IFirebaseService _firebaseService;
        private readonly IScheduleService _scheduleService;
        private readonly IRoleService _roleService;
        private readonly IUtilService _utilService;
        public AccountService(IUnitOfWork unitOfWork, IAccountRepository repository, IMapper mapper 
            , IFirebaseService firebaseService, IScheduleService scheduleService, IRoleService roleService
            , IUtilService utilService) : base(unitOfWork, repository)
        {
            _mapper = mapper;
            _firebaseService = firebaseService;
            _scheduleService = scheduleService;
            _roleService = roleService;
            _utilService = utilService;
        }


        public async Task<TokenViewModel> Login(AccountLoginViewModel model)
        {
            try
            {
                // Validate input
                if(!_utilService.ValidateEmail(model.Email)) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Vui lòng nhập đúng email");
                _utilService.ValidatePassword(model.Password);

                // Check account in firebase
                User us = null;
                try
                {
                    FirebaseAuthProvider auth = new FirebaseAuthProvider(new FirebaseConfig(FirebaseKeyConstant.apiKEY));
                    FirebaseAuthLink a = await auth.SignInWithEmailAndPasswordAsync(model.Email, EncryptedPassword(model.Password).ToString());
                    string tok = a.FirebaseToken;
                    us = a.User;
                }
                catch (Exception ex)
                {
                    throw new ErrorResponse((int)HttpStatusCode.NotFound, ex.Message);
                }

                // Get account in database
                Account acc = await Get(account => account.Email == model.Email && us.LocalId == account.FirebaseId && account.Password.SequenceEqual(EncryptedPassword(model.Password))  && account.IsActive)
                    .Include(account => account.Role).Include(account => account.StaffAssignStorages).FirstOrDefaultAsync();
                if (acc == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Email or password not found");
                TokenViewModel result = _mapper.Map<TokenViewModel>(acc);
                
                // Generate Token to return
                TokenGenerateViewModel token = GenerateToken(acc);
                string refreshToken = GenerateRefreshToken(acc);
                token.RefreshToken = refreshToken;
                result = _mapper.Map(token, result);
                result.StorageId = null;
                
                // Update user Device token
                acc.DeviceTokenId = model.DeviceToken;
                await UpdateAsync(acc);

                // Check user role
                if (acc.Role.Name != "Office Staff") return result;

                // Get office staff storage Id
                Guid? storageId = acc.StaffAssignStorages.Where(staffAssignStorage => staffAssignStorage.IsActive == true).FirstOrDefault()?.StorageId;
                if (storageId != null) result.StorageId = storageId;

                return result;
            }
            catch(ErrorResponse e)
            {
                throw new ErrorResponse((int)e.Error.Code, e.Error.Message);
            }
            catch(Exception ex)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<AccountViewModel> ChangePassword(AccountChangePasswordViewModel model)
        {
            try
            {
                // Validate input
                _utilService.ValidatePassword(model.Password);
                _utilService.ValidatePassword(model.ConfirmPassword);
                _utilService.ValidatePassword(model.OldPassword);
                if (!model.ConfirmPassword.Equals(model.Password)) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Mật khẩu không khớp");

                Account account = await Get(account => account.Id == model.Id).FirstOrDefaultAsync();
                if(account == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "User not found");

                byte[] OldPassword = EncryptedPassword(model.OldPassword);
                if (!account.Password.SequenceEqual(OldPassword)) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Wrong old password");
                if (account.FirebaseId == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "FirebaseID null");


                // Config firebaseApp
                if (FirebaseApp.DefaultInstance == null)
                    FirebaseApp.Create(new AppOptions()
                    {
                        Credential = GoogleCredential.FromFile("privatekey.json"),
                    });

                UserRecord firebaseUser = await FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance.GetUserAsync(account.FirebaseId);
                UserRecordArgs args = new UserRecordArgs()
                {
                    Uid = firebaseUser.Uid,
                    PhoneNumber = firebaseUser.PhoneNumber,
                    Password = EncryptedPassword(model.Password).ToString()
                };

                UserRecord userRecordUpdate = await FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance.UpdateUserAsync(args);

                //Update account password
                account.Password = EncryptedPassword(model.Password);
                await UpdateAsync(account);
                return _mapper.Map<AccountViewModel>(account);
            }
            catch (ErrorResponse e)
            {
                throw new ErrorResponse((int)e.Error.Code, e.Error.Message);
            }
            catch (Exception ex)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<DynamicModelResponse<AccountViewModel>> GetAll(AccountViewModel model, Guid? storageId, string[] fields, int page, int size, string accessToken)
        {
            try
            {
                // Get account token
                JwtSecurityToken secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
                Guid uid = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);
                string role = secureToken.Claims.First(claim => claim.Type.Contains("role")).Value;

                var accounts = Get(account => !account.Role.Name.Equals("Admin") && account.IsActive).Include(account => account.Role);
                if (role == "Manager")
                {
                    var manager = await Get(account => account.Id == uid).Include(account => account.StaffAssignStorages.Where(staffAssignStorage => staffAssignStorage.IsActive)).FirstOrDefaultAsync();
                    // get storage id of storage where manager manage
                    List<Guid> storageIds = manager.StaffAssignStorages.Select(staffAssignStorage => staffAssignStorage.StorageId).ToList();

                    // account list remove manager
                    accounts = accounts.Where(account => account.Role.Name != "Manager").Include(accounts => accounts.Role);
                    // account list get account if storageIds contain storage id of manager manage
                    // account list get account if account do not in any storage
                    accounts = accounts.Where(account => account.StaffAssignStorages.Any(staffAssignStorage => storageIds.Contains((Guid)staffAssignStorage.StorageId)) || account.StaffAssignStorages.Count == 0)
                        .Include(accounts => accounts.Role);
                }

                var result = accounts.ProjectTo<AccountViewModel>(_mapper.ConfigurationProvider)
                    .DynamicFilter(model).PagingIQueryable(page, size, CommonConstant.LimitPaging, CommonConstant.DefaultPaging);
                if (result.Item2.ToList().Count < 1) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Can not found");
                var rs = new DynamicModelResponse<AccountViewModel>
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
            catch (ErrorResponse e)
            {
                throw new ErrorResponse((int)e.Error.Code, e.Error.Message);
            }
            catch (Exception ex)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<AccountViewModel> GetById(Guid id)
        {
            try
            {
                var result = await Get(account => account.Id == id && account.IsActive).ProjectTo<AccountViewModel>(_mapper.ConfigurationProvider).FirstOrDefaultAsync();
                if (result == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "User not found");
                return result;
            }
            catch (ErrorResponse e)
            {
                throw new ErrorResponse((int)e.Error.Code, e.Error.Message);
            }
            catch (Exception ex)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<AccountViewModel> GetByPhone(string phone)
        {
            try
            {
                _utilService.ValidatePhonenumber(phone);
                var result = await Get(account => account.Phone == phone && account.IsActive).ProjectTo<AccountViewModel>(_mapper.ConfigurationProvider).FirstOrDefaultAsync();
                if (result == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "User phone not found");
                return result;
            }
            catch (ErrorResponse e)
            {
                throw new ErrorResponse((int)e.Error.Code, e.Error.Message);
            }
            catch (Exception ex)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<TokenViewModel> Create(AccountCreateViewModel model)
        {
            try
            {
                // Validate input
                if (!_utilService.ValidateEmail(model.Email)) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Vui lòng nhập đúng email");
                _utilService.ValidatePassword(model.Password);
                _utilService.ValidateBirthDate(model.Birthdate);
                _utilService.ValidatePhonenumber(model.Phone);
                _utilService.ValidateString(model.Image.File, "Avatar");
                _utilService.ValidateString(model.Name,"Name");
                _utilService.ValidateString(model.Address, "Address");

                Role role = _roleService.Get(role => role.Id == model.RoleId && role.IsActive).First();
                if(role == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Role not found");

                // Create user to firebase
                var autho = new FirebaseAuthProvider(new FirebaseConfig(FirebaseKeyConstant.apiKEY));
                Firebase.Auth.User us = null;
                try
                {
                    var a = await autho.CreateUserWithEmailAndPasswordAsync(model.Email, EncryptedPassword(model.Password).ToString(), model.Name, false);
                    us = a.User;
                }
                catch (Exception e)
                {
                    throw new ErrorResponse((int)HttpStatusCode.BadRequest, e.Message);
                }

                var account = await Get(account => account.Email == model.Email && account.IsActive).FirstOrDefaultAsync();
                if (account != null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Email existed");

                // Create user to database
                
                var userCreate = _mapper.Map<Account>(model);
                var image = model.Image;
                userCreate.ImageUrl = null;
                userCreate.FirebaseId = us.LocalId;
                userCreate.DeviceTokenId = model.DeviceToken;
                userCreate.Password = EncryptedPassword(model.Password);
                await CreateAsync(userCreate);

                // Upload image to firebase
                if (image != null)
                {
                    var url = await _firebaseService.UploadImageToFirebase(image.File, "accounts", userCreate.Id, "avatar");
                    if (url != null) userCreate.ImageUrl = url;
                }

                List<StaffAssignStorage> staffToAssigns = null;
                // Assign user to storages
                if (model.StorageIds != null)
                {
                    staffToAssigns = new List<StaffAssignStorage>();
                    DateTime now = DateTime.Now;
                    for (int i = 0; i < model.StorageIds.Count; i++)
                    {
                        StaffAssignStorage staffAssignModel = new StaffAssignStorage
                        {
                            CreatedDate = now,
                            IsActive = true,
                            StorageId = model.StorageIds.ElementAt(i),
                            RoleName = userCreate.Role.Name
                        };
                        staffToAssigns.Add(staffAssignModel);
                    }
                }

                if (staffToAssigns != null) userCreate.StaffAssignStorages = staffToAssigns;

                // update user devicetoken
                userCreate.DeviceTokenId = model.DeviceToken;
                await UpdateAsync(userCreate);

                // Get user token to return
                var newUser = await Get(account => account.Email == model.Email && us.LocalId == account.FirebaseId && account.Password == EncryptedPassword(model.Password) && account.IsActive).Include(account => account.Role).Include(account => account.StaffAssignStorages).FirstOrDefaultAsync();
                var result = _mapper.Map<TokenViewModel>(newUser);
                var token = GenerateToken(newUser);
                var refreshToken = GenerateRefreshToken(newUser);
                token.RefreshToken = refreshToken;
                result = _mapper.Map(token, result);
                result.StorageId = null;
                
                // get office staff storage id
                var storageId = userCreate.StaffAssignStorages.Where(staffAssignStorage => staffAssignStorage.IsActive == true).FirstOrDefault()?.StorageId;
                if (storageId != null && userCreate.Role.Name == "Office Staff")
                    result.StorageId = storageId;
                
                return result;
            }
            catch (ErrorResponse e)
            {
                throw new ErrorResponse((int)e.Error.Code, e.Error.Message);
            }
            catch (Exception ex)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, ex.Message);
            }
            
        }

        public async Task<AccountViewModel> Update(Guid id, AccountUpdateViewModel model)
        {
            try
            {
                // validate input
                _utilService.ValidateBirthDate(model.Birthdate);
                _utilService.ValidatePhonenumber(model.Phone);
                _utilService.ValidateString(model.Name, "Name");
                _utilService.ValidateString(model.Address, "Address");

                if (id != model.Id) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "User Id not matched");

                var account = await Get(account => account.Id == id && account.IsActive).FirstOrDefaultAsync();
                if (account == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "User not found");

                var image = model.Image;

                // Upload image to firebase
                string url = account.ImageUrl;
                Account updateAccount = _mapper.Map(model, account);
                updateAccount.ImageUrl = url;
                if (image != null)
                {
                    url = await _firebaseService.UploadImageToFirebase(image.File, "accounts", id, "avatar");
                    if (url != null)
                        updateAccount.ImageUrl = url;
                }

                await UpdateAsync(updateAccount);

                return _mapper.Map<AccountViewModel>(updateAccount);
            }
            catch (ErrorResponse e)
            {
                throw new ErrorResponse((int)e.Error.Code, e.Error.Message);
            }
            catch (Exception ex)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<AccountViewModel> Delete(Guid id)
        {
            try
            {
                var entity = await Get(account => account.Id == id && account.IsActive)
                    .Include(account => account.Role)
                    .Include(account => account.Schedules).FirstOrDefaultAsync();
                if (entity == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "User not found");
                if(entity.Role.Name == "Customer") throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Can not delete customer");
                if (entity.Role.Name == "Delivery Staff")
                {
                    DateTime now = DateTime.Now;
                    var schedules = entity.Schedules.Where(schedule => schedule.Status == 1 && schedule.ScheduleDay.Date >= now.Date && schedule.IsActive).ToList();
                    if(schedules.Count > 0) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Delivery Staff has schedules to delivery");
                }
                entity.IsActive = false;
                await UpdateAsync(entity);
                return _mapper.Map<AccountViewModel>(entity);
            }
            catch (ErrorResponse e)
            {
                throw new ErrorResponse((int)e.Error.Code, e.Error.Message);
            }
            catch (Exception ex)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, ex.Message);
            }
            
        }

        private TokenGenerateViewModel GenerateToken(Account acc)
        {
            var storageId = acc.StaffAssignStorages.Where(x => x.IsActive).FirstOrDefault()?.StorageId.ToString();
            if (storageId == null) storageId = "";
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
            if (storageId == null) storageId = "";
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

        public async Task<List<AccountViewModel>> GetStaff(Guid? storageId, string accessToken, List<string> roleName, DateTime? scheduleDay, ICollection<string> deliveryTimes, bool getFromAllStorage)
        {
            var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
            var uid = secureToken.Claims.First(claim => claim.Type == "user_id").Value;
            var role = secureToken.Claims.First(claim => claim.Type.Contains("role")).Value;


            var staffs = Get(account => account.IsActive && account.Role.Name != "Admin" && account.Role.Name != "Customer").Include(account => account.StaffAssignStorages).Include(account => account.Schedules);
            if (roleName.Count > 0) staffs = Get(account => account.IsActive && roleName.Contains(account.Role.Name)).Include(account => account.StaffAssignStorages).Include(account => account.Schedules);
            if (role == "Manager")
                staffs = staffs.Where(account => account.Role.Name != "Manager").Include(account => account.StaffAssignStorages).Include(account => account.Schedules);

            // Nhân viên không thuộc kho nào
            if (storageId == null && !getFromAllStorage)
                staffs = staffs.Where(account => account.StaffAssignStorages.Where(staffAssignStorage => staffAssignStorage.IsActive).Count() == 0).Include(account => account.StaffAssignStorages).Include(account => account.Schedules);

            if(getFromAllStorage)
                staffs = staffs.Where(account => account.StaffAssignStorages.Where(staffAssignStorage => staffAssignStorage.IsActive).Count() > 0).Include(account => account.StaffAssignStorages).Include(account => account.Schedules);

            if (storageId != null)
                staffs = staffs.Where(account => account.StaffAssignStorages.Any(staffAssignStorage => staffAssignStorage.StorageId == storageId && staffAssignStorage.IsActive)).Include(account => account.StaffAssignStorages).Include(account => account.Schedules);

            if (scheduleDay != null)
            {
                // delivery staff busy in the time
                var usersInDelivery = _scheduleService.Get(schedule => scheduleDay.Value.Date == schedule.ScheduleDay.Date && deliveryTimes.Contains(schedule.ScheduleTime) && schedule.IsActive).Select(schedule => schedule.UserId).Distinct().ToList();
                staffs = staffs.Where(account => !usersInDelivery.Contains(account.Id)).Include(account => account.StaffAssignStorages).Include(account => account.Schedules);
                //delivery staff busy in the day
                var deliveryStaffBusyInDate = _scheduleService.Get(schedule => schedule.ScheduleDay.Date == scheduleDay.Value.Date && !schedule.IsActive && schedule.Status == 6).Select(schedule => schedule.UserId).Distinct().ToList();
                staffs = staffs.Where(account => !deliveryStaffBusyInDate.Contains(account.Id)).Include(account => account.StaffAssignStorages).Include(account => account.Schedules);
            }
            var result = await staffs.ProjectTo<AccountViewModel>(_mapper.ConfigurationProvider).ToListAsync();
            return result;
        }

        private byte[] EncryptedPassword(string password)
        {
            byte[] result;
            SHA256 mySha = SHA256.Create();
            Encoding enc = Encoding.UTF8;
            result = mySha.ComputeHash(enc.GetBytes(password));
            return result;
        }
    }
}
