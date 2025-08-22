using Dapper.Contrib.Extensions;
using System;

namespace ApplicationCore.Entities.SetupAndConfigurations
{
    [Table("Countries")]
    public class Country : BaseEntity
    {
        [Key]
        public int CountriesId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int Status { get; set; }
        public DateTime? Deleted_At { get; set; }

    }

}
