namespace ApplicationCore.DTOs
{
    public class BulkImport
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int CategoryId { get; set; }
        public int BrandId { get; set; }
        public string VideoProvider { get; set; }
        public string VideoLink { get; set; }
        public string Tags { get; set; }
        public decimal UnitPrice { get; set; }
        public int UnitId { get; set; }
        public string Slug { get; set; }
        public int CurrentStock { get; set; }
        public int EstShippingDays { get; set; }
        public string ProductSKU { get; set; }
        public string MetaDescription { get; set; }
        public string ThumbnailImage { get; set; }
        public string Photos { get; set; }
    }
}
