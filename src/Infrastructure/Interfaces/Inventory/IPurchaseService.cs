using ApplicationCore.Entities.Inventory;
using ApplicationCore.Entities.Products;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces.Inventory
{
    public interface IPurchaseService
    {
        Task<List<Purchase>> GetDataFromDbase(string searchBy, int take, int skip, string sortBy, string sortDir, string status);

        Task<Purchase> GetInitial();
    }
}
