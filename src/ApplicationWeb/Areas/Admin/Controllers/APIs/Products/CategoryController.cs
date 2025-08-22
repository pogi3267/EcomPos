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
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _service;
        private readonly IImageProcessing _image;

        public CategoryController(ICategoryService service, IImageProcessing imageProcessing)
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

        [HttpGet("GetInitial")]
        public async Task<IActionResult> GetInitial()
        {
            try
            {
                Category data = await _service.GetInitial();
                if (data == null) return NotFound("Data not found");
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet("Edit/{id}")]
        [Authorize(Policy = "CategoryEditPolicy")]
        public async Task<IActionResult> GetAsync(int id)
        {
            try
            {
                Category data = await _service.GetAsync(id);
                if (data == null) return NotFound("Data not found");
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("Save")]
        [Authorize(Policy = "CategoryCreatePolicy")]
        public async Task<IActionResult> SaveAsync([FromForm] Category model)
        {
            try
            {
                Category entity;
                if (model.CategoryId > 0)
                {
                    entity = await _service.GetAsync(model.CategoryId);

                    entity.Name = model.Name;
                    entity.ParentId = model.ParentId;
                    entity.OrderLevel = model.OrderLevel;
                    entity.Digital = model.Digital;
                    entity.Banner = entity.Banner;
                    entity.Icon = entity.Icon;
                    entity.MetaTitle = model.MetaTitle;
                    entity.MetaDescription = model.MetaDescription;
                    entity.Updated_At = DateTime.UtcNow;
                    entity.EntityState = EntityState.Modified;
                }
                else
                {
                    entity = model;
                    entity.Created_At = DateTime.UtcNow;
                    entity.EntityState = EntityState.Added;
                }
                string fileName = "";
                if (model.BannerImage != null)
                {
                    fileName = DateTime.UtcNow.ToString("dddd_dd_MMMM_yyyy_HH_mm_ss") + model.BannerImage.FileName;
                    string path = _image.GetImagePath(fileName, "Image");
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        model.BannerImage.CopyTo(stream);
                    }
                    entity.Banner = _image.GetImagePathForDb(path);
                }

                if (model.IconImage != null)
                {
                    fileName = DateTime.UtcNow.ToString("dddd_dd_MMMM_yyyy_HH_mm_ss") + model.IconImage.FileName;
                    string path = _image.GetImagePath(fileName, "Image");
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        model.IconImage.CopyTo(stream);
                    }
                    entity.Icon = _image.GetImagePathForDb(path);
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
        [Authorize(Policy = "CategoryDeletePolicy")]
        public async Task<IActionResult> DeleteAsync([FromRoute] int id)
        {
            try
            {
                Category entity = await _service.GetAsync(id);
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

        [HttpPut("FeatureUpdate/{categoryId}/{isChecked}")]
        public async Task<IActionResult> UpdateStatus(int categoryId, bool isChecked)
        {
            try
            {
                var data = await _service.GetAsync(categoryId);
                if (data == null) return NotFound("Data not found");
                data.Featured = isChecked == true ? 1 : 0;
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