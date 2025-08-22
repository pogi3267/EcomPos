using ApplicationCore.Entities.Inventory;
using ApplicationCore.Enums;
using ApplicationWeb.HelperAndConstant;
using Infrastructure.Interfaces.Inventory;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationWeb.Areas.Admin.Controllers.APIs.Inventory;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Authorize]
public class ItemController : ControllerBase
{
    // GET
    private readonly IItemService _service;

    public ItemController(IItemService service)
    {
        _service = service;
    }

    [HttpPost("list")]
    public async Task<IActionResult> GetListPostAsync()
    {
        var paginationResult = DataTableHandler.PaginationHandler(Request);
        var result = await _service.GetListAsync(paginationResult.Item2, paginationResult.Item3, paginationResult.Item4, paginationResult.Item5, paginationResult.Item6);
        int filteredResultsCount = result.Count > 0 ? result[0].TotalRows : 0;
        int totalResultsCount = result.Count > 0 ? result[0].TotalRows : 0;
        return Ok(new
        {
            paginationResult.Item1,
            recordsTotal = totalResultsCount,
            recordsFiltered = filteredResultsCount,
            data = result
        });
    }

    [HttpGet("Edit/{id}")]
    [Authorize(Policy = "ItemEditPolicy")]
    public async Task<IActionResult> GetItemAsync(int id)
    {
        try
        {
            Item data = await _service.GetAsync(id);
            if (data == null) return NotFound("Data not found");
            return Ok(data);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }

    [HttpPost("Save")]
    [Authorize(Policy = "ItemCreatePolicy")]
    public async Task<IActionResult> SaveAsync([FromForm] Item model)
    {
        try
        {
            Item entity = model;
            if (entity.ItemId > 0)
            {
                entity.EntityState = EntityState.Modified;
            }
            else
            {
                entity.Created_At = DateTime.Now;
                entity.EntityState = EntityState.Added;
            }
            await _service.SaveAsync(entity);
            return Ok(entity);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }

    [HttpDelete("Delete/{id}")]
    [Authorize(Policy = "ItemDeletePolicy")]
    public async Task<IActionResult> DeleteItemAsync([FromRoute] int id)
    {
        try
        {
            Item entity = await _service.GetAsync(id);
            if (entity == null) return NotFound("Data not found");
            entity.EntityState = EntityState.Deleted;
            await _service.SaveAsync(entity);
            return Ok(id);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }
}