using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using Dapper.Contrib.Extensions;

namespace ApplicationCore.Entities.Inventory
{
    [Table("PurchaseItems")]
    public class PurchaseItem : IBaseEntity
    {
        [Key]
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
        public string VariantName { get; set; }

        [Write(false)]
        public string ProductName { get; set; }

        [Write(false)]
        public string BranchName { get; set; }

        [Write(false)]
        public string Unit { get; set; }

        [Write(false)]
        public EntityState EntityState { get; set; }

    }

}





