using ApplicationCore.Entities.Products;
using Dapper.Contrib.Extensions;
using FluentValidation;
using System;
using System.Collections.Generic;

namespace ApplicationCore.Entities.Inventory
{
    [Table("Purchases")]
    public class Purchase : BaseEntity
    {
        [Key]
        public int PurchaseId { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string PurchaseNo { get; set; }
        public int SupplierId { get; set; }
        public string Remarks { get; set; }
        public decimal SubTotalAmount { get; set; }
        public decimal GrandTotalAmount { get; set; }
        public decimal Discount { get; set; }
        public string DiscountType { get; set; }
        public decimal OtherAmount { get; set; }

        #region Extra
        [Write(false)]
        public string SupplierName { get; set; }

        [Write(false)]
        public int TotalItems { get; set; }
        
        [Write(false)]
        public int GrandTotal { get; set; }
        
        [Write(false)]
        public string SupplierAddress { get; set; }

        [Write(false)]
        public List<Select2OptionModel> SupplierList { get; set; } = new List<Select2OptionModel>();

        [Write(false)]
        public List<Select2OptionModel> ProductList { get; set; } = new List<Select2OptionModel>();

        [Write(false)]
        public List<Select2OptionModel> UnitList { get; set; } = new List<Select2OptionModel>();

        [Write(false)]
        public List<Select2OptionModel> BranchList { get; set; } = new List<Select2OptionModel>();

        [Write(false)]
        public List<Select2OptionModel> CostList { get; set; } = new List<Select2OptionModel>();

        [Write(false)]
        public List<ProductVariant> VariationList { get; set; } = new List<ProductVariant>();

        [Write(false)]
        public List<PurchaseItem> PurchaseItems { get; set; } = new();

        [Write(false)]
        public List<PurchaseExpense> Expenses { get; set; } = new();

        #endregion Extra
    }

    public class PurchaseValidator : AbstractValidator<Purchase>
    {
        public PurchaseValidator()
        {
            //RuleFor(x => x.Name).NotEmpty().WithMessage("Please enter the product name.");
            //RuleFor(x => x.CategoryId).NotNull().WithMessage("Please select a category.");
            //RuleFor(x => x.UnitId).NotEmpty().WithMessage("Please select a unit.");
            //RuleFor(x => x.UnitPrice).NotEmpty().WithMessage("Please enter unit price.");
            //RuleFor(x => x.PurchasePrice).NotEmpty().WithMessage("Please enter purchase price.");
            //RuleFor(x => x.CurrentStock).NotEmpty().WithMessage("Please enter current stock.");
        }

    }
}



