using System.ComponentModel.DataAnnotations.Schema;

namespace ApplicationCore.Entities.Products
{
    [Table("ProductVariants")]
    public class ProductVariant : BaseEntity
    {
        [Dapper.Contrib.Extensions.Key]
        public int Variant_id { get; set; }
        public string Variant { get; set; }
        public int Product_id { get; set; }
        public string SKU { get; set; }
        public string Barcode { get; set; }
        public string Attributes { get; set; }
        public int Unit_id { get; set; }
        public decimal Regular_price { get; set; }
        public decimal? Sale_price { get; set; }
        public int? Reorder_level { get; set; }
        public string Image { get; set; }
        public bool Status { get; set; }


    }
}