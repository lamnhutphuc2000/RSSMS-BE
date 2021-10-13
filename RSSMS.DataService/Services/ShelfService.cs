﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Constants;
using RSSMS.DataService.Models;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.Responses;
using RSSMS.DataService.UnitOfWorks;
using RSSMS.DataService.Utilities;
using RSSMS.DataService.ViewModels.Shelves;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RSSMS.DataService.Services
{
    public interface IShelfService : IBaseService<Shelf>
    {
        Task<DynamicModelResponse<ShelfViewModel>> GetAll(ShelfViewModel model, string[] fields, int page, int size);
        Task<ShelfViewModel> GetById(int id);
        Task<ShelfViewModel> Create(ShelfCreateViewModel model);
        Task<ShelfViewModel> Delete(int id);
        Task<ShelfViewModel> Update(int id, ShelfUpdateViewModel model);
        Dictionary<string, double> GetBoxUsageByAreaId(int areaId);
    }
    public class ShelfService : BaseService<Shelf>, IShelfService

    {
        private readonly IMapper _mapper;
        private readonly IBoxService _boxService;
        public ShelfService(IUnitOfWork unitOfWork, IBoxService boxService, IShelfRepository repository, IMapper mapper) : base(unitOfWork, repository)
        {
            _mapper = mapper;
            _boxService = boxService;
        }

        public async Task<ShelfViewModel> Create(ShelfCreateViewModel model)
        {
            var shelf = Get(x => x.Name == model.Name && x.AreaId == model.AreaId && x.IsActive == true).FirstOrDefault();
            if (shelf != null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Shelf name is existed");
            var shelfToCreate = _mapper.Map<Shelf>(model);
            await CreateAsync(shelfToCreate);
            int numberOfShelve = model.BoxesInHeight * model.BoxesInWidth;
            await _boxService.CreateNumberOfBoxes(shelfToCreate.Id, numberOfShelve, model.BoxSize);
            return _mapper.Map<ShelfViewModel>(shelfToCreate);
        }

        public async Task<ShelfViewModel> Delete(int id)
        {
            var entity = await GetAsync<int>(id);
            if (entity == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Shelf id not found");
            entity.IsActive = false;
            await UpdateAsync(entity);
            return _mapper.Map<ShelfViewModel>(entity);
        }

        public async Task<ShelfViewModel> GetById(int id)
        {
            var shelf = await GetAsync(id);
            if (shelf == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Shelf id not found");
            var result = _mapper.Map<ShelfViewModel>(shelf);
            return result;
        }

        public async Task<ShelfViewModel> Update(int id, ShelfUpdateViewModel model)
        {
            if (id != model.Id) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Shelf Id not matched");

            var entity = await GetAsync(id);
            var shelf = Get(x => x.Name == model.Name && x.AreaId == entity.AreaId && x.Id != id && x.IsActive == true).FirstOrDefault();
            if (shelf != null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Shelf name is existed");
            if (entity == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Shelf not found");

            var updateEntity = _mapper.Map(model, entity);
            await UpdateAsync(updateEntity);

            return _mapper.Map<ShelfViewModel>(updateEntity);
        }
        public async Task<DynamicModelResponse<ShelfViewModel>> GetAll(ShelfViewModel model, string[] fields, int page, int size)
        {
            var shelves = Get(x => x.IsActive == true).ProjectTo<ShelfViewModel>(_mapper.ConfigurationProvider)
                .DynamicFilter(model)
                .PagingIQueryable(page, size, CommonConstant.LimitPaging, CommonConstant.DefaultPaging);
            var rs = new DynamicModelResponse<ShelfViewModel>
            {
                Metadata = new PagingMetaData
                {
                    Page = page,
                    Size = size,
                    Total = shelves.Item1,
                    TotalPage = (int)Math.Ceiling((double)shelves.Item1 / size)
                },
                Data = shelves.Item2.ToList()
            };
            return rs;
        }

        public Dictionary<string, double> GetBoxUsageByAreaId(int areaId)
        {
            var shelves = Get(x => x.AreaId == areaId && x.IsActive == true).Include(x => x.Boxes).ToList();

            double totalBox = 0;
            double boxRemaining = 0;
            double usage = 0;
            var result = new Dictionary<string, double>();
            result["Total"] = totalBox;
            result["BoxRemaining"] = boxRemaining;
            result["Usage"] = usage;
            if (shelves == null)
            {
                return result;
            }
            foreach (var shelf in shelves)
            {
                if (shelf.Boxes != null)
                {
                    var boxes = shelf.Boxes;
                    totalBox += boxes.Count;

                    var boxesNotUsed = boxes.Where(x => x.Status == 0).ToList().Count;
                    boxRemaining += boxesNotUsed;
                }
            }

            if (totalBox - boxRemaining != 0)
            {
                usage = Math.Ceiling((totalBox - boxRemaining) / totalBox * 100);
            }


            result["Total"] = totalBox;
            result["BoxRemaining"] = boxRemaining;
            result["Usage"] = usage;
            return result;
        }
    }
}
