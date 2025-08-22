using Dapper.Contrib.Extensions;

namespace ApplicationCore.Entities.Orders
{
    [Table("Carts")]
    public class Carts : BaseEntity
    {
        public Carts()
        {
            Discount = 0;
            Tax = 0;
        }

        [Key]
        public int CartId { get; set; }
        public string OwnerId { get; set; }
        public string UserId { get; set; }
        public string TempUserId { get; set; }
        public int AddressId { get; set; }
        public int ProductId { get; set; }
        public string Variation { get; set; }
        public int VariationId { get; set; }
        public decimal? Price { get; set; }
        public decimal? Tax { get; set; }
        public decimal? ShippingCost { get; set; }
        public string ShippingType { get; set; }
        public int? PickupPoint { get; set; }
        public decimal? Discount { get; set; }
        public string ProductReferralCode { get; set; }
        public string CouponCode { get; set; }
        public short CouponApplied { get; set; }
        public int Quantity { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsBuy { get; set; }
        [Write(false)]
        public string Variant { get; set; }
    }
}