using ApplicationCore.Interfaces;
using Dapper.Contrib.Extensions;

namespace ApplicationCore.Entities.Inventory
{
    [Table("PurchaseItems")]
    public class PurchaseItem : BaseEntity
    {
        public int Id { get; set; }
        public int PurchaseId { get; set; }
        public string VariantName { get; set; }
        public decimal VariantPrice { get; set; }
        public int Quantity { get; set; }
        [Write(false)]
        public string Variant { get; set; }
        [Write(false)]
        public decimal Price { get; set; }
        public int BranchId { get; set; } = 1;
      
    }

    public class PurchaseItemDTO
    {
        public int Quantity { get; set; }
        public string Variant { get; set; }
        public decimal Price { get; set; }

    }
}





