using ApplicationCore.Entities.Marketing;
using ApplicationCore.Enums;
using ApplicationWeb.HelperAndConstant;
using Infrastructure.Interfaces.Marketing;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcomarceOnlineShop.Areas.Admin.Controllers.APIs.Marketing
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Authorize]
    public class CouponController : ControllerBase
    {
        private readonly ICouponService _service;
        private readonly IImageProcessing _image;

        public CouponController(ICouponService service, IImageProcessing imageProcessing)
        {
            _service = service;
            _image = imageProcessing;
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
        public async Task<IActionResult> EditAsync(int id)
        {
            try
            {
                Coupon data = await _service.GetAsync(id);
                if (data == null) return NotFound("Data not found");
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("Save")]
        public async Task<IActionResult> SaveAsync([FromForm] Coupon model)
        {
            try
            {
                Coupon entity;

                if (model.CouponId > 0)
                {
                    entity = await _service.GetAsync(model.CouponId);
                    entity.Code = model.Code;
                    entity.Discount = model.Discount;
                    entity.DiscountType = model.DiscountType;
                    entity.StartDate = model.StartDate;
                    entity.EndDate = model.EndDate;
                    entity.Details = model.Details;
                    entity.IsActive = model.IsActive;

                    entity.Updated_At = DateTime.UtcNow;
                    entity.Updated_By = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
                    entity.UserId = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
                    entity.EntityState = EntityState.Modified;
                }
                else
                {
                    entity = model;
                    entity.Created_At = DateTime.UtcNow;
                    entity.Created_By = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
                    entity.UserId = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
                    entity.EntityState = EntityState.Added;
                }

                entity.StartDate = entity.StartDate.ToUniversalTime();
                entity.EndDate = entity.EndDate.ToUniversalTime();

                await _service.SaveAsync(entity);
                return Ok(entity);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] int id)
        {
            try
            {
                Coupon entity = await _service.GetAsync(id);
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

        [HttpPut("CouponStatus/{CouponId}/{isChecked}")]
        public async Task<IActionResult> UpdateStatus(int CouponId, bool isChecked)
        {
            try
            {
                var data = await _service.GetAsync(CouponId);
                if (data == null) return NotFound("Data not found");
                data.IsActive = isChecked;
                data.EntityState = EntityState.Modified;
                await _service.SaveAsync(data);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("subscriber-list")]
        public async Task<IActionResult> GetSubscriberListAsync()
        {
            var paginationResult = DataTableHandler.PaginationHandler(Request);
            var result = await _service.GetSubscriberListAsync(paginationResult.Item2, paginationResult.Item3, paginationResult.Item4, paginationResult.Item5, paginationResult.Item6);
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
    }
}