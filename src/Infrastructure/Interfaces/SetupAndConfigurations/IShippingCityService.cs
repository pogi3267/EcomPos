using ApplicationCore.Entities;
using ApplicationCore.Entities.SetupAndConfigurations;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces.SetupAndConfigurations
{
    public interface IShippingCityService
    {
        Task<City> GetAsync(int Id);
        Task<City> GetCityEntityAsync(int Id);
        Task<City> GetNew();
        Task<List<Select2OptionModel>> GetState(int countryid);
        Task<int> SaveAsync(City entity);
        Task<List<City>> GetDataFromDbase(string searchBy, int take, int skip, string sortBy, string sortDir);
    }
}