using ApplicationCore.Entities.Inventory;
using ApplicationCore.Entities.Products;
using ApplicationCore.Enums;
using ApplicationWeb.HelperAndConstant;
using Infrastructure.Interfaces.Inventory;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ApplicationWeb.Areas.Admin.Controllers.APIs.Inventory;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Authorize]
public class SalesReturnController : ControllerBase
{
    private readonly ISalesReturnService _service;
    private readonly IImageProcessing _image;
    public SalesReturnController(ISalesReturnService salesReturnService, IImageProcessing imageProcessing)
    {
        _service = salesReturnService;
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
    public async Task<IActionResult> SaveAsync([FromForm] SalseReturn model)
    {
        try
        {
            //var salesReturnValidator = new Sa();
            //var result = salesReturnValidator.Validate(model);
            //if (!result.IsValid)
            //{
            //    return BadRequest(result.Errors);
            //}

            SalseReturn entity;

            if (model.SaleReturnId > 0)
            {
                entity = await _service.GetSalseReturnAsync(model.SaleReturnId);

                entity.ProductId = model.ProductId;
                entity.SaleReturnDate = model.SaleReturnDate.HasValue ? model.SaleReturnDate.Value.ToUniversalTime() : (DateTime?)null;
                entity.CustomerId = model.CustomerId;
                entity.Expanse = model.Expanse;
                entity.EntityState = EntityState.Modified;
                entity.Updated_At = DateTime.UtcNow;
                entity.Updated_By = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
                entity.UserId = entity.Updated_By;


                // Product Stock
                List<ProductStock> productStocks = new List<ProductStock>();
                if (!string.IsNullOrEmpty(model.SalseItem))
                {
                    var prodListRtn = JsonSerializer.Deserialize<List<SalseReturnItem>>(model.SalseItem);
                    if (prodListRtn != null && prodListRtn.Count > 0)
                    {
                        foreach (var x in entity.Items)
                        {
                            var stock = await _service.GetProductStockAsync(x.ProductId, x.VariantName);
                            if (stock != null)
                            {
                                stock.Quantity -= x.Quantity;
                                stock.EntityState = EntityState.Modified;
                                productStocks.Add(stock);
                            }
                        }

                        foreach (var x in prodListRtn)
                        {
                            var stock = productStocks.FirstOrDefault(p => p.ProductId == x.ProductId && p.Variant == x.VariantName);
                            if (stock != null)
                            {
                                stock.Quantity += x.Quantity;
                                stock.EntityState = EntityState.Modified;
                            }
                            else
                            {
                                stock = await _service.GetProductStockAsync(x.ProductId, x.VariantName);
                                if (stock != null)
                                {
                                    stock.Quantity += x.Quantity;
                                    stock.EntityState = EntityState.Modified;
                                    productStocks.Add(stock);
                                }
                            }
                        }
                    }
                }




                var updatedItems = JsonSerializer.Deserialize<List<SalseReturnItem>>(model.SalseItem) ?? new List<SalseReturnItem>();
                entity.Items = entity.Items.Where(ei => updatedItems.All(ui => ui.Id != ei.Id)).ToList();
                entity.Items.ForEach(item => item.EntityState = EntityState.Deleted);

                List<SalseReturnItem> newItems = new List<SalseReturnItem>();
                foreach (var item in updatedItems)
                {
                    var existingItem = entity.Items.FirstOrDefault(ei => ei.Id == item.Id);
                    if (existingItem != null)
                    {
                        existingItem.ProductId = item.ProductId;
                        existingItem.VariantId = item.VariantId;
                        existingItem.VariantName = item.VariantName;
                        existingItem.UnitId = item.UnitId;
                        existingItem.Quantity = item.ReturnQuantity;
                        existingItem.SalePrice = item.SalePrice;
                        existingItem.EntityState = EntityState.Modified;
                        newItems.Add(existingItem);
                    }
                    else
                    {
                        item.SaleReturnId = entity.SaleReturnId;
                        item.ProductId = item.ProductId;
                        item.VariantId = item.VariantId;
                        item.VariantName = item.VariantName;
                        item.UnitId = item.UnitId;
                        item.Quantity = item.ReturnQuantity;
                        item.SalePrice = item.SalePrice;
                        item.EntityState = EntityState.Added;
                        newItems.Add(item);
                    }
                }

                entity.Items.AddRange(newItems);
                await _service.UpdateAsync(entity, productStocks);
            }
            else
            {
                entity = model;
                entity.Created_By = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
                entity.UserId = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
                entity.SaleId = model.SaleId;
                if (entity.Photo != null)
                {
                    string fileName = DateTime.UtcNow.ToString("dddd_dd_MMMM_yyyy_HH_mm_ss") + entity.Photo.FileName;
                    string path = _image.GetImagePath(fileName, "Image");
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        model.Photo.CopyTo(stream);
                    }
                    entity.ProductImage = _image.GetImagePathForDb(path);
                }

                //SalesReturn Item
                List<SalseReturnItem> salesReturnItems = new List<SalseReturnItem>();

                if (!string.IsNullOrEmpty(model.SalseItem))
                {
                    var prodList = JsonSerializer.Deserialize<List<SalseReturnItem>>(model.SalseItem);

                    if (prodList != null && prodList.Count > 0)
                    {
                        prodList.ForEach(item =>
                        {
                            salesReturnItems.Add(new SalseReturnItem
                            {
                                ProductId = item.ProductId,
                                VariantId = item.VariantId,
                                VariantName = item.VariantName,
                                UnitId = item.UnitId,
                                Quantity = item.ReturnQuantity,
                                SalePrice = item.SalePrice
                            });
                        });
                    }
                }


                // Product Stock

                List<ProductStock> productStocks = new List<ProductStock>();
                if (!string.IsNullOrEmpty(model.SalseItem))
                {
                    var prodListRtn = JsonSerializer.Deserialize<List<SalseReturnItem>>(model.SalseItem);
                    if (prodListRtn != null && prodListRtn.Count > 0)
                    {
                        foreach (var x in prodListRtn)
                        {
                            var stock = await _service.GetProductStockAsync(x.ProductId, x.VariantName);
                            if (stock != null)
                            {
                                stock.Quantity = stock.Quantity + x.ReturnQuantity;
                                stock.EntityState = EntityState.Modified;

                                productStocks.Add(stock);
                            }
                        }
                    }
                }

                await _service.SaveAsync(entity, salesReturnItems, productStocks);

            }
            return Ok(entity);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }


    }

    [HttpGet("EditSalesReturn/{id}")]
    public async Task<IActionResult> EditSalesReturn(int id)
    {
        try
        {
            SalseReturn data = await _service.GetAsync(id);
            if (data == null) return NotFound("Data not found");
            return Ok(data);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    } 
    [HttpGet("SalesReturnLoad/{id}")]
    public async Task<IActionResult> SalesReturnLoad(int id)
    {
        try
        {
            SalseReturn data = await _service.SalesReturnLoad(id);
            if (data == null) return NotFound("Data not found");
            return Ok(data);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("GetProductVariantList/{id}")]
    public async Task<IActionResult> GetProductVariantList(int id)
    {
        try
        {
            SalseReturn data = await _service.GetProductVariantList(id);
            if (data == null) return NotFound("Data not found");



            return Ok(data);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

}

