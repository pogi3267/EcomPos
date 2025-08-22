using ApplicationCore.Entities.Products;
using ApplicationCore.Enums;
using ApplicationWeb.HelperAndConstant;
using Infrastructure.Interfaces.Products;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcomarceOnlineShop.Areas.Admin.Controllers.APIs.Products
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Authorize]
    public class BrandController : ControllerBase
    {
        private readonly IBrandService _service;
        private readonly IImageProcessing _image;

        public BrandController(IBrandService service, IImageProcessing imageProcessing)
        {
            _service = service;
            _image = imageProcessing;
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
        [Authorize(Policy = "BrandEditPolicy")]
        public async Task<IActionResult> GetAsync(int id)
        {
            try
            {
                Brand data = await _service.GetAsync(id);
                if (data == null) return NotFound("Data not found");
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("Save")]
        [Authorize(Policy = "BrandCreatePolicy")]
        public async Task<IActionResult> SaveAsync([FromForm] Brand model)
        {
            try
            {
                Brand entity = model;
                if (entity.Photo != null)
                {
                    string fileName = DateTime.UtcNow.ToString("dddd_dd_MMMM_yyyy_HH_mm_ss") + entity.Photo.FileName;
                    string path = _image.GetImagePath(fileName, "Image");
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        model.Photo.CopyTo(stream);
                    }
                    entity.Logo = _image.GetImagePathForDb(path);
                }

                if (entity.BrandId > 0)
                {
                    var existing = await _service.GetAsync(model.BrandId);
                    if (entity.Logo == "" || entity.Logo == null) entity.Logo = existing.Logo;
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
        [Authorize(Policy = "BrandDeletePolicy")]
        public async Task<IActionResult> DeleteColorAsync([FromRoute] int id)
        {
            try
            {
                Brand entity = await _service.GetAsync(id);
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