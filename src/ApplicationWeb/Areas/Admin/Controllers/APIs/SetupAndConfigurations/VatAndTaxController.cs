using ApplicationCore.Entities.SetupAndConfigurations;
using ApplicationCore.Enums;
using ApplicationWeb.HelperAndConstant;
using Infrastructure.Interfaces.SetupAndConfigurations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcomarceOnlineShop.Areas.Admin.Controllers.APIs.SetupAndConfigurations
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Authorize]
    public class VatAndTaxController : ControllerBase
    {
        private readonly IVatAndTaxService _service;

        public VatAndTaxController(IVatAndTaxService service)
        {
            _service = service;
        }

        [HttpPost("list")]
        public async Task<IActionResult> GetListAsync()
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
        [Authorize(Policy = "VatAndTaxEditPolicy")]
        public async Task<IActionResult> GetAsync(int id)
        {
            try
            {
                Tax data = await _service.GetAsync(id);
                if (data == null) return NotFound("Data not found");
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("Save")]
        [Authorize(Policy = "VatAndTaxCreatePolicy")]
        public async Task<IActionResult> SaveAsync([FromForm] Tax model)
        {
            try
            {
                Tax entity = model;
                if (entity.TaxId > 0)
                {
                    entity.Updated_At = DateTime.UtcNow;
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
        [Authorize(Policy = "VatAndTaxDeletePolicy")]
        public async Task<IActionResult> DeleteTaxAsync([FromRoute] int id)
        {
            try
            {
                Tax entity = await _service.GetAsync(id);
                entity.EntityState = EntityState.Deleted;
                await _service.SaveAsync(entity);
                return Ok(id);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPut("TaxStatus/{taxId}/{isChecked}")]
        [Authorize(Policy = "VatAndTaxEditPolicy")]
        public async Task<IActionResult> UpdateTaxStatus(int taxId, bool isChecked)
        {
            try
            {
                var getTax = await _service.GetAsync(taxId);
                getTax.TaxStatus = (int)(isChecked ? 1 : 0);
                getTax.EntityState = EntityState.Modified;
                await _service.SaveAsync(getTax);
                return Ok(getTax);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}