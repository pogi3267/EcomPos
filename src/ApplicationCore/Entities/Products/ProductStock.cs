using Dapper.Contrib.Extensions;

namespace ApplicationCore.Entities.Products
{
    [Table("ProductStocks")]
    public class ProductStock : BaseEntity
    {
        public ProductStock()
        {

        }
        [Key]
        public int ProductStockId { get; set; }
        public string Variant { get; set; }
        public string SKU { get; set; }
        public decimal Price { get; set; }
        public string Image { get; set; }
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
        public decimal PurchaseQty { get; set; }

        [Write(false)]
        public string FileName { get; set; }
        [Write(false)]
        public int ReturnQuantity { get; set; }
        [Write(false)]
        public int PurchaseQuantity { get; set; }

    }
    public class ProductStockHelper
    {
        public int ProductStockId { get; set; }
        public int Quantity { get; set; }

    }
}
