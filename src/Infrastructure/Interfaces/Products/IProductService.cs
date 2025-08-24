using ApplicationCore.DTOs;
using ApplicationCore.Entities;
using ApplicationCore.Entities.Marketing;
using ApplicationCore.Entities.Products;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces.Products
{
    public interface IProductService
    {
        Task<List<Product>> GetAll();
        Task<Product> GetInitial();
        Task<Product> GetAsync(int Id);
        Task<Product> GetProductAsync(int Id);
        Task<Product> GetProductDiscountAsync(int Id);
        Task<Product> GetViewProductByIdAsync(int Id);
        Task<int> SaveAsync(Product entity, List<ProductVariant> productVariant, ProductTax productTax, FlashDealProducts flashDealProducts);
        Task<int> UpdateAsync(Product entity);
        Task<int> UpdateProductAsync(Product entity);
        Task<List<Product>> GetDataFromDbase(string searchBy, int take, int skip, string sortBy, string sortDir, string status);
        Task<List<AttributeValue>> GetAttribute(string attributeList);
        Task<List<Product>> GetFlashDealProductsAsync(int flashDealId);
        Task<List<Product>> GetFlashDealProductAsync(int flashDealId, int productId);
        Task<List<Product>> GetProductsToAddAsync();
        Task<List<ProductStock>> GetVariantByProductAsync(int productId);
        Task<List<Select2OptionModel>> GetProductsAsync();
        Task DeleteFlashDealProductAsync(int flashDealId, int productId);
        Task DeleteUploadImg(string ids);
        Task<int> SaveFlashDealProductAsync(FlashDealProducts entity);

        // Barcode functionality methods

        Task<List<object>> GetProductsWithVariantsForBarcode();
        Task<List<object>> SearchProductsWithVariantsForBarcode(string searchTerm);
        Task<object> GetProductWithVariantsForBarcode(int productId);
        Task<BarcodeResult> GenerateBarcode(BarcodeRequestModel request);
        Task<byte[]> GenerateBarcodeImage(string barcodeText, string barcodeType, int width, int height);
        Task<byte[]> GenerateBarcodePDF(List<ApplicationCore.DTOs.BarcodeItemModel> barcodeItems, ApplicationCore.DTOs.BarcodeSettingsModel settings);
    }

    public class BarcodeRequestModel
    {
        public List<BarcodeProductModel> Products { get; set; }
        public BarcodePrintOptions PrintOptions { get; set; }
        public string PaperSize { get; set; }
        public string BarcodeType { get; set; }
        public int BarcodeWidth { get; set; }
        public int BarcodeHeight { get; set; }
        public bool ShowProductName { get; set; }
        public bool ShowPrice { get; set; }
        public bool ShowCompanyName { get; set; }
        public string CompanyName { get; set; }
    }

    public class BarcodeProductModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Variant { get; set; }
        public int? VariantId { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string Barcode { get; set; }
        public int ProductId { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class BarcodePrintOptions
    {
        public bool ProductName { get; set; }
        public bool Price { get; set; }
        public bool PromotionalPrice { get; set; }
        public bool CompanyName { get; set; }
    }

    public class BarcodeResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string PrintUrl { get; set; }
        public string DownloadUrl { get; set; }
        public byte[] BarcodeData { get; set; }
    }
}