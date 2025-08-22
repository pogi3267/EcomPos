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
    public class FlashDealController : ControllerBase
    {
        private readonly IFlashDealService _service;
        private readonly IImageProcessing _image;

        public FlashDealController(IFlashDealService service, IImageProcessing imageProcessing)
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
                FlashDeal data = await _service.GetAsync(id);
                if (data == null) return NotFound("Data not found");
                data.IsStatus = data.Status == 1 ? true : false;
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("Save")]
        public async Task<IActionResult> SaveAsync([FromForm] FlashDeal model)
        {
            try
            {
                FlashDeal entity = model;
                entity.Status = model.IsStatus == true ? 1 : 0;

                if (entity.BannerImage != null)
                {
                    string fileName = DateTime.UtcNow.ToString("dddd_dd_MMMM_yyyy_HH_mm_ss") + entity.BannerImage.FileName;
                    string path = _image.GetImagePath(fileName, "Images");
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        model.BannerImage.CopyTo(stream);
                    }
                    entity.Banner = _image.GetImagePathForDb(path);
                }

                if (entity.FlashDealId > 0)
                {
                    var data = await _service.GetAsync(entity.FlashDealId);
                    if (string.IsNullOrEmpty(entity.Banner) && !string.IsNullOrEmpty(data.Banner)) entity.Banner = data.Banner;
                    entity.Updated_At = DateTime.UtcNow;
                    entity.Updated_By = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
                    entity.EntityState = EntityState.Modified;
                }
                else
                {
                    entity.Created_At = DateTime.UtcNow;
                    entity.Created_By = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
                    entity.EntityState = EntityState.Added;
                }

                entity.StartDate = entity.StartDate.HasValue ? entity.StartDate.Value.ToUniversalTime() : (DateTime?)null;
                entity.EndDate = entity.EndDate.HasValue ? entity.EndDate.Value.ToUniversalTime() : (DateTime?)null;

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
                FlashDeal entity = await _service.GetAsync(id);
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

        [HttpPut("FlashDealStatus/{flashDealId}/{isChecked}")]
        public async Task<IActionResult> UpdateStatus(int flashDealId, bool isChecked)
        {
            try
            {
                var data = await _service.GetAsync(flashDealId);
                if (data == null) return NotFound("Data not found");
                data.Status = isChecked == true ? 1 : 0;
                data.EntityState = EntityState.Modified;
                await _service.SaveAsync(data);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}