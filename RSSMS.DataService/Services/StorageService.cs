using _3DBinPacking.Enum;
using _3DBinPacking.Model;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Constants;
using RSSMS.DataService.Enums;
using RSSMS.DataService.Models;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.Responses;
using RSSMS.DataService.UnitOfWorks;
using RSSMS.DataService.Utilities;
using RSSMS.DataService.ViewModels.Floors;
using RSSMS.DataService.ViewModels.Geocodes;
using RSSMS.DataService.ViewModels.Requests;
using RSSMS.DataService.ViewModels.StaffAssignStorage;
using RSSMS.DataService.ViewModels.Storages;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
namespace RSSMS.DataService.Services
{
    public interface IStorageService : IBaseService<Storage>
    {
        Task<DynamicModelResponse<StorageViewModel>> GetAll(StorageViewModel model, List<int> types, string[] fields, int page, int size, string accessToken);
        Task<StorageDetailViewModel> GetById(Guid id, string accessToken);
        Task<StorageViewModel> Create(StorageCreateViewModel model);
        Task<StorageUpdateViewModel> Update(Guid id, StorageUpdateViewModel model);
        Task<StorageViewModel> Delete(Guid id);
        Task<IDictionary<Guid, List<FloorGetByIdViewModel>>> GetFloorWithStorage(Guid? storageId, int spaceType, DateTime date, bool isMany);
        Task<StaffAssignStorageCreateViewModel> AssignStaffToStorage(StaffAssignInStorageViewModel model, string accessToken);
        Task<bool> CheckStorageAvailable(Guid? storageId, int spaceType, DateTime dateFrom, DateTime dateTo, bool isMany, List<Cuboid> cuboids, List<Request> requestsAssignToStorage, bool isCustomerDelivery, string accessToken, List<string> deliveryTimes, bool isCreateOrder);
        Task<List<StorageViewModel>> GetStorageAvailable(Guid? storageId, int spaceType, DateTime dateFrom, DateTime dateTo, bool isMany, List<Cuboid> cuboids, List<Request> requestsAssignToStorage, bool isCustomerDelivery, string accessToken, List<string> deliveryTimes, string deliveryAddress, decimal serviceDeliveryFee);
    }
    public class StorageService : BaseService<Storage>, IStorageService
    {
        private readonly IMapper _mapper;
        private readonly IStaffAssignStorageService _staffAssignStoragesService;
        private readonly IAreaService _areaService;
        private readonly IFirebaseService _firebaseService;
        private readonly IUtilService _utilService;
        private readonly IAccountService _accountService;
        public StorageService(IUnitOfWork unitOfWork, IStorageRepository repository, IMapper mapper
            , IStaffAssignStorageService staffAssignStoragesService, IAreaService areaService, IAccountService accountService
            , IFirebaseService firebaseService, IUtilService utilService) : base(unitOfWork, repository)
        {
            _mapper = mapper;
            _staffAssignStoragesService = staffAssignStoragesService;
            _areaService = areaService;
            _firebaseService = firebaseService;
            _utilService = utilService;
            _accountService = accountService;
        }

        public async Task<StorageViewModel> Create(StorageCreateViewModel model)
        {
            try
            {
                _utilService.ValidateString(model.Image.File, " ảnh kho");
                _utilService.ValidateString(model.Name, " tên kho");
                _utilService.ValidateString(model.Address, " địa chỉ kho");
                _utilService.ValidateDecimal(model.Width, " chiều rộng kho");
                _utilService.ValidateDecimal(model.Height, " chiều cao kho");
                _utilService.ValidateDecimal(model.Length, " chiều dài kho");

                var storage = _mapper.Map<Storage>(model);
                var image = model.Image;
                await CreateAsync(storage);
                if (image != null)
                {
                    if (image.File != null)
                    {
                        var url = await _firebaseService.UploadImageToFirebase(image.File, "storages", storage.Id, "avatar");
                        if (url != null)
                        {
                            storage.ImageUrl = url;
                        }

                    }
                }
                await UpdateAsync(storage);
                return _mapper.Map<StorageViewModel>(storage);
            }
            catch (ErrorResponse e)
            {
                throw new ErrorResponse((int)e.Error.Code, e.Error.Message);
            }
            catch (Exception e)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, e.Message);
            }


        }

        public async Task<StorageViewModel> Delete(Guid id)
        {
            try
            {
                var entity = await Get(x => x.Id == id && x.IsActive).Include(a => a.Areas.Where(area => area.IsActive))
                    .Include(storage => storage.Requests)
                    .Include(storage => storage.StaffAssignStorages).ThenInclude(staffAssign => staffAssign.Staff).ThenInclude(staff => staff.Role)
                    .Include(storage => storage.Orders)
                    .FirstOrDefaultAsync();
                if (entity == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Storage id not found");
                var areas = entity.Areas;
                foreach (var area in areas)
                    if (await _areaService.CheckIsUsed(area.Id)) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Storage is in used");
                if (entity.Orders.Where(order => !order.IsActive || order.Status != 0 || order.Status != 6 || order.Status != 7).Count() > 0)
                    throw new ErrorResponse((int)HttpStatusCode.NotFound, "Trong kho vẫn còn đơn đang được gán vào");
                if (entity.Requests.Where(request => !request.IsActive || request.Status != 0 || request.Status != 3 || request.Status != 4 || request.Status != 5).Count() > 0)
                    throw new ErrorResponse((int)HttpStatusCode.NotFound, "Trong kho vẫn còn đơn yêu cầu đang được gán vào");
                if (entity.StaffAssignStorages.Where(staffAssign => staffAssign.IsActive && staffAssign.Staff.Role.Name != "Manager").Count() > 0)
                    throw new ErrorResponse((int)HttpStatusCode.NotFound, "Trong kho vẫn còn nhân viên đang được gán vào");
                entity.IsActive = false;
                await UpdateAsync(entity);
                return _mapper.Map<StorageViewModel>(entity);
            }
            catch (ErrorResponse e)
            {
                throw new ErrorResponse((int)e.Error.Code, e.Error.Message);
            }
            catch (Exception e)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, e.Message);
            }

        }

        public async Task<DynamicModelResponse<StorageViewModel>> GetAll(StorageViewModel model, List<int> types, string[] fields, int page, int size, string accessToken)
        {
            try
            {
                var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
                var userId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);
                var role = secureToken.Claims.First(claim => claim.Type.Contains("role")).Value;

                var storages = Get(x => x.IsActive == true).Include(a => a.StaffAssignStorages.Where(s => s.Staff.Role.Name == "Manager" && s.IsActive == true)).ThenInclude(a => a.Staff).ProjectTo<StorageViewModel>(_mapper.ConfigurationProvider).DynamicFilter(model);



                if (types.Count > 0)
                    storages = storages.Where(x => types.Contains((int)x.Type));


                if (role == "Manager")
                {
                    var storagesManagerManage = _staffAssignStoragesService.Get(x => x.StaffId == userId && x.IsActive == true).Select(x => x.StorageId).ToList();
                    storages = storages.Where(x => storagesManagerManage.Contains((Guid)x.Id));
                }


                var result = storages.PagingIQueryable(page, size, CommonConstant.LimitPaging, CommonConstant.DefaultPaging);

                if (result.Item2.ToList().Count < 1) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Can not found");
                var rs = new DynamicModelResponse<StorageViewModel>
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
            catch (ErrorResponse e)
            {
                throw new ErrorResponse((int)e.Error.Code, e.Error.Message);
            }
            catch (Exception e)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, e.Message);
            }

        }

        public async Task<StorageDetailViewModel> GetById(Guid id, string accessToken)
        {
            try
            {
                var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
                var userId = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);
                var role = secureToken.Claims.First(claim => claim.Type.Contains("role")).Value;

                var result = await Get(x => x.Id == id && x.IsActive == true)
                                    .Include(a => a.StaffAssignStorages.Where(s => s.Staff.Role.Name == "Manager" && s.IsActive == true))
                                    .ThenInclude(a => a.Staff).ProjectTo<StorageDetailViewModel>(_mapper.ConfigurationProvider).FirstOrDefaultAsync();
                if (result == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Storage id not found");

                if (role == "Office Staff")
                {
                    result = await Get(x => x.Id == id && x.IsActive == true)
                    .Include(a => a.StaffAssignStorages.Where(s => s.Staff.Role.Name == "Office Staff" && s.IsActive == true && s.StaffId == userId))
                    .ThenInclude(a => a.Staff).ProjectTo<StorageDetailViewModel>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync();
                }

                return result;
            }
            catch (ErrorResponse e)
            {
                throw new ErrorResponse((int)e.Error.Code, e.Error.Message);
            }
            catch (Exception e)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, e.Message);
            }

        }

        public async Task<StorageUpdateViewModel> Update(Guid id, StorageUpdateViewModel model)
        {
            try
            {
                _utilService.ValidateString(model.Name, " tên kho");
                _utilService.ValidateString(model.Address, " địa chỉ kho");
                _utilService.ValidateDecimal(model.Width, " chiều rộng kho");
                _utilService.ValidateDecimal(model.Height, " chiều cao kho");
                _utilService.ValidateDecimal(model.Length, " chiều dài kho");

                if (id != model.Id) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Storage Id not matched");

                var entity = await Get(storage => storage.Id == id && storage.IsActive).Include(a => a.Areas).FirstOrDefaultAsync();
                if (entity == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Storage not found");

                var areas = entity.Areas.Where(area => area.IsActive).ToList();
                foreach (var area in areas)
                    if (await _areaService.CheckIsUsed(area.Id)) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Storage is in used");

                //Check kích thước kho sau khi update
                List<Cuboid> cuboids = new List<Cuboid>();
                for (int i = 0; i < areas.Count; i++)
                    cuboids.Add(new Cuboid((decimal)areas[i].Width, (decimal)areas[i].Height, (decimal)areas[i].Length));

                var parameter = new BinPackParameter((decimal)model.Width, (decimal)model.Height, (decimal)model.Length, cuboids);

                var binPacker = BinPacker.GetDefault(BinPackerVerifyOption.BestOnly);
                var result = binPacker.Pack(parameter);
                if (result.BestResult.Count > 1)
                    throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Kích thước kho quá nhỏ so với kích thước khu vực đang tồn tại");

                var updateEntity = _mapper.Map(model, entity);

                var image = model.Image;
                if (image != null)
                {
                    if (image.File != null)
                    {
                        var url = await _firebaseService.UploadImageToFirebase(image.File, "storages", id, "avatar");
                        if (url != null) updateEntity.ImageUrl = url;
                    }
                }
                await UpdateAsync(updateEntity);

                return _mapper.Map<StorageUpdateViewModel>(updateEntity);
            }
            catch (InvalidOperationException)
            {
                throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Kích thước kho quá nhỏ so với kích thước khu vực đang tồn tại");
            }
            catch (ErrorResponse e)
            {
                throw new ErrorResponse((int)e.Error.Code, e.Error.Message);
            }
            catch (Exception e)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, e.Message);
            }

        }


        public async Task<IDictionary<Guid, List<FloorGetByIdViewModel>>> GetFloorWithStorage(Guid? storageId, int spaceType, DateTime date, bool isMany)
        {
            // storage Id
            IDictionary<Guid, List<FloorGetByIdViewModel>> result = new Dictionary<Guid, List<FloorGetByIdViewModel>>();
            IQueryable<Storage> storages = Get(storage => storage.IsActive).Include(storage => storage.Areas.Where(area => area.IsActive));
            if (storageId != null) storages = storages.Where(storage => storage.Id == storageId).Include(storage => storage.Areas.Where(area => area.IsActive));
            if (storages.ToList().Count == 0) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Not enough storage");
            var storageList = storages.ToList();
            for (int i = 0; i < storageList.Count; i++)
            {
                var area = await _areaService.GetFloorOfArea(storageList[i].Id, spaceType, date, isMany);
                // Add result vào
                if (area != null) result.Add(storageList[i].Id, area);
            }
            if (result.Count == 0) return null;
            return result;
        }

        public async Task<StaffAssignStorageCreateViewModel> AssignStaffToStorage(StaffAssignInStorageViewModel model, string accessToken)
        {
            try
            {
                var secureToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
                var uid = Guid.Parse(secureToken.Claims.First(claim => claim.Type == "user_id").Value);

                var staffAssigned = model.UserAssigned;
                var staffUnAssigned = model.UserUnAssigned;

                // Validate staff unassigned
                if (staffUnAssigned != null)
                {
                    var staffUnAssignedId = staffUnAssigned.Select(staff => staff.UserId);
                    // get staff need to unassign with role and schedules
                    var staffsToUnAssigned = _staffAssignStoragesService.Get(staffAssign => staffUnAssignedId.Contains(staffAssign.Id) && staffAssign.IsActive)
                                                .Include(staffAssign => staffAssign.Staff).ThenInclude(staff => staff.Role)
                                                .Include(staffAssign => staffAssign.Staff).ThenInclude(staff => staff.Schedules).ThenInclude(schedule => schedule.Request).ToList();
                    // check if there is staff who schedule have not finish
                    if (staffsToUnAssigned.Where(staffAssign => staffAssign.Staff.Schedules.Where(schedule => schedule.IsActive && (schedule.Request.Status == (int)RequestStatus.Dang_van_chuyen || schedule.Request.Status == (int)RequestStatus.Da_xu_ly)).Count() > 0).Count() > 0)
                        throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Nhân viên còn lịch chưa hoàn thành không thể rút khỏi kho");

                    // check if there is request that is assigned to this storage but unassigned staff leading to not enough staff;
                    var requests = Get(storage => storage.Id == model.StorageId && storage.IsActive).Include(storage => storage.Requests).FirstOrDefault()
                        .Requests.Where(request => request.IsActive && request.DeliveryTime != null && (request.Type == (int)RequestType.Tao_don || request.Type == (int)RequestType.Tra_don) && (request.Status == 2 || request.Status == 4) && request.DeliveryDate != null).AsEnumerable();
                    if (requests.Count() > 0)
                    {
                        // group by request to schedule day
                        var requestByDateTime = requests.GroupBy(request => new { request.DeliveryDate, request.DeliveryTime })
                                            .Select(request => new RequestByDateTimeViewModel
                                            {
                                                DeliveryDate = (DateTime)request.Key.DeliveryDate,
                                                DeliveryTime = _utilService.TimeToString((TimeSpan)request.Key.DeliveryTime),
                                                Requests = request.ToList()
                                            });
                        foreach (var request in requestByDateTime)
                        {
                            if(staffAssigned == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Nhân viên được thêm sẽ không đủ với các yêu cầu đã được tiếp nhận");
                            if (request.Requests?.Count > staffAssigned.Count)
                                throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Nhân viên được thêm sẽ không đủ với các yêu cầu đã được tiếp nhận");
                        }
                    }

                    var staffs = _staffAssignStoragesService.Get(x => x.StorageId == model.StorageId && x.IsActive == true).ToList().Where(x => staffUnAssigned.Any(a => a.UserId == x.StaffId)).ToList();

                    foreach (var staff in staffs)
                    {
                        staff.IsActive = false;
                        staff.ModifiedBy = uid;
                        await _staffAssignStoragesService.UpdateAsync(staff);
                    }
                }

                if (staffAssigned != null)
                {
                    var managerAssigned = staffAssigned.Where(a => a.RoleName == "Manager").ToList();
                    if (managerAssigned.Count > 0)
                    {
                        if (managerAssigned.Count > 1) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "More than 1 manager assigned to this storage");
                        var managerInStorage = _staffAssignStoragesService.Get(x => x.Staff.Role.Name == "Manager" && x.StorageId == model.StorageId && x.IsActive == true).FirstOrDefault();
                        if (managerInStorage != null)
                        {
                            if (staffAssigned.Where(x => x.UserId == managerInStorage.StaffId).FirstOrDefault() == null && staffUnAssigned.Where(x => x.UserId == managerInStorage.StaffId).FirstOrDefault() == null)
                                throw new ErrorResponse((int)HttpStatusCode.BadRequest, "More than 1 manager assigned to this storage");
                        }
                    }

                    foreach (var staff in staffAssigned)
                    {
                        var staffManageStorage = await _staffAssignStoragesService.Get(x => x.StaffId == staff.UserId && x.Staff.Role.Name != "Manager" && x.StorageId != model.StorageId && x.IsActive == true).FirstOrDefaultAsync();
                        if (staffManageStorage != null)
                        {
                            throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Staff has assigned to a storage before");
                        }

                    }



                    foreach (var staff in staffAssigned)
                    {
                        var staffAssign = await _staffAssignStoragesService.Get(x => x.StorageId == model.StorageId && x.StaffId == staff.UserId && x.IsActive == true).FirstOrDefaultAsync();
                        if (staffAssign == null)
                        {
                            StaffAssignStorage staffAdd = new StaffAssignStorage
                            {
                                StorageId = model.StorageId,
                                StaffId = staff.UserId,
                                IsActive = true,
                                CreatedDate = DateTime.Now
                            };
                            await _staffAssignStoragesService.CreateAsync(staffAdd);
                        }
                    }
                }

                return null;
            }
            catch (ErrorResponse e)
            {
                throw new ErrorResponse((int)e.Error.Code, e.Error.Message);
            }
            catch (Exception e)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, e.Message);
            }

        }

        public async Task<bool> CheckStorageAvailable(Guid? storageId, int spaceType, DateTime dateFrom, DateTime dateTo, bool isMany, List<Cuboid> cuboids, List<Request> requestsAssignToStorage, bool isCustomerDelivery, string accessToken, List<string> deliveryTimes, bool isCreateOrder)
        {
            bool result = false;

            // Get all storage
            IQueryable<Storage> storages = Get(storage => storage.IsActive).Include(storage => storage.Areas.Where(area => area.IsActive));

            // Get storage by Id
            if (storageId != null) storages = storages.Where(storage => storage.Id == storageId).Include(storage => storage.Areas.Where(area => area.IsActive));
            if (storages.ToList().Count == 0) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Không đủ kho");

            var storageList = storages.ToList();
            int i = 0;
            bool deliFlag = false;
            do
            {
                // request chiếm kho
                var requestsNeedSpace = requestsAssignToStorage.Where(request => request.StorageId == storageList[i].Id && (request.Type == (int)RequestType.Gia_han_don || request.Type == (int)RequestType.Tao_don) && (request.Status == (int)RequestStatus.Da_xu_ly || request.Status == (int)RequestStatus.Dang_xu_ly)).ToList();
                // request chiếm nhân viên
                var requestsNeedDeliver = requestsAssignToStorage.Where(request => request.StorageId == storageList[i].Id && (request.Type == (int)RequestType.Tao_don || request.Type == (int)RequestType.Tra_don) && (request.Status == (int)RequestStatus.Da_xu_ly || request.Status == (int)RequestStatus.Dang_xu_ly)).ToList();
                if (spaceType == (int)SpaceType.Dien_tich) requestsNeedSpace = requestsNeedSpace.Where(request => request.TypeOrder == (int)OrderType.Kho_tu_quan).ToList();
                result = await _areaService.CheckAreaAvailable(storageList[i].Id, spaceType, dateFrom, dateTo, isMany, cuboids, requestsNeedSpace, isCustomerDelivery);
                var staffs = await _accountService.GetStaff(storageList[i].Id, accessToken, new List<string> { "Delivery Staff" }, dateFrom, deliveryTimes, false);
                if (result)
                {
                    if (!isCustomerDelivery && spaceType == (int)SpaceType.Ke && !isCreateOrder)
                    {
                        // list time
                        List<TimeSpan> timeSpan = new List<TimeSpan>();
                        if (deliveryTimes.Count > 0)
                        {
                            foreach (var time in deliveryTimes)
                                timeSpan.Add(_utilService.StringToTime(time));
                        }
                        var requestsNeedToDeli = requestsNeedDeliver.Where(request => !(bool)request.IsCustomerDelivery && request.DeliveryDate.Value.Date == dateFrom.Date && request.TypeOrder == (int)OrderType.Giu_do_thue && timeSpan.Contains((TimeSpan)request.DeliveryTime)).ToList();
                        if (requestsNeedToDeli.Count() + 1 <= staffs.Count())
                        {
                            deliFlag = true;
                            break;
                        }

                        result = false;
                    }
                    break;
                }
                i++;
            } while (i < storageList.Count);
            if(!result) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Kho không còn chỗ chứa");
            if (!deliFlag && !isCustomerDelivery && spaceType == (int)SpaceType.Ke && !isCreateOrder) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Không đủ nhân viên vận chuyển");
            return result;
        }

        public async Task<List<StorageViewModel>> GetStorageAvailable(Guid? storageId, int spaceType, DateTime dateFrom, DateTime dateTo, bool isMany, List<Cuboid> cuboids, List<Request> requestsAssignToStorage, bool isCustomerDelivery, string accessToken, List<string> deliveryTimes, string deliveryAddress, decimal serviceDeliveryFee)
        {
            List<StorageViewModel> result = new List<StorageViewModel>();
            bool isAvailable = false;
            // Get all storage
            IQueryable<Storage> storages = Get(storage => storage.IsActive).Include(storage => storage.Areas.Where(area => area.IsActive));

            // Get storage by Id
            if (storageId != null) storages = storages.Where(storage => storage.Id == storageId).Include(storage => storage.Areas.Where(area => area.IsActive));
            if (storages.ToList().Count == 0) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Không đủ kho");

            var storageList = storages.ToList();
            int i = 0;
            do
            {
                // request chiếm kho
                var requestsNeedSpace = requestsAssignToStorage.Where(request => request.StorageId == storageList[i].Id && (request.Type == (int)RequestType.Gia_han_don || request.Type == (int)RequestType.Tao_don) && (request.Status == (int)RequestStatus.Da_xu_ly || request.Status == (int)RequestStatus.Dang_xu_ly)).ToList();
                // request chiếm nhân viên
                var requestsNeedDeliver = requestsAssignToStorage.Where(request => request.StorageId == storageList[i].Id && (request.Type == (int)RequestType.Tao_don || request.Type == (int)RequestType.Tra_don) && (request.Status == (int)RequestStatus.Da_xu_ly || request.Status == (int)RequestStatus.Dang_xu_ly)).ToList();
                if (spaceType == (int)SpaceType.Dien_tich) requestsNeedSpace = requestsNeedSpace.Where(request => request.TypeOrder == (int)OrderType.Kho_tu_quan).ToList();
                isAvailable = await _areaService.CheckAreaAvailable(storageList[i].Id, spaceType, dateFrom, dateTo, isMany, cuboids, requestsNeedSpace, isCustomerDelivery);
                var staffs = await _accountService.GetStaff(storageList[i].Id, accessToken, new List<string> { "Delivery Staff" }, dateFrom, deliveryTimes, false);
                if (isAvailable)
                {
                    if (!isCustomerDelivery && spaceType == (int)SpaceType.Ke)
                    {
                        // list time
                        List<TimeSpan> timeSpan = new List<TimeSpan>();
                        if (deliveryTimes.Count > 0)
                        {
                            foreach (var time in deliveryTimes)
                                timeSpan.Add(_utilService.StringToTime(time));
                        }
                        var requestsNeedToDeli = requestsNeedDeliver.Where(request => !(bool)request.IsCustomerDelivery && request.DeliveryDate.Value.Date == dateFrom.Date && request.TypeOrder == (int)OrderType.Giu_do_thue && timeSpan.Contains((TimeSpan)request.DeliveryTime)).ToList();
                        if (requestsNeedToDeli.Count() + 1 <= staffs.Count())
                        {
                            StorageViewModel storage = _mapper.Map<StorageViewModel>(storageList[i]);
                            DistanceViewModel distance = await GetDistanceFromCustomerToStorage(deliveryAddress, storage.Address);
                            if(distance != null)
                            {
                                storage.DeliveryDistance = distance.rows[0].elements[0].distance.text;
                                storage.DeliveryFee = serviceDeliveryFee * Math.Ceiling(Convert.ToDecimal(storage.DeliveryDistance.Split(' ')[0]));
                            }
                                
                            result.Add(storage);
                        }
                    } else
                        result.Add(_mapper.Map<StorageViewModel>(storageList[i]));
                }
                isAvailable = false;
                i++;
            } while (i < storageList.Count);
            return result;
        }

        private async Task<DistanceViewModel> GetDistanceFromCustomerToStorage(string deliveryAddress, string storageAddress)
        {
            DistanceViewModel result= null;

            // Điểm bắt đầu
            string origin = null;
            // Điểm đến
            string destination = null;
            string key = "9gAZdEIcBCUAYj8o3p7EcCepH30zYGgK1Pw3LULV";
            GeometryViewModel geometry = new GeometryViewModel();
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://rsapi.goong.io/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = await client.GetAsync(
                $"/geocode?address={deliveryAddress}&api_key={key}"
                );
            if (response.IsSuccessStatusCode)
            {
                geometry = await response.Content.ReadAsAsync<GeometryViewModel>();
                if (geometry.results.Count() > 0)
                    origin = geometry.results[0].geometry.location.lat + "," + geometry.results[0].geometry.location.lng;
            }
            
            response = await client.GetAsync(
                $"/geocode?address={storageAddress}&api_key={key}"
                );
            if (response.IsSuccessStatusCode)
            {
                geometry = await response.Content.ReadAsAsync<GeometryViewModel>();
                if (geometry.results.Count() > 0)
                    destination = geometry.results[0].geometry.location.lat + "," + geometry.results[0].geometry.location.lng;
            }


            // tính quãng đường
            response = await client.GetAsync(
                $"DistanceMatrix?origins={origin}&destinations={destination}&vehicle=truck&api_key={key}"
                );
            if (response.IsSuccessStatusCode)
            {
                result = await response.Content.ReadAsAsync<DistanceViewModel>();
            }
            return result;
        }
    }
}
