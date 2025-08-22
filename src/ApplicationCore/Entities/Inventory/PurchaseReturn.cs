using ApplicationCore.Entities.Marketing;
using ApplicationCore.Entities.Products;
using Dapper.Contrib.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace ApplicationCore.Entities.Inventory
{
    [Table("PurchaseReturns")]
    public class PurchaseReturn : BaseEntity
    {
        [Key]
        public int PurchaseRetuenId { get; set; }
        public int PurchaseId { get; set; }
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public int SupplierId { get; set; }
        public int UnitId { get; set; }
        public int BranchId { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal ReturnPrice { get; set; }
        public decimal RegularPrice { get; set; }
        public decimal SalePrice { get; set; }
        public decimal Expanse { get; set; }
        public int TotalQty { get; set; }
        public string ProductImage { get; set; }
        public string Colors { get; set; }
        public string VariantProduct { get; set; }
        public string Attributes { get; set; }
        public string ChoiceOptions { get; set; }
        public string Variations { get; set; }
        public string UserId { get; set; }


        #region Extra

        [Write(false)]
        public int Id { get; set; }

        [Write(false)]
        public bool ProductReturnStatus { get; set; }

        [Write(false)]
        public string InvoiceNumber { get; set; }
        [Write(false)]
        public string Address { get; set; }

        [Write(false)]
        public IFormFile Photo { get; set; }

        [Write(false)]
        public List<PurchaseReturnItem> Items { get; set; } 
        [Write(false)]
        public PurchaseReturnItem PurchaseItem { get; set; }
        [Write(false)]
        public List<PurchaseRtnItemDto> PurchaseItemDTO { get; set; }

        [Write(false)]
        public List<Select2OptionModel> PhotoSourceList { get; set; }

        [Write(false)]
        public string ImageLink { get; set; }

        [Write(false)]
        public int HasVariation { get; set; }

        [Write(false)]
        public string Variation { get; set; }

        [Write(false)]
        public List<int> AttributeIds { get; set; }

        [Write(false)]
        public List<Select2OptionModel> AttributeList { get; set; }

        [Write(false)]
        public List<ProductStock> ProductStocks { get; set; }

        [Write(false)]
        public List<Select2OptionModel> ProductList { get; set; }

        [Write(false)]
        public List<Select2OptionModel> UnitList { get; set; }
        [Write(false)]
        public List<Select2OptionModel> SupplierList { get; set; }

        [Write(false)]
        public List<int> ColorIds { get; set; }

        [Write(false)]
        public List<ColorSelect2Option> ColorList { get; set; }

        [Write(false)]
        public List<ProductVariation> ProductStockList { get; set; }
         [Write(false)]
        public string ItemName { get; set; }
        [Write(false)]
        public string PurchaseRtnItem { get; set; }

        #endregion Extra


        public class PurchaseRtnItemDto
        {
            public int Id { get; set; }
            public int PurchaseId { get; set; }
            public string Variant { get; set; } = string.Empty;
            public decimal VariantPrice { get; set; }
            public int PurchaseQuantity { get; set; }
            public int Quantity { get; set; }
            public int AlreadyReturnedQuantity { get; set; }
        }
    }

  
}





