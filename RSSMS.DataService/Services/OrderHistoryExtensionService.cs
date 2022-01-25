using RSSMS.DataService.Models;
using RSSMS.DataService.Repositories;
using RSSMS.DataService.UnitOfWorks;
using System;
using System.Collections.Generic;
using System.Text;

namespace RSSMS.DataService.Services
{
    public interface IOrderHistoryExtensionService : IBaseService<OrderHistoryExtension>
    {
    }
    public class OrderHistoryExtensionService : BaseService<OrderHistoryExtension>, IOrderHistoryExtensionService
    {
        public OrderHistoryExtensionService(IUnitOfWork unitOfWork, IOrderHistoryExtensionRepository repository) : base(unitOfWork, repository)
        {
        }
    }
}
