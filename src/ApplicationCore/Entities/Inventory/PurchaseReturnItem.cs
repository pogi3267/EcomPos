using ApplicationCore.Interfaces;
using Dapper.Contrib.Extensions;

namespace ApplicationCore.Entities.Inventory
{
    [Table("PurchaseReturnItems")]
    public class PurchaseReturnItem : BaseEntity
    {
        public int Id { get; set; }
        public int PurchaseId { get; set; }
        public string VariantName { get; set; }
        public decimal VariantPrice { get; set; }
        public int Quantity { get; set; }
        public int PurchaseQuantity { get; set; }
        [Write(false)]
        public string Variant { get; set; }
        [Write(false)]
        public int ReturnQuantity { get; set; }
        [Write(false)]
        public decimal Price { get; set; }
      
    }

}







