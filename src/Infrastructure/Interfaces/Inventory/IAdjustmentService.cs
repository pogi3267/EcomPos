using ApplicationCore.Entities.Inventory;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace Infrastructure.Interfaces.Inventory
{
    public interface IAdjustmentService
    {
        Task<List<Adjustment>> GetListAsync(string searchBy, int take, int skip, string sortBy, string sortDir);
        Task<Adjustment> GetAsync(int id);
        Task<int> SaveAsync(Adjustment entity);
    }
}