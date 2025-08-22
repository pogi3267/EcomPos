using Dapper.Contrib.Extensions;
using System;
using System.ComponentModel.DataAnnotations;

namespace ApplicationCore.Entities.Products
{
    [Table("SearchActivitys")]
    public class SearchActivity
    {
        public int Id { get; set; }
        [Required]
        public string URL { get; set; }
        [Required]
        public string SearchCriteria { get; set; }
        [Required]
        public DateTime Date { get; set; }
        public string loginID { get; set; }
        public string UserName { get; set; }

    }

}
