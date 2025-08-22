using ApplicationCore.Entities.Marketing;
using ApplicationCore.Entities.Products;
using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace ApplicationCore.Entities.SetupAndConfigurations
{
    [Table("BusinessSettings")]
    public class BusinessSetting : BaseEntity
    {
        [Key]
        public int BusinessSettingsId { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public string Lang { get; set; }
        public DateTime? Deleted_At { get; set; }
        [Write(false)]
        public List<BusinessSetting> Settings { get; set; }
        [Write(false)]
        public List<Select2OptionModel> CarouselImageList { get; set; }
        [Write(false)]
        public List<Select2OptionModel> PaymentImageList { get; set; }
        [Write(false)]
        public List<TimeZoneInfo> Timezones { get; set; }
        [Write(false)]
        public List<Category> Categories { get; set; }
        [Write(false)]
        public List<FlashDeal> FlashDeals { get; set; }
        [Write(false)]
        public List<Select2OptionModel> Banner1ImageList { get; set; }
        [Write(false)]
        public List<Select2OptionModel> Banner2ImageList { get; set; }
        [Write(false)]
        public List<Select2OptionModel> Banner3ImageList { get; set; }
        [Write(false)]
        public List<IFormFile> Images { get; set; }
        [Write(false)]
        public List<IFormFile> Images2 { get; set; }
        [Write(false)]
        public List<IFormFile> Images3 { get; set; }
        [Write(false)]
        public string Type2 { get; set; }
        [Write(false)]
        public string Type3 { get; set; }
        public BusinessSetting()
        {
            Settings = new List<BusinessSetting>();
            CarouselImageList = new List<Select2OptionModel>();
            PaymentImageList = new List<Select2OptionModel>();
            Images = new List<IFormFile>();
            Images2 = new List<IFormFile>();
            Images3 = new List<IFormFile>();
        }

    }
}
