using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Models;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.Responses;
using RSSMS.DataService.UnitOfWorks;
using RSSMS.DataService.Utilities;
using RSSMS.DataService.ViewModels.Images;
using RSSMS.DataService.ViewModels.Products;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RSSMS.DataService.Services
{
    public interface IProductService : IBaseService<Product>
    {
        Task<Dictionary<string, List<ProductViewAllModel>>> GetAll(ProductViewAllModel model);
        Task<ProductViewAllModel> GetById(int id);
        Task<ProductViewModel> Create(ProductCreateViewModel model);
        Task<ProductUpdateViewModel> Update(int id, ProductUpdateViewModel model);
        Task<ProductViewAllModel> Delete(int id);
    }
    public class ProductService : BaseService<Product>, IProductService
    {
        private readonly IMapper _mapper;
        private readonly IFirebaseService _firebaseService;
        public ProductService(IUnitOfWork unitOfWork, IProductRepository repository, IMapper mapper, IFirebaseService firebaseService) : base(unitOfWork, repository)
        {
            _mapper = mapper;
            _firebaseService = firebaseService;
        }

        public async Task<ProductViewModel> Create(ProductCreateViewModel model)
        {
            var product = _mapper.Map<Product>(model);
            var images = model.Images;
            product.Images = null;
            await CreateAsync(product);

            
            foreach (var avatar in images)
            {
                var url = await _firebaseService.UploadImageToFirebase(avatar.File, "products", product.Id, "avatar");
                if (url != null) avatar.Url = url;
            }
            product.Images = images.AsQueryable().ProjectTo<Image>(_mapper.ConfigurationProvider).ToList();
            await UpdateAsync(product);

            return _mapper.Map<ProductViewModel>(product);
        }

        public async Task<ProductViewAllModel> Delete(int id)
        {
            var entity = await GetAsync<int>(id);
            if (entity == null) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Product id not found");
            entity.IsActive = false;
            await UpdateAsync(entity);
            return _mapper.Map<ProductViewAllModel>(entity);
        }

        public async Task<Dictionary<string, List<ProductViewAllModel>>> GetAll(ProductViewAllModel model)
        {
            var products = Get(x => x.IsActive == true).OrderBy(x => x.Type)
                    .ProjectTo<ProductViewAllModel>(_mapper.ConfigurationProvider)
                    .DynamicFilter(model);
            if (products.ToList().Count == 0) throw new ErrorResponse((int)HttpStatusCode.NotFound, "Products not found");


            Dictionary<string, List<ProductViewAllModel>> result = new Dictionary<string, List<ProductViewAllModel>>();
            var distinctProductList = products.ToList()
                .GroupBy(m => new { m.Type })
                .Select(group => group.First())
                .ToList();
            foreach (var distinctProduct in distinctProductList)
            {
                result.Add(distinctProduct.Type.ToString(), products.Where(x => x.Type == distinctProduct.Type).ToList());
            }
            return result;
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
            updateEntity.IsActive = false;
            updateEntity.Images = null;
            await UpdateAsync(updateEntity);

            updateEntity.Id = 0;
            updateEntity.IsActive = true;
            await CreateAsync(updateEntity);

            List<AvatarImageViewModel> images = model.Images.ToList();
            foreach (var avatar in images)
            {
                var url = await _firebaseService.UploadImageToFirebase(avatar.File, "products", updateEntity.Id, "avatar");
                if (url != null)
                {
                    avatar.Id = 0;
                    avatar.Url = url;
                }
            }
            updateEntity.Images = images.AsQueryable().ProjectTo<Image>(_mapper.ConfigurationProvider).ToList();

            await UpdateAsync(updateEntity);

            return _mapper.Map<ProductUpdateViewModel>(updateEntity);
        }
    }
}
