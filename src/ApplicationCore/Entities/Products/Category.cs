using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace ApplicationCore.Entities.Products
{
    [Table("Categories")]
    public class Category : BaseEntity
    {
        public Category()
        {
            Created_At = DateTime.UtcNow;
            Categories = new List<Category>();
        }

        [Key]
        public int CategoryId { get; set; }
        public int? ParentId { get; set; }
        public int Level { get; set; }
        public string Name { get; set; }
        public int OrderLevel { get; set; }
        public decimal CommisionRate { get; set; }
        public string Banner { get; set; }
        public string Icon { get; set; }
        public int Featured { get; set; }
        public int Top { get; set; }
        public int Digital { get; set; }
        public string Slug { get; set; }
        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }
        public DateTime? Deleted_At { get; set; }

        [Write(false)]
        public IFormFile BannerImage { get; set; }

        [Write(false)]
        public IFormFile IconImage { get; set; }

        [Write(false)]
        public string ParentName { get; set; }

        [Write(false)]
        public List<CategorySelect2Option> ParentList { get; set; }

        [Write(false)]
        public List<Category> Categories { get; set; }
    }
}