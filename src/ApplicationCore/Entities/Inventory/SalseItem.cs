using ApplicationCore.Interfaces;
using Dapper.Contrib.Extensions;

namespace ApplicationCore.Entities.Inventory
{
    [Table("SalseItems")]
    public class SalseItem : BaseEntity
    {
        public int Id { get; set; }
        public int SalseId { get; set; }
        public int ProductId { get; set; } 
        public int VariantId { get; set; } 
        public int UnitId { get; set; }        
        public int Quantity { get; set; } 
        public decimal SalePrice { get; set; } 
        public string VariantName { get; set; }
        [Write(false)]
        public string UnitName { get; set; }
        [Write(false)]
        public string ProductName { get; set; }
    }

    public class SalseItemDTO
    {
        public int Quantity { get; set; }
        public string Variant { get; set; }
        public decimal Price { get; set; }

    }
}







