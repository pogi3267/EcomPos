using ApplicationCore.Entities.Inventory;
using ApplicationCore.Entities.Marketing;
using ApplicationCore.Entities.Products;
using ApplicationCore.Entities.SetupAndConfigurations;
using ApplicationCore.Enums;
using ApplicationCore.Extensions;
using ApplicationWeb.HelperAndConstant;
using Infrastructure.Interfaces.Inventory;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuGet.Packaging;
using System.Security.Policy;
using System.Text.Json;

namespace ApplicationWeb.Areas.Admin.Controllers.APIs.Inventory;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Authorize]
public class PurchaseController : ControllerBase
{
    private readonly IPurchaseService _service;
    private readonly IImageProcessing _image;
    public PurchaseController(IPurchaseService purchaseService, IImageProcessing imageProcessing)
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

    //[HttpGet("GetAttribute/{search}")]
    //public async Task<IActionResult> GetAttribute(string search)
    //{
    //    return Ok(await _service.GetAttribute(search));
    //}

    //[HttpPost("Save")]
    //public async Task<IActionResult> SaveAsync([FromForm] Purchase model)
    //{
    //    try
    //    {
    //        //var purchaseValidator = new PurchaseValidator();
    //        //var result = purchaseValidator.Validate(model);
    //        //if (!result.IsValid)
    //        //{
    //        //    return BadRequest(result.Errors);
    //        //}

    //        //Purchase entity;

    //        //if (model.Id > 0)
    //        //{
    //        //    entity = await _service.GetPurchaseAsync(model.Id);

    //        //    entity.ProductId = model.ProductId;
    //        //    entity.ProductCode = model.ProductCode;
    //        //    entity.PurchaseDate = model.PurchaseDate.HasValue ? model.PurchaseDate.Value.ToUniversalTime() : (DateTime?)null;
    //        //    entity.SupplierId = model.SupplierId;
    //        //    entity.UnitId = model.UnitId;
    //        //    entity.BranchId = model.BranchId;
    //        //    entity.PurchasePrice = model.PurchasePrice;
    //        //    entity.RegularPrice = model.RegularPrice;
    //        //    entity.SalePrice = model.SalePrice;
    //        //    entity.Expanse = model.Expanse;
    //        //    entity.TotalQty = model.TotalQty;
    //        //    entity.ProductImage = model.ProductImage;
    //        //    entity.Colors = model.Colors;
    //        //    entity.ColorIds = model.ColorIds;
    //        //    entity.VariantProduct = model.VariantProduct;
    //        //    entity.Attributes = model.Attributes;
    //        //    entity.AttributeIds = model.AttributeIds;
    //        //    entity.ChoiceOptions = model.ChoiceOptions;
    //        //    entity.Variations = model.Variation;
    //        //    entity.HasVariation = model.HasVariation;
    //        //    entity.ImageLink = model.ImageLink;
    //        //    entity.EntityState = EntityState.Modified;
    //        //    entity.Updated_At = DateTime.UtcNow;
    //        //    entity.Updated_By = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
    //        //    entity.UserId = entity.Updated_By;
    //        //    if (model.Photo != null)
    //        //    {
    //        //        string fileName = DateTime.UtcNow.ToString("dddd_dd_MMMM_yyyy_HH_mm_ss") + model.Photo.FileName;
    //        //        string path = _image.GetImagePath(fileName, "Image");
    //        //        using (var stream = new FileStream(path, FileMode.Create))
    //        //        {
    //        //            model.Photo.CopyTo(stream);
    //        //        }
    //        //        entity.ProductImage = _image.GetImagePathForDb(path);
    //        //    }
    //        //    else
    //        //    {
    //        //        entity.ProductImage = model.ProductImage;
    //        //    }



    //        //    // Product Stock
    //        //    List<ProductStock> productStocks = new List<ProductStock>();
    //        //    if (!string.IsNullOrEmpty(model.Variation))
    //        //    {
    //        //        var prodListRtn = JsonSerializer.Deserialize<List<ProductStock>>(model.Variation);
    //        //        if (prodListRtn != null && prodListRtn.Count > 0)
    //        //        {
    //        //            foreach (var x in entity.Items)
    //        //            {
    //        //                var stock = entity.ProductStocks.Where(p => p.ProductId == model.ProductId && p.Variant == x.VariantName).FirstOrDefault();
    //        //                if (stock != null)
    //        //                {
    //        //                    stock.ProductId = model.ProductId;
    //        //                    stock.Variant = x.VariantName;
    //        //                    stock.Quantity -= x.Quantity;
    //        //                    await _service.UpdateProductStockAsync(stock);
    //        //                }
    //        //            }

    //        //            foreach (var newStock in prodListRtn)
    //        //            {
    //        //                newStock.Created_By = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
    //        //                newStock.Variant = newStock.Variant.Replace(" ", "");
    //        //                newStock.Price = 0;

    //        //                var existingStock = await _service.GetProductStockAsync(model.ProductId, newStock.Variant);

    //        //                if (existingStock != null)
    //        //                {
    //        //                    existingStock.Quantity += newStock.Quantity;
    //        //                    await _service.UpdateProductStockAsync(existingStock);
    //        //                }
    //        //                else
    //        //                {
    //        //                    newStock.ProductId = model.ProductId;
    //        //                    newStock.Variant = newStock.Variant;
    //        //                    newStock.Quantity = newStock.Quantity;
    //        //                    newStock.EntityState = EntityState.Added;
    //        //                    productStocks.Add(newStock);
    //        //                }
    //        //            }


    //        //        }
    //        //    }


               

    //        //    var existingItems = await _service.GetPurchaseItemsByPurchaseIdAsync(entity.Id);
    //        //    var updatedItems = JsonSerializer.Deserialize<List<PurchaseItem>>(model.Variation) ?? new List<PurchaseItem>();
    //        //    // Delete items that are in the database but not in the new list
    //        //    entity.Items = existingItems.Where(ei => updatedItems.All(ui => ui.Id != ei.Id)).ToList();
    //        //    entity.Items.ForEach(item => item.EntityState = EntityState.Deleted);

    //        //    // Update existing items and add new ones
    //        //    List<PurchaseItem> newItems = new List<PurchaseItem>();
    //        //    foreach (var item in updatedItems)
    //        //    {
    //        //        var existingItem = entity.Items.FirstOrDefault(ei => ei.Id == item.Id);
    //        //        if (existingItem != null)
    //        //        {
    //        //            existingItem.VariantName = item.Variant;
    //        //            existingItem.VariantPrice = item.Price;
    //        //            existingItem.Quantity = item.Quantity;
    //        //            existingItem.EntityState = EntityState.Modified;
    //        //            entity.Items.Add(existingItem);
    //        //        }
    //        //        else
    //        //        {
    //        //            item.PurchaseId = entity.Id;
    //        //            item.VariantName = item.Variant;
    //        //            item.VariantPrice = item.Price;
    //        //            item.EntityState = EntityState.Added;
    //        //            newItems.Add(item);
    //        //        }
    //        //    }

    //        //    entity.Items.AddRange(newItems); // Add new items




    //        //    await _service.UpdateAsync(entity, productStocks);
    //        //}
    //        //else
    //        //{

    //        //    entity = model;
    //        //    entity.Created_By = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
    //        //    entity.UserId = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
    //        //    entity.Variations = model.Variation;
    //        //    if (entity.Photo != null)
    //        //    {
    //        //        string fileName = DateTime.UtcNow.ToString("dddd_dd_MMMM_yyyy_HH_mm_ss") + entity.Photo.FileName;
    //        //        string path = _image.GetImagePath(fileName, "Image");
    //        //        using (var stream = new FileStream(path, FileMode.Create))
    //        //        {
    //        //            model.Photo.CopyTo(stream);
    //        //        }
    //        //        entity.ProductImage = _image.GetImagePathForDb(path);
    //        //    }

    //        //    //Purchase Item
    //        //    List<PurchaseItem> purchaseItems = new List<PurchaseItem>();

    //        //    if (!string.IsNullOrEmpty(model.Variation))
    //        //    {
    //        //        var prodList = JsonSerializer.Deserialize<List<ProductStock>>(model.Variation);

    //        //        if (prodList != null && prodList.Count > 0)
    //        //        {
    //        //            prodList.ForEach(item =>
    //        //            {
    //        //                purchaseItems.Add(new PurchaseItem
    //        //                {
    //        //                    VariantName = item.Variant.Replace(" ", ""),
    //        //                    Quantity = Convert.ToInt32(item.Quantity),
    //        //                    VariantPrice = item.Price
    //        //                });
    //        //            });
    //        //        }
    //        //    }


    //        //    // Product Stock

    //        //    List<ProductStock> productStocks = new List<ProductStock>();
    //        //    if (!string.IsNullOrEmpty(model.Variation))
    //        //    {
    //        //        var prodList = JsonSerializer.Deserialize<List<ProductStock>>(model.Variation);

    //        //        if (prodList != null && prodList.Count > 0)
    //        //        {
    //        //            foreach (var newStock in prodList)
    //        //            {
    //        //                newStock.Created_By = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
    //        //                newStock.Variant = newStock.Variant.Replace(" ", "");
    //        //                newStock.Price = 0;

    //        //                var existingStock = await _service.GetProductStockAsync(model.ProductId, newStock.Variant);

    //        //                if (existingStock != null)
    //        //                {
    //        //                    existingStock.Quantity += newStock.Quantity;
    //        //                    await _service.UpdateProductStockAsync(existingStock);
    //        //                }
    //        //                else
    //        //                {
    //        //                    newStock.ProductId = model.ProductId;
    //        //                    newStock.Variant = newStock.Variant;
    //        //                    newStock.Quantity = newStock.Quantity;
    //        //                    productStocks.Add(newStock);
    //        //                }
    //        //            }
    //        //        }
    //        //    }


    //        //    await _service.SaveAsync(entity, purchaseItems, productStocks);




    //        //}
    //        return Ok();
    //    }
    //    catch (Exception ex)
    //    {
    //        return BadRequest(ex);
    //    }


    //}

    //[HttpGet("EditPurchase/{id}")]
    //public async Task<IActionResult> EditPurchase(int id)
    //{
    //    try
    //    {
    //        Purchase data = await _service.GetAsync(id);
    //        if (data == null) return NotFound("Data not found");

    //        List<object> stocks = new List<object>();
    //        //foreach (ProductStock item in data.ProductStocks)
    //        //{
    //        //    object stock = new
    //        //    {
    //        //        Variant = item.Variant,
    //        //        Price = item.Price,
    //        //        SKU = item.SKU,
    //        //        Quantity = item.Quantity,
    //        //        Image = item.Image
    //        //    };
    //        //    stocks.Add(stock);
    //        //}
    //        string jsonResult = Newtonsoft.Json.JsonConvert.SerializeObject(stocks);
    //        //data.Variations = jsonResult;

    //        return Ok(data);
    //    }
    //    catch (Exception ex)
    //    {
    //        return BadRequest(ex.Message);
    //    }
    //}

}

