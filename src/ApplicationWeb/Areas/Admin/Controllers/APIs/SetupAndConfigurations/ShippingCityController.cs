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
    public class ShippingCityController : ControllerBase
    {
        private readonly IShippingCityService _service;

        public ShippingCityController(IShippingCityService service)
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
        [Authorize(Policy = "CityEditPolicy")]
        public async Task<IActionResult> GetCityAsync(int id)
        {
            try
            {
                City data = await _service.GetAsync(id);
                if (data == null) return NotFound("Data not found");
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("Save")]
        [Authorize(Policy = "CityCreatePolicy")]
        public async Task<IActionResult> SaveAsync([FromForm] City model)
        {
            try
            {
                City entity = model;
                if (entity.CitiesId > 0)
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
        [Authorize(Policy = "CityDeletePolicy")]
        public async Task<IActionResult> DeleteCityAsync([FromRoute] int id)
        {
            try
            {
                City entity = await _service.GetCityEntityAsync(id);
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

        [HttpGet("New")]
        public async Task<IActionResult> GetNew()
        {
            try
            {
                City data = await _service.GetNew();
                if (data == null) return NotFound("Data not found");
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet("GetState/{countryid}")]
        public async Task<IActionResult> GetState(int countryid)
        {
            try
            {
                return Ok(await _service.GetState(countryid));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}