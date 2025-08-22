using ApplicationCore.Entities.Orders;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces.Marketing
{
    public interface IOrderListService
    {
        Task<List<Orders>> GetList(string searchBy, int take, int skip, string sortBy, string sortDir, string status, bool isPickupOrder);
        Task<Orders> GetAsync(int Id);
        Task SaveAsync(Orders model);
        Task SaveDetailAsync(OrderDetail model);
        Task DeleteNotificationAsync();
    }
}