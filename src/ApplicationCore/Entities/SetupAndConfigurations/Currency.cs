using Dapper.Contrib.Extensions;
using System;

namespace ApplicationCore.Entities.SetupAndConfigurations
{
    [Table("Currencies")]
    public class Currency : BaseEntity
    {
        [Key]
        public int CurrencyId { get; set; }
        public string Name { get; set; }
        public string Symbol { get; set; }
        public decimal ExchangeRate { get; set; }
        public int Status { get; set; }
        public string Code { get; set; }
        public DateTime? Deleted_At { get; set; }
        [Write(false)]
        public string SymbolFormatId { get; set; }
        [Write(false)]
        public decimal DecimalSeparatorId { get; set; }
        [Write(false)]
        public string NoOfDecimalsId { get; set; }

    }
}
