using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.DTOs
{
    public class BarcodeItemModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string BarcodeText { get; set; }
        public decimal Price { get; set; }
        public string Variant { get; set; }
        public int Quantity { get; set; }

    }
}
