using ApplicationCore.Entities.Products;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces.Products
{
    public interface IAttributeService
    {
        Task<ProductAttribute> GetAsync(int Id);
        Task<ProductAttribute> SaveAsync(ProductAttribute entity);
        Task<List<ProductAttribute>> GetDataFromDbase(string searchBy, int take, int skip, string sortBy, string sortDir);
        Task<List<AttributeValue>> GetAttributeValues(int attributeId);
    }
}
