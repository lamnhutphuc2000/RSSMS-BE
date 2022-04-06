﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Models;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.Responses;
using RSSMS.DataService.UnitOfWorks;
using RSSMS.DataService.ViewModels.Floors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RSSMS.DataService.Services
{
    public interface IFloorService : IBaseService<Floor>
    {
        Task<bool> CreateNumberOfFloor(Guid spaceId, int numberOfFloor, decimal floorHeight, decimal floorWidth, decimal floorLength, DateTime now);
        Task<bool> RemoveFloors(Guid spaceId);
        Task<List<FloorInSpaceViewModel>> GetFloorInSpace(Guid spaceId, DateTime? date);
        Task<FloorGetByIdViewModel> GetById(Guid id);
        Task<List<FloorGetByIdViewModel>> GetBySpaceId(Guid spaceId, DateTime date, bool isMany, bool isSelfStorage);
    }
    public class FloorService : BaseService<Floor>, IFloorService

    {
        private readonly IMapper _mapper;
        public FloorService(IUnitOfWork unitOfWork, IFloorRepository repository, IMapper mapper) : base(unitOfWork, repository)
        {
            _mapper = mapper;
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
                if(floors.Any(floor => floor.OrderDetails.Count > 0)) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Floor is in used");
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
                var floors = Get(floor => floor.SpaceId == spaceId && floor.IsActive)
                    .Include(floor => floor.OrderDetails).ThenInclude(orderDetail => orderDetail.OrderDetailServiceMaps).ThenInclude(orderDetailServiceMaps => orderDetailServiceMaps.Service)
                    .Include(floor => floor.OrderDetails).ThenInclude(orderDetail => orderDetail.Floor).ThenInclude(floor => floor.Space).ThenInclude(space => space.Area).ThenInclude(area => area.Storage)
                    .Include(floor => floor.OrderDetails).ThenInclude(orderDetail => orderDetail.Order).ToList();
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
                .Include(floor => floor.Space).ThenInclude(space => space.Area).ThenInclude(area => area.Storage)
                .Include(floor => floor.OrderDetails).ThenInclude(orderDetail => orderDetail.OrderDetailServiceMaps).ThenInclude(serviceMap => serviceMap.Service)
                .Include(floor => floor.OrderDetails).ThenInclude(orderDetail => orderDetail.Order).ThenInclude(order => order.Customer)
                .Include(floor => floor.OrderDetails).ThenInclude(orderDetail => orderDetail.Images)
                .FirstOrDefaultAsync();
                if (floor == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Floor not found");
                var result = _mapper.Map<FloorGetByIdViewModel>(floor);
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
                var floor = await Get(floor => floor.Id == floorId && floor.IsActive).Include(floor => floor.OrderDetails)
                    .ThenInclude(orderDetail => orderDetail.OrderDetailServiceMaps)
                    .ThenInclude(orderDetailServiceMaps => orderDetailServiceMaps.Service)
                    .Include(floor => floor.OrderDetails).ThenInclude(orderDetail => orderDetail.Order).FirstOrDefaultAsync();
                ICollection<OrderDetail> orderDetails = floor.OrderDetails;
                if (date != null) orderDetails = floor.OrderDetails.Where(orderDetail => orderDetail.Order.DeliveryDate <= date && orderDetail.Order.ReturnDate >= date).ToList();
                double floorVolume = (double)(floor.Height * floor.Width * floor.Length);
                if (orderDetails.Count <= 0)
                {
                    floorUsage.Add(0);
                    floorUsage.Add(0);
                    floorUsage.Add(floorVolume);
                }
                var totalHeight = orderDetails.Select(orderDetail => orderDetail.Height).ToList().Sum();
                var totalWidth = orderDetails.Select(orderDetail => orderDetail.Width).ToList().Sum();
                var totalLength = orderDetails.Select(orderDetail => orderDetail.Length).ToList().Sum();
                double totalVolume = (double)(totalHeight * totalWidth * totalLength);
                
                double usage = totalVolume * 100 / floorVolume;
                floorUsage.Add(usage);
                floorUsage.Add(totalVolume);
                floorUsage.Add(floorVolume - totalVolume);
                return floorUsage;
            }
            catch(Exception ex)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, "" + ex.Message);
            }
            
        }
        public async Task<List<FloorGetByIdViewModel>> GetBySpaceId(Guid spaceId, DateTime date, bool isMany, bool isSelfStorage)
        {
            try
            {
                var floors = await Get(floor => floor.SpaceId == spaceId && floor.IsActive)
                .Include(floor => floor.Space).ThenInclude(space => space.Area).ThenInclude(area => area.Storage)
                .Include(floor => floor.OrderDetails).ThenInclude(orderDetail => orderDetail.OrderDetailServiceMaps).ThenInclude(serviceMap => serviceMap.Service)
                .Include(floor => floor.OrderDetails).ThenInclude(orderDetail => orderDetail.Order).ThenInclude(order => order.Customer)
                .Include(floor => floor.OrderDetails).ThenInclude(orderDetail => orderDetail.Images)
                .ToListAsync();
                if(isSelfStorage)
                {
                    floors = await Get(floor => floor.SpaceId == spaceId && floor.IsActive && floor.Name == "Tầng trệt")
                        .Include(floor => floor.Space).ThenInclude(space => space.Area).ThenInclude(area => area.Storage)
                        .Include(floor => floor.OrderDetails).ThenInclude(orderDetail => orderDetail.OrderDetailServiceMaps).ThenInclude(serviceMap => serviceMap.Service)
                        .Include(floor => floor.OrderDetails).ThenInclude(orderDetail => orderDetail.Order).ThenInclude(order => order.Customer)
                        .Include(floor => floor.OrderDetails).ThenInclude(orderDetail => orderDetail.Images)
                        .ToListAsync();
                    //var tmp = floors.Where(floor => floor.OrderDetails.Count == 0).ToList();
                    //var floorList = floors.Where(floor => floor.OrderDetails.Count > 0);
                    //floors = tmp;
                    //foreach(var floor in floorList)
                    //{
                    //    var orderDetails = floor.OrderDetails.Where(orderDetail => orderDetail.Order.DeliveryDate <= date && orderDetail.Order.ReturnDate >= date).ToList();

                    //    floors.Add(floor);
                    //}
                }
                if(isMany)
                {
                    floors = await Get(floor => floor.SpaceId == spaceId && floor.IsActive && floor.Name == "Tầng trệt")
                        .Include(floor => floor.Space).ThenInclude(space => space.Area).ThenInclude(area => area.Storage)
                        .Include(floor => floor.OrderDetails).ThenInclude(orderDetail => orderDetail.OrderDetailServiceMaps).ThenInclude(serviceMap => serviceMap.Service)
                        .Include(floor => floor.OrderDetails).ThenInclude(orderDetail => orderDetail.Order).ThenInclude(order => order.Customer)
                        .Include(floor => floor.OrderDetails).ThenInclude(orderDetail => orderDetail.Images)
                        .ToListAsync();
                }
                if (floors == null) return null;

                foreach(var floor in floors)
                {
                    var orderDetails = floor.OrderDetails.Where(orderDetail => orderDetail.Order.DeliveryDate <= date && orderDetail.Order.ReturnDate >= date).ToList();
                    if (isSelfStorage && orderDetails.Count == 1) return null;
                    floor.OrderDetails = orderDetails;
                }


                var result = floors.AsQueryable().ProjectTo<FloorGetByIdViewModel>(_mapper.ConfigurationProvider).ToList();
                
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
