using Dapper.Contrib.Extensions;
using System;

namespace ApplicationCore.Entities.SetupAndConfigurations
{
    [Table("Taxes")]
    public class Tax : BaseEntity
    {
        public Tax()
        {
            Created_At = DateTime.UtcNow;
            Updated_At = DateTime.UtcNow;
        }
        [Key]
        public int TaxId { get; set; }
        public string Name { get; set; }
        public int TaxStatus { get; set; }
        public DateTime? Deleted_At { get; set; }

    }
}

