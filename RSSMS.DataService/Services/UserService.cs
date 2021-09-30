﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Constants;
using RSSMS.DataService.Models;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.Responses;
using RSSMS.DataService.UnitOfWorks;
using RSSMS.DataService.Utilities;
using RSSMS.DataService.ViewModels;
using RSSMS.DataService.ViewModels.Users;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RSSMS.DataService.Services
{
    public interface IUserService : IBaseService<User>
    {
        Task<DynamicModelResponse<UserViewModel>> GetAll(UserViewModel model, string[] fields, int page, int size);
        Task<UserViewModel> GetById(int id);
        Task<UserCreateViewModel> Create(UserCreateViewModel model);
        Task<UserViewModel> Update(int id, UserUpdateViewModel model);
        Task<UserViewModel> Delete(int id);
        Task<int> Count(List<UserViewModel> shelves);
    }
    public class UserService : BaseService<User>, IUserService
    {
        private readonly IMapper _mapper;
        public UserService(IUnitOfWork unitOfWork, IUserRepository repository, IMapper mapper) : base(unitOfWork, repository)
        {
            _mapper = mapper;
        }

        public Task<int> Count(List<UserViewModel> shelves)
        {
            return Count();
        }

        public async Task<UserCreateViewModel> Create(UserCreateViewModel model)
        {
            var user = _mapper.Map<User>(model);
            await CreateAsync(user);
            return model;
        }

        public async Task<UserViewModel> Delete(int id)
        {
            var entity = await GetAsync<int>(id);
            if (entity == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "User id not found");
            entity.IsActive = false;
            await UpdateAsync(entity);
            return _mapper.Map<UserViewModel>(entity);
        }

        public async Task<DynamicModelResponse<UserViewModel>> GetAll(UserViewModel model, string[] fields, int page, int size)
        {
            var users = Get(x => x.IsActive == true).ProjectTo<UserViewModel>(_mapper.ConfigurationProvider)
                .DynamicFilter(model)
                .PagingIQueryable(page, size, CommonConstant.LimitPaging, CommonConstant.DefaultPaging);
            if (users.Item2.ToList().Count < 1) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Can not found");
            var rs = new DynamicModelResponse<UserViewModel>
            {
                Metadata = new PagingMetaData
                {
                    Page = page,
                    Size = size,
                    Total = users.Item1
                },
                Data = users.Item2.ToList()
            };
            return rs;
        }

        public async Task<UserViewModel> GetById(int id)
        {
            var result = await Get(x => x.Id == id && x.IsActive == true).ProjectTo<UserViewModel>(_mapper.ConfigurationProvider).FirstOrDefaultAsync();
            if (result == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "User id not found");
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
    }

}
