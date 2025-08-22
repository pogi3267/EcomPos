using ApplicationCore.Entities.SetupAndConfigurations;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces.SetupAndConfigurations
{
    public interface IBusinessSettingService
    {
        Task<BusinessSetting> GetAsync(string type);
        Task<List<BusinessSetting>> GetsAsync(string types);
        Task<int> SaveAsync(BusinessSetting entity);
        Task SaveMultipleAsync(List<BusinessSetting> entities);
        Task<BusinessSetting> GetsImagesAsync();
    }
}
