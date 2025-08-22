using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace ApplicationCore.Entities.Products
{
    [Table("Reviews")]
    public class Reviews : IBaseEntity
    {
        [Key]
        public int ReviewId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public int Status { get; set; }
        public int Viewed { get; set; }
        public int ProductId { get; set; }
        public int OrderId { get; set; }
        public string UserId { get; set; }
        public string ReviewPhotos { get; set; }
        public int NoOfLike { get; set; }
        public int NoOfDislike { get; set; }
        public DateTime Created_At { get; set; }
        public DateTime? Updated_At { get; set; }

        [Write(false)]
        public EntityState EntityState { get; set; }

        [Write(false)]
        public int TotalRows { get; set; }

        [Write(false)]
        public string StatusInString { get; set; }

        [Write(false)]
        public string ProductName { get; set; }

        [Write(false)]
        public string UserName { get; set; }

        [Write(false)]
        public List<Select2OptionModel> UserList { get; set; }
        [Write(false)]
        public List<Select2OptionModel> ProductList { get; set; }
        [Write(false)]
        public List<IFormFile> ReviewImages { get; set; }
        [Write(false)]
        public string[] Photos { get; set; }

        public Reviews()
        {
            ReviewImages = new List<IFormFile>();
        }
    }
}
