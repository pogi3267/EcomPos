using Dapper.Contrib.Extensions;

namespace ApplicationCore.Entities.Marketing
{
    [Table("FlashDealProducts")]
    public class FlashDealProducts : BaseEntity
    {
        [Key]
        public int Id { get; set; }
        public int FlashDealId { get; set; }
        public int ProductId { get; set; }
        public decimal? Discount { get; set; }
        public string DiscountType { get; set; }
    }
}