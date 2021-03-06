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
using RSSMS.DataService.Enums;
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
        private readonly IRoleService _roleService;
        private readonly IUtilService _utilService;
        public AccountService(IUnitOfWork unitOfWork, IAccountRepository repository, IMapper mapper
            , IFirebaseService firebaseService, IRoleService roleService
            , IUtilService utilService) : base(unitOfWork, repository)
        {
            _mapper = mapper;
            _firebaseService = firebaseService;
            _roleService = roleService;
            _utilService = utilService;
        }


        public async Task<TokenViewModel> Login(AccountLoginViewModel model)
        {
            try
            {
                // Kiểm tra input email và password hợp lệ
                if (!_utilService.ValidateEmail(model.Email)) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Vui lòng nhập đúng email");
                _utilService.ValidatePassword(model.Password);

                // Kiểm tra account trên firebase
                User us = null;
                try
                {
                    FirebaseAuthProvider auth = new FirebaseAuthProvider(new FirebaseConfig(FirebaseKeyConstant.apiKEY));
                    if (auth != null)
                    {
                        FirebaseAuthLink a = await auth.SignInWithEmailAndPasswordAsync(model.Email, EncryptedPassword(model.Password).ToString());
                        if (a != null) us = a.User;
                    }

                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("EMAIL_NOT_FOUND"))
                        throw new ErrorResponse((int)HttpStatusCode.NotFound, "Không tìm thấy email");
                    if (ex.Message.Contains("INVALID_PASSWORD"))
                        throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Sai mật khẩu");

                }
                // Kiểm tra tài khoản trên database
                Account acc = null;
                if (us != null)
                    acc = await Get(account => account.Email == model.Email && us.LocalId == account.FirebaseId && account.Password.SequenceEqual(EncryptedPassword(model.Password)) && account.IsActive)
                    .Include(account => account.Role)
                    .Include(account => account.StaffAssignStorages).FirstOrDefaultAsync();
                if(acc == null)
                    acc = await Get(account => account.Email == model.Email && account.Password.SequenceEqual(EncryptedPassword(model.Password)) && account.IsActive)
                    .Include(account => account.Role)
                    .Include(account => account.StaffAssignStorages)
                    .FirstOrDefaultAsync();
                
                    

                if (acc == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Email hoặc mật khẩu không đúng");

                TokenViewModel result = _mapper.Map<TokenViewModel>(acc);

                // Tạo token
                TokenGenerateViewModel token = GenerateToken(acc);
                string refreshToken = GenerateRefreshToken(acc);
                token.RefreshToken = refreshToken;
                result = _mapper.Map(token, result);
                result.StorageId = null;

                // Cập nhật device token của người dùng
                acc.DeviceTokenId = model.DeviceToken;
                await UpdateAsync(acc);

                // Kiểm tra role của người dùng
                if (acc.Role.Name != "Office Staff" && acc.Role.Name != "Delivery Staff") return result;

                // Lấy Id của storage của nhân viên
                Guid? storageId = acc.StaffAssignStorages.Where(staffAssignStorage => staffAssignStorage.IsActive).FirstOrDefault()?.StorageId;
                if (storageId != null) result.StorageId = storageId;

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

        public async Task<AccountViewModel> ChangePassword(AccountChangePasswordViewModel model)
        {
            try
            {
                // Kiểm tra input
                _utilService.ValidatePassword(model.Password);
                _utilService.ValidatePassword(model.ConfirmPassword);
                _utilService.ValidatePassword(model.OldPassword);
                if (!model.ConfirmPassword.Equals(model.Password)) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Mật khẩu không khớp");

                Account account = await Get(account => account.Id == model.Id).FirstOrDefaultAsync();
                if (account == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Không tìm thấy tài khoản này");

                byte[] OldPassword = EncryptedPassword(model.OldPassword);
                if (!account.Password.SequenceEqual(OldPassword)) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Mật khẩu cũ không đúng");

                // Cập nhật tài khoản trên firebase
                if (account.FirebaseId != null)
                {
                    // Cài đặt firebase
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
                }


                //Update mật khẩu cho tài khoản vào database
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
                // Lấy token đăng nhập
                JwtSecurityToken secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
                Guid uid = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);
                var actor = await Get(account => account.Id == uid && account.IsActive).Include(account => account.Role).Include(account => account.StaffAssignStorages.Where(staffAssignStorage => staffAssignStorage.IsActive)).FirstOrDefaultAsync();
                var role = actor.Role;
                var accounts = Get(account => !account.Role.Name.Equals("Admin") && account.IsActive).Include(account => account.Role).Include(account => account.StaffAssignStorages);

                if (role.Name == "Manager")
                {
                    // Lấy Id của những kho mà manager quản lý
                    List<Guid> storageIds = actor.StaffAssignStorages.Select(staffAssignStorage => staffAssignStorage.StorageId).ToList();

                    // Lấy những account không phải manager và customer
                    accounts = accounts.Where(account => account.Role.Name != "Manager" && account.Role.Name != "Customer").Include(accounts => accounts.Role).Include(account => account.StaffAssignStorages);
                    // Lấy những tài khoản thuộc kho manager quản lý, hoặc không thuộc kho nào cả
                    accounts = accounts.Where(account => account.StaffAssignStorages.Any(staffAssignStorage => storageIds.Contains(staffAssignStorage.StorageId)) || account.StaffAssignStorages.Count == 0)
                        .Include(accounts => accounts.Role).Include(account => account.StaffAssignStorages);
                }
                if (role.Name == "Office Staff" || role.Name == "Delivery Staff" || role.Name == "Customer")
                    throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Không thể coi tài khoản người dùng khác");

                var result = accounts.ProjectTo<AccountViewModel>(_mapper.ConfigurationProvider)
                    .DynamicFilter(model).PagingIQueryable(page, size, CommonConstant.LimitPaging, CommonConstant.DefaultPaging);
                if (result.Item2.ToList().Count < 1) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Không tìm thấy tài khoản");
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
                if (result == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Không tìm thấy tài khoản");
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
                if (result == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Không tìm thấy tài khoản với số điện thoại trên");
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
                // Kiểm tra input
                if (!_utilService.ValidateEmail(model.Email)) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Vui lòng nhập đúng email");
                _utilService.ValidatePassword(model.Password);
                _utilService.ValidateBirthDate(model.Birthdate);
                _utilService.ValidatePhonenumber(model.Phone);
                _utilService.ValidateString(model.Name, "Tên");
                _utilService.ValidateString(model.Address, "Địa chỉ");

                Role role = _roleService.Get(role => role.Id == model.RoleId).First();
                if (role == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Vui lòng nhập chức vụ");

                var account = await Get(account => account.Phone == model.Phone).FirstOrDefaultAsync();
                if (account != null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Số điện thoại bị trùng");
                account = await Get(account => account.Email == model.Email).FirstOrDefaultAsync();
                if (account != null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Email đã tồn tại");
                // Tạo tài khoản trên firebase
                var autho = new FirebaseAuthProvider(new FirebaseConfig(FirebaseKeyConstant.apiKEY));
                User us = null;
                try
                {
                    var a = await autho.CreateUserWithEmailAndPasswordAsync(model.Email, EncryptedPassword(model.Password).ToString(), model.Name, false);
                    us = a.User;
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("EMAIL_EXISTS"))
                        throw new ErrorResponse((int)HttpStatusCode.NotFound, "Email đã tồn tại");
                }



                // Cập nhật thông tin tài khoản

                var userCreate = _mapper.Map<Account>(model);
                var image = model.Image;
                userCreate.ImageUrl = null;
                userCreate.FirebaseId = us.LocalId;
                userCreate.DeviceTokenId = model.DeviceToken;
                userCreate.Password = EncryptedPassword(model.Password);


                // Đăng ảnh lên firebase
                if (image != null)
                {
                    var url = await _firebaseService.UploadImageToFirebase(image.File, "accounts", userCreate.Id, "avatar");
                    if (url != null) userCreate.ImageUrl = url;
                }

                List<StaffAssignStorage> staffToAssigns = null;
                // Phân công nhân viên vào kho
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
                        };
                        staffToAssigns.Add(staffAssignModel);
                    }
                }

                if (staffToAssigns != null)
                    userCreate.StaffAssignStorages = staffToAssigns;


                await CreateAsync(userCreate);



                // Lấy JWT để trả về
                var newUser = await Get(account => account.Id == userCreate.Id).Include(account => account.Role).Include(account => account.StaffAssignStorages).FirstOrDefaultAsync();
                var result = _mapper.Map<TokenViewModel>(newUser);
                var token = GenerateToken(newUser);
                var refreshToken = GenerateRefreshToken(newUser);
                token.RefreshToken = refreshToken;
                result = _mapper.Map(token, result);
                result.StorageId = null;

                // Lấy Id của kho mà nhân viên thuộc
                var storageId = userCreate.StaffAssignStorages.Where(staffAssignStorage => staffAssignStorage.IsActive == true).FirstOrDefault()?.StorageId;
                if (storageId != null && (userCreate.Role.Name == "Office Staff" || userCreate.Role.Name == "Delivery Staff"))
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
                // Kiểm tra input
                _utilService.ValidateBirthDate(model.Birthdate);
                _utilService.ValidatePhonenumber(model.Phone);
                _utilService.ValidateString(model.Name, "Tên");
                _utilService.ValidateString(model.Address, "Địa chỉ");

                if (id != model.Id) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Id tài khoản không khớp");

                var account = await Get(account => account.Id == id && account.IsActive).FirstOrDefaultAsync();
                if (account == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Không tìm thấy tài khoản");

                var image = model.Image;

                // Đăng ảnh lên firebase
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
                if (entity == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Không tìm thấy tài khoản");
                if (entity.Role.Name == "Delivery Staff")
                {
                    DateTime now = DateTime.Now;
                    var schedules = entity.Schedules.Where(schedule => schedule.Status == 1 && schedule.ScheduleDay.Date >= now.Date && schedule.IsActive).ToList();
                    if (schedules.Count > 0) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Nhân viên vận chuyển còn lịch cần thực hiện");
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
                expires: DateTime.Now.AddMonths(1),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );
            TimeSpan expires = DateTime.Now.Subtract(token.ValidTo);
            return new TokenGenerateViewModel
            {
                IdToken = new JwtSecurityTokenHandler().WriteToken(token),
                ExpiresIn = expires.TotalDays,
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
            try
            {
                // Lấy token của actor
                var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
                var userId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);
                var account = Get(account => account.IsActive && (Guid)account.Id == userId).Include(account => account.Role).FirstOrDefault();


                var staffs = Get(account => account.IsActive && account.Role.Name != "Admin" && account.Role.Name != "Customer").Include(account => account.StaffAssignStorages).Include(account => account.Schedules);
                if (roleName.Count > 0) staffs = Get(account => account.IsActive && roleName.Contains(account.Role.Name)).Include(account => account.StaffAssignStorages).Include(account => account.Schedules);
                // Admin chỉ lấy người quản lý
                if (account.Role.Name == "Admin")
                    staffs = staffs.Where(account => account.Role.Name == "Manager").Include(account => account.StaffAssignStorages).Include(account => account.Schedules);
                // Người quản lý chỉ lấy nhân viên thủ kho hoặc vận chuyển
                if (account.Role.Name == "Manager")
                    staffs = staffs.Where(account => account.Role.Name != "Manager").Include(account => account.StaffAssignStorages).Include(account => account.Schedules);

                // Lấy nhân viên chưa được phân công vào kho
                if (storageId == null && !getFromAllStorage)
                    staffs = staffs.Where(account => (account.StaffAssignStorages.Where(staffAssignStorage => staffAssignStorage.IsActive).Count() == 0) || account.Role.Name == "Manager").Include(account => account.StaffAssignStorages).Include(account => account.Schedules);
                // Lấy tất cả nhân viên từ mọi kho
                if (getFromAllStorage)
                    staffs = staffs.Where(account => account.StaffAssignStorages.Where(staffAssignStorage => staffAssignStorage.IsActive).Count() > 0).Include(account => account.StaffAssignStorages).Include(account => account.Schedules);
                // Lấy đúng kho
                if (storageId != null)
                    staffs = staffs.Where(account => account.StaffAssignStorages.Any(staffAssignStorage => staffAssignStorage.StorageId == storageId && staffAssignStorage.IsActive)).Include(account => account.StaffAssignStorages).Include(account => account.Schedules);
                if (scheduleDay != null)
                {
                    List<TimeSpan> timeSpan = new List<TimeSpan>();
                    if (deliveryTimes.Count > 0)
                    {
                        foreach (var time in deliveryTimes)
                        {
                            if (!string.IsNullOrWhiteSpace(time))
                                timeSpan.Add(_utilService.StringToTime(time));
                        }

                        // Lấy những nhân viên không bận trong khung giờ
                        if (timeSpan.Count > 0)
                            staffs = staffs.Where(account => account.Schedules.Where(schedule => schedule.ScheduleDay.Date == scheduleDay.Value.Date && timeSpan.Contains(schedule.ScheduleTime) && schedule.IsActive).ToList().Count == 0).Include(account => account.StaffAssignStorages).Include(account => account.Schedules);
                    }
                    // Lấy những nhân viên không bận trong ngày
                    staffs = staffs.Where(account => account.Requests.Where(request => request.CreatedBy == account.Id && request.IsActive && request.Type == (int)RequestType.Huy_lich_giao_hang && request.CancelDate.Value.Date == scheduleDay.Value.Date).FirstOrDefault() == null).Include(account => account.StaffAssignStorages).Include(account => account.Schedules);
                }
                var result = await staffs.ProjectTo<AccountViewModel>(_mapper.ConfigurationProvider).ToListAsync();
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
