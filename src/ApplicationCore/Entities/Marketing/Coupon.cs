using Dapper.Contrib.Extensions;
using System;

namespace ApplicationCore.Entities.Marketing
{
    [Table("Coupons")]
    public class Coupon : BaseEntity
    {
        [Key]
        public int CouponId { get; set; }
        public string UserId { get; set; }
        public string Type { get; set; }
        public string Code { get; set; }
        public string Details { get; set; }
        public decimal Discount { get; set; }
        public string DiscountType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public Coupon()
        {
            StartDate = DateTime.UtcNow;
            EndDate = DateTime.UtcNow;
            IsActive = true;
        }
    }
}