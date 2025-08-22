using ApplicationCore.Entities.Marketing;
using ApplicationCore.Entities.Products;
using Dapper.Contrib.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace ApplicationCore.Entities.Inventory
{
    [Table("SalesReturns")]
    public class SalseReturn : BaseEntity
    {
        [Key]
        public int SaleReturnId { get; set; }
        public int SaleId { get; set; }
        public int ProductId { get; set; }
        public DateTime? SaleReturnDate { get; set; }
        public int CustomerId { get; set; }
        public int VariantId { get; set; }
        public int UnitId { get; set; }
        public int BranchId { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal RegularPrice { get; set; }
        public decimal Expanse { get; set; }
        public int TotalQty { get; set; }
        public string ProductImage { get; set; }
        public string UserId { get; set; }


        #region Extra


        

        [Write(false)]
        public decimal SalsePrice { get; set; }
        [Write(false)]
        public string CustomerName { get; set; }

        [Write(false)]
        public decimal SalseReturnPrice { get; set; }



        [Write(false)]
        public bool ProductReturnStatus { get; set; }

        [Write(false)]
        public string InvoiceNumber { get; set; }
        [Write(false)]
        public string Address { get; set; }

        [Write(false)]
        public IFormFile Photo { get; set; }

        [Write(false)]
        public List<SalseReturnItem> Items { get; set; }
        [Write(false)]
        public string SalseItem { get; set; }
        [Write(false)]
        public List<SalseItemDTO> SalseItemDTO { get; set; }

        [Write(false)]
        public List<Select2OptionModel> PhotoSourceList { get; set; }

        [Write(false)]
        public string ImageLink { get; set; }

        [Write(false)]
        public int HasVariation { get; set; }

        [Write(false)]
        public string Variation { get; set; }



        [Write(false)]
        public List<Select2OptionModel> VariantList { get; set; }

        [Write(false)]
        public List<ProductStock> ProductStocks { get; set; }

        [Write(false)]
        public List<Select2OptionModel> ProductList { get; set; }

        [Write(false)]
        public List<Select2OptionModel> UnitList { get; set; }
        [Write(false)]
        public List<Select2OptionModel> CustomerList { get; set; }

        [Write(false)]
        public List<int> ColorIds { get; set; }

        [Write(false)]
        public List<ColorSelect2Option> ColorList { get; set; }

        [Write(false)]
        public List<ProductVariation> ProductStockList { get; set; }
        [Write(false)]
        public string ItemName { get; set; }

        #endregion Extra
    }


}



