using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;

namespace ApplicationCore.Entities.Products
{
    [Table("Attributes")]
    public class ProductAttribute : BaseEntity
    {
        [Key]
        public int AttributeId { get; set; }
        public string Name { get; set; }
        public DateTime? Deleted_At { get; set; }

        [Write(false)]
        public string Values { get; set; }

        [Write(false)]
        public List<AttributeValue> AttributeValueList { get; set; }
        public ProductAttribute()
        {
            AttributeValueList = new List<AttributeValue>();
        }

    }
    public class ProductAttributeViewDTO
    {
        public string AttributeId { get; set; }
        public string[] Value { get; set; }
    }
}
