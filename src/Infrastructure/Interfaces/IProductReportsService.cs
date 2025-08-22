using ApplicationCore.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public interface IProductReportsService
    {
        Task<ReportsDTO> GetAsync();
        Task<List<ReportsDTO>> GetInhouseProduct(ReportsDTO reportsDTO);
        Task<List<ReportsDTO>> GetProductSales(ReportsDTO reportsDTO);
        Task<List<ReportsDTO>> GetProductStock(ReportsDTO reportsDTO);
        Task<List<ReportsDTO>> GetProductWishList(ReportsDTO reportsDTO);
        Task<List<ReportsDTO>> GetUserSearchList(ReportsDTO reportsDTO);
        Task<InvoiceOrderDTO> GetInvoiceAsync(int id);

    }
}

