using ApplicationCore.Entities;

namespace ApplicationCore.DTOs
{
    public class AdminWalletDTO : BaseEntity
    {
        public decimal TotalCollection { get; set; }
        public decimal TotalDeliveryCharge { get; set; }
        public decimal TotalTaxCollection { get; set; }
        public decimal PendingAmount { get; set; }
        public decimal TotalRevenue { get; set; }

    }
}
