using ApplicationCore.Entities.SetupAndConfigurations;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces.SetupAndConfigurations
{
    public interface IVatAndTaxService
    {
        Task<Tax> GetAsync(int Id);
        Task<int> SaveAsync(Tax entity);
        Task<List<Tax>> GetDataFromDbase(string searchBy, int take, int skip, string sortBy, string sortDir);
    }
}
