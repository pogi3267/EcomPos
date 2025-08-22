using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace ApplicationCore.Entities.Marketing
{
    [Table("FlashDeals")]
    public class FlashDeal : BaseEntity
    {
        [Key]
        public int FlashDealId { get; set; }
        public string Title { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int Status { get; set; }
        public int Featured { get; set; }
        public string BackgroundColor { get; set; }
        public string TextColor { get; set; }
        public string Banner { get; set; }
        public string Slug { get; set; }
        public int? ProductId { get; set; }

        [Write(false)]
        public bool IsStatus { get; set; }

        [Write(false)]
        public List<Select2OptionModel> ProductList { get; set; }

        [Write(false)]
        public string ProductName { get; set; }

        [Write(false)]
        public IFormFile BannerImage { get; set; }

    }

}
