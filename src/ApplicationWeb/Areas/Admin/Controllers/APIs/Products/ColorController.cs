using ApplicationCore.Entities.Products;
using ApplicationCore.Enums;
using ApplicationWeb.HelperAndConstant;
using Infrastructure.Interfaces.Products;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationWeb.Areas.Admin.Controllers.APIs.Products
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Authorize]
    public class ColorController : ControllerBase
    {
        private readonly IColorService _service;

        public ColorController(IColorService service)
        {
            _service = service;
        }

        [HttpPost("list")]
        public async Task<IActionResult> GetListPostAsync()
        {
            var paginationResult = DataTableHandler.PaginationHandler(Request);
            var result = await _service.GetDataFromDbase(paginationResult.Item2, paginationResult.Item3, paginationResult.Item4, paginationResult.Item5, paginationResult.Item6);
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
        [Authorize(Policy = "ColorEditPolicy")]
        public async Task<IActionResult> GetColorAsync(int id)
        {
            try
            {
                Color data = await _service.GetAsync(id);
                if (data == null) return NotFound("Data not found");
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("Save")]
        [Authorize(Policy = "ColorCreatePolicy")]
        public async Task<IActionResult> SaveAsync([FromForm] Color model)
        {
            try
            {
                var color = await _service.GetByCodeAsync(model.Code, model.ColorId);
                if (color != null)
                {
                    return BadRequest("This color code already exist!");
                }

                Color entity = model;
                if (entity.ColorId > 0)
                {
                    entity.EntityState = EntityState.Modified;
                }
                else
                {
                    entity.Created_At = DateTime.UtcNow;
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
        [Authorize(Policy = "ColorDeletePolicy")]
        public async Task<IActionResult> DeleteColorAsync([FromRoute] int id)
        {
            try
            {
                Color entity = await _service.GetAsync(id);
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
}
