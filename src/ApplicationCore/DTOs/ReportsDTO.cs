using ApplicationCore.Entities;
using System.Collections.Generic;

namespace ApplicationCore.DTOs
{
    public class ReportsDTO : BaseEntity
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string NumberOfSale { get; set; }
        public string NumberOfStock { get; set; }
        public string NumberOfInHouseSale { get; set; }
        public string SearchBy { get; set; }
        public string NumberOfSearches { get; set; }
        public string NumberOfWishProduct { get; set; }
        public int CategoryId { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public int OrderId { get; set; }
        public string ParentCategoryName { get; set; }

        public List<Select2OptionModel> CategoryList { get; set; }

    }
}





