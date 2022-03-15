using AutoMapper;
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
using System.Text;
using System.Threading.Tasks;

namespace RSSMS.DataService.Services
{
    public interface IFloorsService : IBaseService<Floor>
    {
        Task<bool> CreateNumberOfFloor(Guid spaceId, int numberOfFloor, decimal floorHeight, decimal floorWidth, decimal floorLength, DateTime now);
        Task<bool> RemoveFloors(Guid spaceId);
        List<FloorInSpaceViewModel> GetFloorInSpace(Guid spaceId);
    }
    public class FloorsService : BaseService<Floor>, IFloorsService

    {
        private readonly IMapper _mapper;
        public FloorsService(IUnitOfWork unitOfWork, IFloorsRepository repository, IServicesService servicesService, IMapper mapper) : base(unitOfWork, repository)
        {
            _mapper = mapper;
        }

        public async Task<bool> CreateNumberOfFloor(Guid spaceId, int numberOfFloor, decimal floorHeight, decimal floorWidth, decimal floorLength, DateTime now)
        {
            if (numberOfFloor <= 0) return false;
            try
            {
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
            }
            catch(Exception ex)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, "" + ex.Message);
            }
            return true;
        }

        public async Task<bool> RemoveFloors(Guid spaceId)
        {
            try
            {
                var floors = Get(floor => floor.SpaceId == spaceId && floor.IsActive == true).ToList();
                foreach(var floor in floors)
                {
                    floor.IsActive = false;
                    await UpdateAsync(floor);
                }
            }
            catch (Exception ex)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, "" + ex.Message);
            }
            return true;
        }

        public List<FloorInSpaceViewModel> GetFloorInSpace(Guid spaceId)
        {
            double usage = 0;
            List<FloorInSpaceViewModel> result = new List<FloorInSpaceViewModel>();
            try
            {
                var floors = Get(floor => floor.SpaceId == spaceId && floor.IsActive).Include(floor => floor.OrderDetails)
                .ThenInclude(orderDetail => orderDetail.OrderDetailServiceMaps)
                .ThenInclude(orderDetailServiceMaps => orderDetailServiceMaps.Service);
                if (floors.ToList().Count() == 0) return null;

                foreach (var floor in floors)
                {
                    if (floor.OrderDetails.Count == 0)
                        result.Add(_mapper.Map<FloorInSpaceViewModel>(floor));
                    else
                    {
                        var orderDetails = floor.OrderDetails;
                        var totalHeight = orderDetails.Select(orderDetail => orderDetail.Height).ToList().Sum();
                        var totalWidth = orderDetails.Select(orderDetail => orderDetail.Width).ToList().Sum();
                        var totalLength = orderDetails.Select(orderDetail => orderDetail.Length).ToList().Sum();
                        double totalVolume = (double)(totalHeight * totalWidth * totalLength);
                        double floorVolume = (double)(floor.Height * floor.Width * floor.Length);
                        usage = totalVolume * 100 / floorVolume;
                        FloorInSpaceViewModel floorToResult = _mapper.Map<FloorInSpaceViewModel>(floor);
                        floorToResult.Usage = usage;
                        result.Add(floorToResult);
                    }
                }
            }
            catch(Exception ex)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, "" + ex.Message);
            }
            result.Sort();
            return result;
        }
    }
}
