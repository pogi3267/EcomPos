using ApplicationCore.Interfaces;
using Dapper.Contrib.Extensions;

namespace ApplicationCore.Entities.Inventory
{
    [Table("SalesReturnItems")]
    public class SalseReturnItem : BaseEntity
    {
        public int Id { get; set; }
        public int SaleReturnId { get; set; }
        public int ProductId { get; set; }
        public int VariantId { get; set; }
        public int UnitId { get; set; }
        public int Quantity { get; set; }
        public decimal SalePrice { get; set; }
        public string VariantName { get; set; }
        [Write(false)]
        public int ReturnQuantity { get; set; }

        [Write(false)]
        public string UnitName { get; set; }
        [Write(false)]
        public string ProductName { get; set; }
        [Write(false)]
        public int SaleQuantity { get; set; }

        [Write(false)]
        public int AlreadyReturnedQuantity { get; set; }=0;

    }

}









