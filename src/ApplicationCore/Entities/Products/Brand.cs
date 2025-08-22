using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace ApplicationCore.Entities.Products
{
    [Table("Brands")]
    public class Brand :BaseEntity
    {
        public Brand()
        {

        }

        [Key]   
        public int BrandId { get; set; }
        public string Name { get; set; }
        public string Logo { get; set; }
        public int Top { get; set; }
        public string Slug { get; set; }
        public string MetaTittle { get; set; }
        public string MetaDescription { get; set; }
        public DateTime? Deleted_At { get; set; }

        [Write(false)]
        public IFormFile Photo { get; set; }
       
    }

}
