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
using System.Collections.Generic;
using System.Text.Json;

namespace ApplicationWeb.Areas.Admin.Controllers.APIs.Inventory;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Authorize]
public class PurchaseReturnController : ControllerBase
{
    private readonly IPurchaseReturnService _service;
    private readonly IImageProcessing _image;
    public PurchaseReturnController(IPurchaseReturnService purchaseService, IImageProcessing imageProcessing)
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

    [HttpGet("GetAttribute/{search}")]
    public async Task<IActionResult> GetAttribute(string search)
    {
        return Ok(await _service.GetAttribute(search));
    }

    [HttpPost("Save")]
    public async Task<IActionResult> SaveAsync([FromForm] PurchaseReturn model)
    {
        try
        {
            //var purchaseValidator = new PurchaseValidator();
            //var result = purchaseValidator.Validate(model);
            //if (!result.IsValid)
            //{
            //    return BadRequest(result.Errors);
            //}

            PurchaseReturn entity;

            if (model.PurchaseRetuenId > 0)
            {
                entity = await _service.GetPurchaseAsync(model.PurchaseRetuenId);
                entity.ProductImage = model.ProductImage;
                entity.EntityState = EntityState.Modified;
                entity.Updated_At = DateTime.UtcNow;
                entity.Updated_By = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
                entity.UserId = entity.Updated_By;

               
                List<ProductStock> proStock = await _service.GetRtnEditStockAsync(model.PurchaseRetuenId);
                
                List<PurchaseReturnItem> rItem = entity.Items;


                // Product Stock
                List<ProductStock> productStocks = new List<ProductStock>();
                if (!string.IsNullOrEmpty(model.Variation))
                {
                    var prodListRtn = JsonSerializer.Deserialize<List<ProductStock>>(model.Variation);
                    if (prodListRtn != null && prodListRtn.Count > 0)
                    {
                        foreach (var x in entity.Items)
                        {
                            var stock = proStock.Where(p => p.ProductId == model.ProductId && p.Variant == x.VariantName).FirstOrDefault();
                            if (stock != null)
                            {
                                stock.ProductId = model.ProductId;
                                stock.Variant = x.VariantName;
                                stock.Quantity += x.Quantity;
                                await _service.UpdateProductStockAsync(stock);
                            }
                        }

                        foreach (var newStock in prodListRtn)
                        {
                            newStock.Created_By = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
                            newStock.Variant = newStock.Variant.Replace(" ", "");
                            newStock.Price = 0;

                            var existingStock = await _service.GetProductStockAsync(model.ProductId, newStock.Variant);

                            if (existingStock != null)
                            {
                                existingStock.Quantity -= newStock.ReturnQuantity;
                                await _service.UpdateProductStockAsync(existingStock);
                            }
                            
                        }


                    }
                }

                // Product Return Stock
                //if (!string.IsNullOrEmpty(model.Variation))
                //{
                //    var prodListRtn = JsonSerializer.Deserialize<List<ProductStock>>(model.Variation);
                //    if (prodListRtn != null && prodListRtn.Count > 0)
                //    {
                //        // First, add the returned items back to the stock
                //        foreach (var returnItem in rItem)
                //        {
                //            var stock = proStock.FirstOrDefault(p => p.Variant == returnItem.VariantName);
                //            if (stock != null)
                //            {
                //                stock.ReturnQuantity += returnItem.Quantity;
                //                stock.EntityState = EntityState.Modified;

                //                productStocks.Add(stock);
                //            }
                //        }

                //        // Then, subtract the new variation quantities from the stock
                //        foreach (var x in prodListRtn)
                //        {
                //            var stock = proStock.FirstOrDefault(p => p.Variant == x.Variant);
                //            if (stock != null)
                //            {
                //                stock.Quantity -= x.ReturnQuantity;
                //                stock.EntityState = EntityState.Modified;

                //                productStocks.Add(stock);
                //            }
                //        }
                //    }
                //}

                var existingItems = await _service.GetPurchaseItemsByPurchaseIdAsync(entity.PurchaseRetuenId);
                var updatedItems = JsonSerializer.Deserialize<List<PurchaseReturnItem>>(model.Variation) ?? new List<PurchaseReturnItem>();
                // Delete items that are in the database but not in the new list
                entity.Items = existingItems.Where(ei => updatedItems.All(ui => ui.Id == ei.Id)).ToList();
                entity.Items.ForEach(item => item.EntityState = EntityState.Deleted);

                // Update existing items and add new ones
                List<PurchaseReturnItem> newItems = new List<PurchaseReturnItem>();
                foreach (var item in updatedItems)
                {
                    var existingItem = existingItems.FirstOrDefault(ei => ei.Id == item.Id);
                    if (existingItem != null)
                    {
                        existingItem.Quantity = item.ReturnQuantity;
                        existingItem.EntityState = EntityState.Modified;
                        entity.Items.Add(existingItem);
                    }
                   
                }

                entity.Items.AddRange(newItems); // Add new items
                await _service.UpdateAsync(entity);
            }
            else
            {

                entity = model;
                entity.Created_By = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
                entity.UserId = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
                entity.Variations = model.Variation;
                entity.ProductImage = model.ProductImage;
                

                //Purchase Item
                List<PurchaseReturnItem> purchaseItems = new List<PurchaseReturnItem>();

                if (!string.IsNullOrEmpty(model.Variation))
                {
                    var prodList = JsonSerializer.Deserialize<List<ProductStock>>(model.Variation);

                    if (prodList != null && prodList.Count > 0)
                    {
                        prodList.ForEach(item =>
                        {
                            purchaseItems.Add(new PurchaseReturnItem
                            {
                                VariantName = item.Variant.Replace(" ", ""),
                                Quantity = Convert.ToInt32(item.ReturnQuantity),
                                VariantPrice = item.Price,
                                PurchaseQuantity = item.PurchaseQuantity,
                            });
                        });
                    }
                }


                // Product Stock

                List<ProductStock> productStocks = new List<ProductStock>();
                List<ProductStock> proStock = await _service.GetPurchaseStockAsync(model.PurchaseId);

                // Product Return Stock
                if (!string.IsNullOrEmpty(model.Variation))
                {
                    var prodListRtn = JsonSerializer.Deserialize<List<ProductStock>>(model.Variation);
                    if (prodListRtn != null && prodListRtn.Count > 0)
                    {
                        foreach (var x in prodListRtn)
                        {
                            var stock = proStock.FirstOrDefault(p => p.Variant == x.Variant);
                            if (stock != null)
                            {
                                stock.Quantity = stock.Quantity - x.ReturnQuantity;
                                stock.EntityState = EntityState.Modified;

                                productStocks.Add(stock);
                            }
                        }
                    }
                }

                await _service.SaveAsync(entity, purchaseItems, productStocks);
            }
            return Ok(entity);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }

    [HttpGet("PurchaseReturnLoad/{id}")]
    public async Task<IActionResult> PurchaseReturnLoad(int id)
    {
        try
        {
            PurchaseReturn data = await _service.GetPurchaseForEditAsync(id);
            if (data == null) return NotFound("Data not found");

            return Ok(data);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }



    [HttpGet("EditPurchase/{id}")]
    public async Task<IActionResult> EditPurchase(int id)
    {
        try
        {
            PurchaseReturn data = await _service.GetAsync(id);
            if (data == null) return NotFound("Data not found");
            return Ok(data);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

}

