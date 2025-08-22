using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using Dapper.Contrib.Extensions;

namespace ApplicationCore.Entities.Orders
{
    [Table("OrderDetails")]
    public class OrderDetail : IBaseEntity
    {
        [Key]
        public int OrderDetailId { get; set; }
        public int OrderId { get; set; }
        public string SellerId { get; set; }
        public int ProductId { get; set; }
        public string Variation { get; set; }
        public decimal Price { get; set; }
        public decimal Tax { get; set; }
        public decimal Discount { get; set; }
        public decimal ShippingCost { get; set; }
        public int Quantity { get; set; }
        public string PaymentStatus { get; set; }
        public string DeliveryStatus { get; set; }
        public string ShippingType { get; set; }
        public int? PickupPointId { get; set; }
        public string ProductReferralCode { get; set; }
        public decimal AdminDiscount { get; set; }

        [Write(false)]
        public string ProductName { get; set; }

        [Write(false)]
        public decimal ProductPrice { get; set; }

        [Write(false)]
        public string ThumbnailImage { get; set; }

        [Write(false)]
        public string VariantSKU { get; set; }

        [Write(false)]
        public decimal VariantPrice { get; set; }

        [Write(false)]
        public EntityState EntityState { get; set; }
        public OrderDetail()
        {
            EntityState = EntityState.Added;
        }
    }
}