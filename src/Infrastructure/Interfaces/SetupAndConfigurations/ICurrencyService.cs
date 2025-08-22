using ApplicationCore.Entities.SetupAndConfigurations;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces.SetupAndConfigurations
{
    public interface ICurrencyService
    {
        Task<Currency> GetAsync(int Id);
        Task<int> SaveAsync(Currency entity);
        Task UpdateCurrencyStatus(int currencyId, bool isChecked);
        Task<List<Currency>> GetAllCategories();
        Task<List<Currency>> GetDataFromDbase(string searchBy, int take, int skip, string sortBy, string sortDir);
    }
}