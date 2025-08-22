using ApplicationCore.Entities.Products;
using ApplicationCore.Enums;
using Infrastructure.Interfaces.Products;
using Microsoft.AspNetCore.Mvc;

namespace EcomarceOnlineShop.Areas.Admin.Controllers.APIs.Products
{
    [Area("Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductTypeController : ControllerBase
    {
        private readonly IProductTypeService _service;

        public ProductTypeController(IProductTypeService service)
        {
            _service = service;
        }

        [HttpPost("list")]
        public async Task<IActionResult> GetListPostAsync()
        {
            var draw = Convert.ToInt32(Request.Form["draw"].FirstOrDefault());
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDir = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            var result = await _service.GetDataFromDbase(searchValue, pageSize, skip, sortColumn, sortColumnDir);
            int filteredResultsCount = result.Count > 0 ? result[0].TotalRows : 0;
            int totalResultsCount = result.Count > 0 ? result[0].TotalRows : 0;
            return Ok(new
            {
                draw,
                recordsTotal = totalResultsCount,
                recordsFiltered = filteredResultsCount,
                data = result
            });
        }

        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> GetProductTypeAsync(int id)
        {
            try
            {
                ProductType data = await _service.GetAsync(id);
                if (data == null) return NotFound("Data not found");
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }


        [HttpPost("Save")]
        public async Task<IActionResult> SaveAsync([FromForm] ProductType model)
        {
            try
            {
                ProductType entity = model;
                if (entity.ProductTypeId > 0)
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
                return BadRequest(ex.ToString());
            }
        }

        [HttpPost("Delete/{id}")]
        public async Task<IActionResult> DeleteProductTypeAsync([FromRoute] int id)
        {
            try
            {
                ProductType entity = await _service.GetAsync(id);
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
    }
}
