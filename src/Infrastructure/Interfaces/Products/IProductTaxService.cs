using ApplicationCore.Entities.Products;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces.Products
{
    public interface IProductTaxService
    {
        Task<int> SaveSingleAsync(ProductTax entity);
    }
}
