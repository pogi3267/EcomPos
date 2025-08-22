using ApplicationCore.Entities.ApplicationUser;

namespace ApplicationCore.DTOs
{
    public class OrderRequest
    {
        public Address Address { get; set; }
        public string ProductStocks { get; set; } //StockId|Quantity
        public int CouponId { get; set; }
        public int PaymentType { get; set; } // 1: Cash on delivery, 2: Online payment
        public int PickupId { get; set; }
        public int ShippingLocation { get; set; } // 1: Inside, 2: Outside
    }
}
