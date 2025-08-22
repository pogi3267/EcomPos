using ApplicationCore.Entities.Inventory;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces.Inventory
{
    public interface IPaymentFinalizedService
    {
        Task<List<Payment>> GetListAsync(string searchBy, int take, int skip, string sortBy, string sortDir);
        Task<Payment> GetAsync(int id);
        Task<int> SaveAsync(Payment entity);
    }
}


