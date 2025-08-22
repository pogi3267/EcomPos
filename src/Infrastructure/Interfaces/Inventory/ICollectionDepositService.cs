using ApplicationCore.Entities.Inventory;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces.Inventory
{
    public interface ICollectionDepositService
    {
        Task<Collection> GetInitial();
        Task<List<Collection>> GetListAsync(string searchBy, int take, int skip, string sortBy, string sortDir);
        Task<Collection> GetAsync(int id);
        Task<int> SaveAsync(Collection entity);
    }
}
