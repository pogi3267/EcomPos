using ApplicationCore.Entities.Inventory;
using ApplicationCore.Entities.Products;
using ApplicationCore.Enums;
using ApplicationCore.Extensions;
using ApplicationWeb.HelperAndConstant;
using Infrastructure.Interfaces.Inventory;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Operations;
using Stripe;

namespace ApplicationWeb.Areas.Admin.Controllers.APIs.Inventory;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Authorize]
public class PurchaseNewController : ControllerBase
{
    private readonly IPurchaseService _service;
    private readonly IImageProcessing _image;

    public PurchaseNewController(IPurchaseService purchaseService, IImageProcessing imageProcessing)
    {
        _service = purchaseService;
        _image = imageProcessing;
    }

    private string GetCurrentUserId()
    {
        return User.Claims.First(i => i.Type == "Id").Value;
    }

    [HttpGet("GetInitial")]
    public async Task<IActionResult> GetInitial()
    {
        try
        {
            return Ok(await _service.GetInitial());
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }

    [HttpPost("list")]
    public async Task<IActionResult> GetListPostAsync()
    {
        #region default intialization
        var draw = Convert.ToInt32(Request.Form["draw"].FirstOrDefault());
        var start = Request.Form["start"].FirstOrDefault();
        var length = Request.Form["length"].FirstOrDefault();
        var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
        var sortColumnDir = Request.Form["order[0][dir]"].FirstOrDefault();
        var searchValue = Request.Form["search[value]"].FirstOrDefault();
        int pageSize = length != null ? Convert.ToInt32(length) : 0;
        int skip = start != null ? Convert.ToInt32(start) : 0;
        string status = Request.Form["status"].ToString();
        #endregion default intialization

        var result = await _service.GetDataFromDbase(searchValue, pageSize, skip, sortColumn, sortColumnDir, status);
        int filteredResultsCount = result.Count > 0 ? result[0].TotalRows : 0;
        int totalResultsCount = result.Count > 0 ? result[0].TotalRows : 0;

        return Ok(new
        {
            draw,
            recordsTotal = totalResultsCount,
            recordsFiltered = filteredResultsCount,
            data = result
        });
    }

    [HttpGet("variants/{productId}")]
    public async Task<IActionResult> GetVariants(int productId)
    {
        try
        {
            var variants = await _service.GetProductVariantsAsync(productId);
            return Ok(variants);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("Save")]
    public async Task<IActionResult> SaveAsync([FromBody] Purchase model)
    {
        try
        {
            Purchase entity;
            List<PurchaseItem> currentItems = new List<PurchaseItem>();
            if (model.PurchaseId > 0)
            {
                entity = await _service.GetPurchaseDetail(model.PurchaseId);
                currentItems = entity.PurchaseItems;

                entity.PurchaseDate = model.PurchaseDate;
                entity.SupplierId = model.SupplierId;
                entity.Remarks = model.Remarks;
                entity.SubTotalAmount = model.SubTotalAmount;
                entity.GrandTotalAmount = model.GrandTotalAmount;
                entity.Discount = model.Discount;
                entity.DiscountType = model.DiscountType;
                entity.OtherAmount = model.OtherAmount;
                entity.Updated_At = DateTime.UtcNow;
                entity.Updated_By = GetCurrentUserId();
                entity.EntityState = EntityState.Modified;

                entity.PurchaseItems.SetUnchanged();
                model.PurchaseItems.ForEach(details =>
                {
                    PurchaseItem? item = entity.PurchaseItems.FirstOrDefault(c => c.PurchaseItemId == details.PurchaseItemId);
                    if (item is null)
                    {
                        item = details;
                        item.PurchaseId = entity.PurchaseId;
                        item.EntityState = EntityState.Added;
                        entity.PurchaseItems.Add(item);                        
                    }
                    else
                    {
                        item.PurchaseId = model.PurchaseId;
                        item.ProductId = details.ProductId;
                        item.VariantId = details.VariantId;
                        item.Quantity = details.Quantity;
                        item.UnitId = details.UnitId;
                        item.Price = details.Price;
                        item.BranchId = details.BranchId;
                        item.TotalPrice = details.TotalPrice;
                        item.EntityState = EntityState.Modified;
                    }
                });
                entity.PurchaseItems.Where(x => x.EntityState == EntityState.Unchanged).ToList()
                    .ForEach(x => x.EntityState = EntityState.Deleted);


                entity.Expenses.SetUnchanged();
                model.Expenses.ForEach(details =>
                {
                    PurchaseExpense? item = entity.Expenses.FirstOrDefault(c => c.PurchaseExpenseId == details.PurchaseExpenseId);
                    if (item is null)
                    {
                        item = details;
                        item.PurchaseId = entity.PurchaseId;
                        item.EntityState = EntityState.Added;
                        entity.Expenses.Add(item);
                    }
                    else
                    {
                        item.PurchaseId = model.PurchaseId;
                        item.Description = details.Description;
                        item.FirstAmount = details.FirstAmount;
                        item.SecondAmount = details.SecondAmount;
                        item.TotalAmount = details.TotalAmount;
                        item.CostId = details.CostId;
                        item.EntityState = EntityState.Modified;
                    }
                });
                entity.Expenses.Where(x => x.EntityState == EntityState.Unchanged).ToList()
                    .ForEach(x => x.EntityState = EntityState.Deleted);
            }
            else 
            {
                entity = model;
                currentItems = model.PurchaseItems;
                if (string.IsNullOrWhiteSpace(entity.PurchaseNo))
                    entity.PurchaseNo = $"PUR-{DateTime.Now:yyMMdd-HHmmss}";
                entity.PurchaseItems.SetAdded();
                entity.Expenses.SetAdded();
                entity.Created_At = DateTime.UtcNow;
                entity.Created_By = GetCurrentUserId();
                entity.EntityState = EntityState.Added;
            }

            var id = await _service.SavePurchaseAsync(entity, entity.PurchaseItems, entity.Expenses, currentItems);
            return Ok(new { purchaseId = id });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> GetPurchaseAsync(int id)
    {
        try
        {
            return Ok(await _service.GetPurchaseAsync(id));
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }

}
