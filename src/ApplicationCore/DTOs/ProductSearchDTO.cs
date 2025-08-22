namespace ApplicationCore.DTOs
{
    public class ProductSearchDTO
    {
        public string SortBy { get; set; }
        public int PageNumber { get; set; }
        public int NumberOfRows { get; set; }
        public string Price { get; set; }
        public string Rating { get; set; }
        public string Brand { get; set; }
        public string Category { get; set; }
        public string KeySearch { get; set; }
        public bool IsFlashDeal { get; set; }
        public bool IsTrending { get; set; }
        public bool IsTodaysDeal { get; set; }
        public bool IsDiscount { get; set; }
        public bool IsFeatured { get; set; }
        public bool InStock { get; set; }
        public bool OutOfStock { get; set; }

    }
}
