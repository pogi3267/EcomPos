using Dapper.Contrib.Extensions;
using System;

namespace ApplicationCore.Entities.Products
{
    [Table("ProductTypes")]
    public class ProductType : BaseEntity
    {
        [Key]
        public int ProductTypeId { get; set; }

        public string Name { get; set; }

        public DateTime? Deleted_At { get; set; }
    }

}
