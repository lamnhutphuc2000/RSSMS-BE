using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Models;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.Responses;
using RSSMS.DataService.UnitOfWorks;
using RSSMS.DataService.ViewModels.Floors;
using RSSMS.DataService.ViewModels.OrderDetails;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RSSMS.DataService.Services
{
    public interface IFloorService : IBaseService<Floor>
    {
        Task<FloorGetByIdViewModel> GetFloorWithOrderDetail(Guid floorId);
        Task<bool> CreateNumberOfFloor(Guid spaceId, int numberOfFloor, decimal floorHeight, decimal floorWidth, decimal floorLength, DateTime now);
        Task<bool> RemoveFloors(Guid spaceId);
        Task<List<FloorInSpaceViewModel>> GetFloorInSpace(Guid spaceId, DateTime? date);
        Task<FloorGetByIdViewModel> GetById(Guid id);
        Task<List<FloorGetByIdViewModel>> GetBySpaceId(Guid spaceId, DateTime dateFrom, DateTime dateTo, bool isMany, bool isSelfStorage);
    }
    public class FloorService : BaseService<Floor>, IFloorService

    {
        private readonly IMapper _mapper;
        private readonly IOrderDetailService _orderDetailService;
        public FloorService(IUnitOfWork unitOfWork, IOrderDetailService orderDetailService, IFloorRepository repository, IMapper mapper) : base(unitOfWork, repository)
        {
            _mapper = mapper;
            _orderDetailService = orderDetailService;
        }
        public async Task<FloorGetByIdViewModel> GetFloorWithOrderDetail(Guid floorId)
        {
            try
            {
                FloorGetByIdViewModel result = Get(floor => floor.Id == floorId).Include(floor => floor.Space).ProjectTo<FloorGetByIdViewModel>(_mapper.ConfigurationProvider).FirstOrDefault();

                var orderDetails = await _orderDetailService.Get(orderDetail => orderDetail.ExportId == null && orderDetail.ImportId != null && orderDetail.Status != 0 && orderDetail.Order.Status != 0)
                    .Include(orderDetail => orderDetail.Import).ThenInclude(import => import.Floor).ThenInclude(floor => floor.Space).ThenInclude(space => space.Area).ThenInclude(area => area.Storage)
                    .Include(orderDetail => orderDetail.TransferDetails).ThenInclude(transferDetail => transferDetail.Transfer).ThenInclude(transfer => transfer.FloorTo).ThenInclude(floor => floor.Space).ThenInclude(space => space.Area).ThenInclude(area => area.Storage)
                    .Include(orderDetail => orderDetail.OrderDetailServiceMaps).ThenInclude(orderDetailService => orderDetailService.Service)
                    .Include(orderDetail => orderDetail.Order).ThenInclude(order => order.Customer)
                    .Include(orderDetail => orderDetail.Images).ToListAsync();

                var orderDetailHaveTransfer = orderDetails.Where(orderDetail => orderDetail.TransferDetails.Count > 0).ToList();
                var orderDetailDontHaveTransfer = orderDetails.Where(orderDetail => orderDetail.TransferDetails.Count == 0).ToList();
                List<OrderDetailInFloorViewModel> orderDetailToAdd = new List<OrderDetailInFloorViewModel>();

                if (orderDetailDontHaveTransfer.Count > 0)
                    foreach (var orderDetail in orderDetailDontHaveTransfer)
                        if (orderDetail.Import.FloorId == floorId)
                            orderDetailToAdd.Add(_mapper.Map<OrderDetailInFloorViewModel>(orderDetail));


                if (orderDetailHaveTransfer.Count > 0)
                {
                    foreach (var orderDetail in orderDetailHaveTransfer)
                    {
                        Transfer transfer = orderDetail.TransferDetails.OrderByDescending(transferDetail => transferDetail.Transfer.CreatedDate).Select(transferDetail => transferDetail.Transfer).FirstOrDefault();
                        if (transfer.FloorToId == floorId)
                            orderDetailToAdd.Add(_mapper.Map<OrderDetailInFloorViewModel>(orderDetail));
                    }
                }
                result.OrderDetails = orderDetailToAdd;
                if (result.OrderDetails == null) result.OrderDetails = new List<OrderDetailInFloorViewModel>();
                return result;
            }
            catch (ErrorResponse e)
            {
                throw new ErrorResponse((int)e.Error.Code, e.Error.Message);
            }
            catch (Exception ex)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, "" + ex.Message);
            }
        }

        public async Task<bool> CreateNumberOfFloor(Guid spaceId, int numberOfFloor, decimal floorHeight, decimal floorWidth, decimal floorLength, DateTime now)
        {
            try
            {
                if (numberOfFloor <= 0) return false;
                for (int i = 1; i <= numberOfFloor; i++)
                {
                    string name = "Tầng - " + i;
                    if (i == 1) name = "Tầng trệt";
                    Floor floorToCreate = new Floor
                    {
                        SpaceId = spaceId,
                        Name = name,
                        Height = floorHeight,
                        Width = floorWidth,
                        Length = floorLength,
                        IsActive = true,
                        CreatedDate = now
                    };
                    await CreateAsync(floorToCreate);
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<bool> RemoveFloors(Guid spaceId)
        {
            try
            {
                List<Floor> floors = Get(floor => floor.SpaceId == spaceId && floor.IsActive).ToList();
                if (floors.Count == 0) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Không có tầng để xóa");
                foreach (var floor in floors)
                {
                    var floorWithOrderDetail = await GetFloorWithOrderDetail(floor.Id);
                    if (floorWithOrderDetail.OrderDetails.Count > 0) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Không gian đang được sử dụng");
                }

                foreach (var floor in floors)
                {
                    floor.IsActive = false;
                    await UpdateAsync(floor);
                }
                return true;
            }
            catch (ErrorResponse e)
            {
                throw new ErrorResponse((int)e.Error.Code, e.Error.Message);
            }
            catch (Exception ex)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, "" + ex.Message);
            }
        }

        public async Task<List<FloorInSpaceViewModel>> GetFloorInSpace(Guid spaceId, DateTime? date)
        {
            try
            {
                List<FloorInSpaceViewModel> result = new List<FloorInSpaceViewModel>();
                var floors = Get(floor => floor.SpaceId == spaceId && floor.IsActive).ToList();
                if (floors.Count() == 0) return null;

                foreach (Floor floor in floors)
                {
                    var floorUsage = await GetFloorUsage(floor.Id, date);
                    FloorInSpaceViewModel floorToResult = _mapper.Map<FloorInSpaceViewModel>(floor);
                    if (floorUsage != null)
                    {
                        floorToResult.Usage = floorUsage[0];
                        floorToResult.Used = floorUsage[1];
                        floorToResult.Available = floorUsage[2];
                    }

                    result.Add(floorToResult);
                }
                result.Sort();
                return result;
            }
            catch (ErrorResponse e)
            {
                throw new ErrorResponse((int)e.Error.Code, e.Error.Message);
            }
            catch (Exception ex)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, "" + ex.Message);
            }
        }

        public async Task<FloorGetByIdViewModel> GetById(Guid id)
        {
            try
            {
                var floor = await Get(floor => floor.Id == id && floor.IsActive)
                    .Include(floor => floor.Space)
                    .FirstOrDefaultAsync();
                if (floor == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Floor not found");
                var result = await GetFloorWithOrderDetail(id);
                var floorUsage = await GetFloorUsage(id, null);
                if (floorUsage != null)
                {
                    result.Usage = floorUsage.ElementAt(0);
                    result.Used = floorUsage.ElementAt(1);
                    result.Available = floorUsage.ElementAt(2);
                }
                return result;
            }
            catch (ErrorResponse e)
            {
                throw new ErrorResponse((int)e.Error.Code, e.Error.Message);
            }
            catch (Exception ex)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, "" + ex.Message);
            }

        }

        public async Task<List<double>> GetFloorUsage(Guid floorId, DateTime? date)
        {
            try
            {
                // first item is floorUsage
                // second item is floorUsed
                // last item is floorAvailable
                List<double> floorUsage = new List<double>();
                var floor = await GetFloorWithOrderDetail(floorId);
                ICollection<OrderDetailInFloorViewModel> orderDetails = floor.OrderDetails;
                if (date != null) orderDetails = floor.OrderDetails.Where(orderDetail => orderDetail.DeliveryDate <= date && orderDetail.ReturnDate >= date).ToList();
                double floorVolume = (double)(floor.Height * floor.Width * floor.Length);
                if (orderDetails.Count == 0)
                {
                    floorUsage.Add(0);
                    floorUsage.Add(0);
                    floorUsage.Add(floorVolume);
                    return floorUsage;
                }
                double totalVolume = 0;
                foreach (var orderDetail in orderDetails)
                    totalVolume += (double)(orderDetail.Height * orderDetail.Width * orderDetail.Length);


                double usage = totalVolume * 100 / floorVolume;
                floorUsage.Add(Math.Round(usage, 2));
                floorUsage.Add(Math.Round(totalVolume, 2));
                floorUsage.Add(Math.Round(floorVolume - totalVolume, 2));
                return floorUsage;
            }
            catch (Exception ex)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, "" + ex.Message);
            }

        }
        public async Task<List<FloorGetByIdViewModel>> GetBySpaceId(Guid spaceId, DateTime dateFrom, DateTime dateTo, bool isMany, bool isSelfStorage)
        {
            try
            {
                var floors = await Get(floor => floor.SpaceId == spaceId && floor.IsActive).ToListAsync();
                if (isSelfStorage)
                    floors = await Get(floor => floor.SpaceId == spaceId && floor.IsActive && floor.Name == "Tầng trệt").ToListAsync();
                if (isMany)
                    floors = await Get(floor => floor.SpaceId == spaceId && floor.IsActive && floor.Name == "Tầng trệt").ToListAsync();
                if (floors == null) return null;
                if (floors.Count == 0) return null;
                List<FloorGetByIdViewModel> result = new List<FloorGetByIdViewModel>();
                foreach (var floor in floors)
                {
                    var floorWithOrderDetail = await GetFloorWithOrderDetail(floor.Id);
                    var orderDetails = floorWithOrderDetail.OrderDetails.Where(orderDetail => (orderDetail.DeliveryDate.Value.Date <= dateFrom.Date && orderDetail.ReturnDate.Value.Date >= dateFrom.Date) || (dateFrom <= orderDetail.DeliveryDate.Value.Date && dateTo >= orderDetail.DeliveryDate.Value.Date)).ToList();
                    if (isSelfStorage && orderDetails.Count > 0) return null;
                    floorWithOrderDetail.OrderDetails = orderDetails;
                    result.Add(floorWithOrderDetail);
                }
                return result;
            }
            catch (ErrorResponse e)
            {
                throw new ErrorResponse((int)e.Error.Code, e.Error.Message);
            }
            catch (Exception ex)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, "" + ex.Message);
            }

        }

    }
}