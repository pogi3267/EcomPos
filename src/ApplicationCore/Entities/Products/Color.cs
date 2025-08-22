using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;

namespace ApplicationCore.Entities.Products
{
    [Table("Color")]
    public class Color : BaseEntity
    {
        public Color()
        {
            Colors = new List<Color>();
        }
        [Key]
        public int ColorId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public DateTime? Deleted_At { get; set; }

        [Write(false)]
        public List<Color> Colors { get; set; }
    }
}
