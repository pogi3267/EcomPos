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
    public class CurrencyController : ControllerBase
    {
        private readonly ICurrencyService _service;

        public CurrencyController(ICurrencyService service)
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
        [Authorize(Policy = "CurrencyEditPolicy")]
        public async Task<IActionResult> GetAsync(int id)
        {
            try
            {
                Currency data = await _service.GetAsync(id);
                if (data == null) return NotFound("Data not found");
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("Save")]
        [Authorize(Policy = "CurrencyCreatePolicy")]
        public async Task<IActionResult> SaveAsync([FromForm] Currency model)
        {
            try
            {
                Currency entity;
                if (model.CurrencyId > 0)
                {
                    entity = await _service.GetAsync(model.CurrencyId);

                    entity.Name = model.Name;
                    entity.Symbol = model.Symbol;
                    entity.Code = model.Code;
                    entity.ExchangeRate = model.ExchangeRate;
                    entity.Updated_At = DateTime.UtcNow;
                    entity.EntityState = EntityState.Modified;
                }
                else
                {
                    entity = model;
                    entity.Created_At = DateTime.UtcNow;
                    entity.EntityState = EntityState.Added;
                }

                await _service.SaveAsync(entity);

                if (entity.Status == 1)
                {
                    await _service.UpdateCurrencyStatus(entity.CurrencyId, true);
                }

                return Ok(entity);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpDelete("Delete/{id}")]
        [Authorize(Policy = "CurrencyDeletePolicy")]
        public async Task<IActionResult> DeleteAsync([FromRoute] int id)
        {
            try
            {
                Currency entity = await _service.GetAsync(id);
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

        [HttpPut("CurrencyStatus/{currencyId}/{isChecked}")]
        [Authorize(Policy = "CurrencyEditPolicy")]
        public async Task<IActionResult> UpdateCurrencyStatus(int currencyId, bool isChecked)
        {
            try
            {
                var currency = await _service.GetAsync(currencyId);
                if (currency == null) return NotFound("Currency Not found!");

                await _service.UpdateCurrencyStatus(currencyId, isChecked);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}