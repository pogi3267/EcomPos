namespace ApplicationCore.Models
{
    public class CreatePaymentRequest
    {
        public long Amount { get; set; }
        public string Currency { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public int OrderId { get; set; }
        public string OrderCode { get; set; }

        public string Key { get; set; }
        public string Secret { get; set; }
        public string SucessUrl { get; set; }
        public string FailedUrl { get; set; }
    }
}