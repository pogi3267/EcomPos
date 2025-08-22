using ApplicationCore.Entities.Marketing;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces.Marketing
{
    public interface IFlashDealService
    {
        Task<List<FlashDeal>> GetAll();
        Task<List<FlashDeal>> GetActiveFlashDEals();
        Task<FlashDeal> GetAsync(int Id);
        Task<int> SaveAsync(FlashDeal entity);
        Task<List<FlashDeal>> GetDataFromDbase(string searchBy, int take, int skip, string sortBy, string sortDir);
    }
}