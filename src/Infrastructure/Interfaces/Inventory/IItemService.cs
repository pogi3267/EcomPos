using System.Collections.Generic;
using System.Threading.Tasks;
using ApplicationCore.Entities.Inventory;

namespace Infrastructure.Interfaces.Inventory
{
    public interface IItemService
    {
        Task<List<Item>> GetListAsync(string searchBy, int take, int skip, string sortBy, string sortDir);
        Task<Item> GetAsync(int id);
        Task<int> SaveAsync(Item entity);
    }
}