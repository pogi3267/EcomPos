using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;

namespace ApplicationCore.Entities.SetupAndConfigurations
{
    [Table("States")]
    public class State : BaseEntity
    {
        [Key]
        public int StateId { get; set; }
        public int CountriesId { get; set; }
        public string Name { get; set; }
        public DateTime? Deleted_At { get; set; }
        [Write(false)]
        public List<CategorySelect2Option> CountriesList { get; set; }
        [Write(false)]
        public string CountryName { get; set; }

    }

}

