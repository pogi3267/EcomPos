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
    public class PickupPointsIndexController : ControllerBase
    {
        private readonly IPickupPointsService _service;

        public PickupPointsIndexController(IPickupPointsService service)
        {
            _service = service;
        }

        [HttpPost("list")]
        public async Task<IActionResult> GetListAsync()
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

        [HttpGet("New")]
        public async Task<IActionResult> GetNewAsync()
        {
            try
            {
                PickupPoint data = await _service.GetNewAsync();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        [HttpGet("Edit/{id}")]
        [Authorize(Policy = "PickupPointEditPolicy")]
        public async Task<IActionResult> GetAsync(int id)
        {
            try
            {
                PickupPoint data = await _service.GetAsync(id);
                if (data == null) return NotFound("Data not found");
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        [HttpPost("Save")]
        [Authorize(Policy = "PickupPointCreatePolicy")]
        public async Task<IActionResult> SaveAsync([FromForm] PickupPoint model)
        {
            try
            {
                PickupPoint entity = model;
                if (entity.PickupPointId > 0)
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
                return BadRequest(ex.ToString());
            }
        }

        [HttpDelete("Delete/{id}")]
        [Authorize(Policy = "PickupPointDeletePolicy")]
        public async Task<IActionResult> DeleteAsync([FromRoute] int id)
        {
            try
            {
                PickupPoint entity = await _service.GetAsync(id);
                if (entity == null) return NotFound("Data not found");
                entity.EntityState = EntityState.Deleted;
                await _service.SaveAsync(entity);
                return Ok(id);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        [HttpPut("UpdatePickupPointStatusFeature/{pickupPointId}/{featureName}/{isChecked}")]
        [Authorize(Policy = "PickupPointEditPolicy")]
        public async Task<IActionResult> UpdateProductFeature(int pickupPointId, string featureName, bool isChecked)
        {
            try
            {
                var pickupPoint = await _service.GetAsync(pickupPointId);

                if (featureName == "PickUpStatus")
                {
                    pickupPoint.PickUpStatus = isChecked;
                }
                else if (featureName == "CashOnPickupStatus")
                {
                    pickupPoint.CashOnPickupStatus = isChecked == true ? 1 : 0;
                }

                pickupPoint.EntityState = EntityState.Modified;
                await _service.SaveAsync(pickupPoint);
                return Ok(pickupPointId);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}