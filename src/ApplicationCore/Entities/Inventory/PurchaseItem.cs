using ApplicationCore.Enums;
using Dapper.Contrib.Extensions;

namespace ApplicationCore.Entities.Inventory
{
    [Table("PurchaseItems")]
    public class PurchaseItem
    {
        public int PurchaseItemId { get; set; }
        public int PurchaseId { get; set; }
        public int ProductId { get; set; }
        public int? VariantId { get; set; }
        public int Quantity { get; set; }
        public int UnitId { get; set; }
        public decimal Price { get; set; }
        public int BranchId { get; set; }
        public decimal TotalPrice { get; set; }

        [Write(false)]
        public string Variant { get; set; }

        [Write(false)]
        public decimal ProductName { get; set; }

        [Write(false)]
        public int BranchName { get; set; }

        [Write(false)]
        public int UnitName { get; set; }

        [Write(false)]
        public EntityState EntityState { get; set; }

    }

}





