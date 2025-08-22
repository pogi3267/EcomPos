using Dapper.Contrib.Extensions;
using System;

namespace ApplicationCore.Entities.Orders
{
    [Table("CombinedOrders")]
    public class CombinedOrder : BaseEntity
    {
        [Key]
        public int CombinedOrderId { get; set; }
        public string UserId { get; set; }
        public string ShippingAddress { get; set; }
        public decimal GrandTotal { get; set; }
        public DateTime? Deleted_At { get; set; }

    }
}
