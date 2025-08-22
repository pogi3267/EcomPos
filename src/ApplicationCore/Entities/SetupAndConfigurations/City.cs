using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;

namespace ApplicationCore.Entities.SetupAndConfigurations
{
    [Table("Cities")]
    public class City : BaseEntity
    {
        [Key]
        public int CitiesId { get; set; }
        public string Name { get; set; }
        public int StateId { get; set; }
        public decimal Cost { get; set; }
        public int Status { get; set; }
        public DateTime? Deleted_At { get; set; }
        [Write(false)]
        public string CountryName { get; set; }
        [Write(false)]
        public int CountryId { get; set; }
        [Write(false)]
        public List<Select2OptionModel> StateList { get; set; }
        [Write(false)]
        public string StateName { get; set; }
    }
}