using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using Dapper.Contrib.Extensions;

namespace ApplicationCore.Entities.Inventory
{
    [Table("PurchaseExpense")]
    public class PurchaseExpense : IBaseEntity
    {
        [Key]
        public int PurchaseExpenseId { get; set; }
        public int PurchaseId { get; set; }
        public string Description { get; set; }
        public decimal FirstAmount { get; set; }
        public decimal SecondAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public int CostId { get; set; }

        [Write(false)]
        public string CostName { get; set; }

        [Write(false)]
        public EntityState EntityState { get; set; } = EntityState.Added;

    }

}





