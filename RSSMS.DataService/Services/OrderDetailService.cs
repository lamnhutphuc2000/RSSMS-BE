using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RSSMS.DataService.Models;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.Responses;
using RSSMS.DataService.UnitOfWorks;
using RSSMS.DataService.ViewModels.OrderDetails;
using System;
using System.Net;
using System.Threading.Tasks;

namespace RSSMS.DataService.Services
{

    public interface IOrderDetailService : IBaseService<OrderDetail>
    {
        Task<OrderDetailViewModel> Update(Guid id, OrderDetailUpdateViewModel model);
    }
    class OrderDetailService : BaseService<OrderDetail>, IOrderDetailService
    {
        private readonly IMapper _mapper;
        public OrderDetailService(IUnitOfWork unitOfWork, IOrderDetailRepository repository, IMapper mapper) : base(unitOfWork, repository)
        {
            _mapper = mapper;
        }

        public async Task<OrderDetailViewModel> Update(Guid id, OrderDetailUpdateViewModel model)
        {
            try
            {
                if (id != model.Id) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Id không khớp");
                var orderDetail = await Get(orderDetail => orderDetail.Id == id).Include(orderDetail => orderDetail.Images)
                                             .Include(orderDetail => orderDetail.OrderDetailServiceMaps).FirstOrDefaultAsync();
                if (orderDetail == null) throw new ErrorResponse((int)HttpStatusCode.BadRequest, "Không tìm thấy chi tiết đơn");
                orderDetail.Status = model.Status;
                await UpdateAsync(orderDetail);
                return _mapper.Map<OrderDetailViewModel>(orderDetail);
            }
            catch (ErrorResponse e)
            {
                throw new ErrorResponse(e.Error.Code, e.Error.Message);
            }
            catch (Exception ex)
            {
                throw new ErrorResponse((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
