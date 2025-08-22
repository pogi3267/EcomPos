using ApplicationCore.Entities.SetupAndConfigurations;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces.SetupAndConfigurations
{
    public interface IGeneralSettings
    {
        Task<GeneralSettings> GetAsync();
        Task<int> SaveAsync(GeneralSettings entity);
    }
}