using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Constants;
using RSSMS.DataService.Models;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.Responses;
using RSSMS.DataService.UnitOfWorks;
using RSSMS.DataService.Utilities;
using RSSMS.DataService.ViewModels.Products;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RSSMS.DataService.Services
{
    public interface IProductService : IBaseService<Product>
    {
        Task<DynamicModelResponse<ProductViewAllModel>> GetAll(ProductViewAllModel model, string[] fields, int page, int size);
        Task<ProductViewAllModel> GetById(int id);
        Task<ProductCreateViewModel> Create(ProductCreateViewModel model);
        Task<ProductUpdateViewModel> Update(int id, ProductUpdateViewModel model);
        Task<ProductViewAllModel> Delete(int id);
    }
    public class ProductService : BaseService<Product>, IProductService
    {
        private readonly IMapper _mapper;
        private readonly IProductRepository _productRepository;
        public ProductService(IUnitOfWork unitOfWork, IProductRepository repository,  IMapper mapper) : base(unitOfWork, repository)
        {
            _mapper = mapper;
            _productRepository = repository;
        }

        public async Task<ProductCreateViewModel> Create(ProductCreateViewModel model)
        {
            var storage = _mapper.Map<Product>(model);
            await CreateAsync(storage);
            return _mapper.Map<ProductCreateViewModel>(storage);
        }

        public async Task<ProductViewAllModel> Delete(int id)
        {
            var entity = await GetAsync<int>(id);
            if (entity == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Product id not found");
            entity.IsActive = false;
            await UpdateAsync(entity);
            return _mapper.Map<ProductViewAllModel>(entity);
        }

        public async Task<DynamicModelResponse<ProductViewAllModel>> GetAll(ProductViewAllModel model, string[] fields, int page, int size)
        {
            var products = Get(x => x.IsActive == true)
                    .ProjectTo<ProductViewAllModel>(_mapper.ConfigurationProvider)
                    .PagingIQueryable(page, size, CommonConstant.LimitPaging, CommonConstant.DefaultPaging);


            if (products.Item2.ToList().Count < 1) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Can not found");
           

            var rs = new DynamicModelResponse<ProductViewAllModel>
            {
                Metadata = new PagingMetaData
                {
                    Page = page,
                    Size = size,
                    Total = products.Item1,
                    TotalPage = (int)Math.Ceiling((double)products.Item1 / size)
                },
                Data = products.Item2.ToList()
            };
           return rs;
        }

        public async Task<ProductViewAllModel> GetById(int id)
        {
            var result = await Get(x => x.Id == id && x.IsActive == true)
                .ProjectTo<ProductViewAllModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
            if (result == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Product id not found");
            return result;
        }

        public async Task<ProductUpdateViewModel> Update(int id, ProductUpdateViewModel model)
        {
            if (id != model.Id) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Product Id not matched");

            var entity = await GetAsync(id);
            if (entity == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Product not found");

            var updateEntity = _mapper.Map(model, entity);
            await UpdateAsync(updateEntity);

            return _mapper.Map<ProductUpdateViewModel>(updateEntity);
        }
    }
}
