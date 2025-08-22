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
public class AdjustmentController : ControllerBase
{
    private readonly IAdjustmentService _service;
    private readonly IOperationalUserService _operationalUserService;

    public AdjustmentController(IAdjustmentService AdjustmentService,
        IOperationalUserService operationalUserService
        )
    {
        _service = AdjustmentService;
        _operationalUserService = operationalUserService;
    }

    private string GetCurrentUserId()
    {
        return User.Claims.First(i => i.Type == "Id").Value;
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

    [HttpPost("Save")]
    [Authorize(Policy = "AdjustmentCreatePolicy")]
    public async Task<IActionResult> SaveAsync([FromForm] Adjustment model)
    {
        try
        {
            string userId = GetCurrentUserId();
            if (model.AdjustmentId > 0)
            {
                model.Updated_By = userId;
                model.EntityState = EntityState.Modified;
            }
            else
            {
                model.Created_By = userId;
                model.EntityState = EntityState.Added;
            }
            await _service.SaveAsync(model);
            return Ok(model);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }

    [HttpGet("Edit/{id}")]
    [Authorize(Policy = "AdjustmentEditPolicy")]
    public async Task<IActionResult> GetAdjustmentAsync(int id)
    {
        try
        {
            var data = await _service.GetAsync(id);
            if (data == null) return NotFound("Data not found");
            return Ok(data);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }

    [HttpDelete("Delete/{id}")]
    [Authorize(Policy = "AdjustmentDeletePolicy")]
    public async Task<IActionResult> DeleteAdjustmentAsync([FromRoute] int id)
    {
        try
        {
            var entity = await _service.GetAsync(id);
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

    [HttpGet("GetOperationalUserByRole/{roleName}")]
    public async Task<IActionResult> GetOperationalUserByRole(string roleName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(roleName)) return NotFound("Data not found");
            var entity = await _operationalUserService.GetOperationalUserByRole(roleName);
            return Ok(entity);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }
}