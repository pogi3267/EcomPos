using Dapper.Contrib.Extensions;

namespace ApplicationCore.Entities.Products
{
    [Table("ProductTaxes")]
    public class ProductTax : BaseEntity
    {
        public ProductTax()
        {

        }
        [Key]
        public int ProductTaxId { get; set; }
        public int ProductId { get; set; }
        public int TaxId { get; set; }
        public decimal Amount { get; set; }
        public string TaxType { get; set; }

    }
}

