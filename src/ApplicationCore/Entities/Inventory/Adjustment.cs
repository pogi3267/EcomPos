using Dapper.Contrib.Extensions;
using System;

namespace ApplicationCore.Entities.Inventory
{
    [Table("Adjustment")]
    public class Adjustment : BaseEntity
    {
        public Adjustment()
        {
            AdjustmentDate = DateTime.Now;
        }
        [Key]
        public int AdjustmentId { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime AdjustmentDate { get; set; }
        public int OperationalUserId { get; set; }
        public string AdjustmentFor { get; set; }
        public string Reason { get; set; }
        public double Amount { get; set; }
        [Write(false)]
        public string OperationalUserName { get; set; }

    }
}
