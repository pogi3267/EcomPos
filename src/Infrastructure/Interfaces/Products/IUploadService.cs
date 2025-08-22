using ApplicationCore.Entities.Products;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces.Products
{
    public interface IUploadService
    {
        Task<int> SaveAllAsync(List<Upload> entity);
        List<Upload> GetAll();
    }
}
