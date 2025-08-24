using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.DTOs
{
    public class BarcodeSettingsModel
    {
        public string BarcodeType { get; set; } = "code128";
        public int BarcodeWidth { get; set; } = 200;
        public int BarcodeHeight { get; set; } = 100;
        public string PaperSize { get; set; } = "a4";
        public bool ShowProductName { get; set; } = true;
        public bool ShowPrice { get; set; } = true;
        public bool ShowCompanyName { get; set; } = true;
        public string CompanyName { get; set; } = "Your Company Name";
    }
}
