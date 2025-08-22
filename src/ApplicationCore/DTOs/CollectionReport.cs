using ApplicationCore.Entities;
using System.Collections.Generic;
using System.Web.Mvc;

namespace ApplicationCore.DTOs
{
    public class CollectionReport:BaseEntity
    {
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string CollectionType { get; set; }
        public int Customer { get; set; }
        public string InvoiceId { get; set; }

        public List<SelectListItem> CustomerList { get; set; }
        public List<SelectListItem> InvoiceList { get; set; }

    }
}



