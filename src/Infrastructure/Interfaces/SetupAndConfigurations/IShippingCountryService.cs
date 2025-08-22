using ApplicationCore.Entities.SetupAndConfigurations;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces.SetupAndConfigurations
{
    public interface IShippingCountryService
    {
        Task<Country> GetAsync(int Id);
        Task<int> SaveAsync(Country entity);
        Task<List<Country>> GetDataFromDbase(string searchBy, int take, int skip, string sortBy, string sortDir);

    }
}

