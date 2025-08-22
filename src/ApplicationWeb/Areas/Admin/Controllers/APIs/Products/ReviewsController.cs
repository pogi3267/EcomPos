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
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewsService _service;

        public ReviewsController(IReviewsService service)
        {
            _service = service;
        }
        [HttpPost("list")]
        public async Task<IActionResult> GetListPostAsync()
        {
            var paginationResult = DataTableHandler.PaginationHandler(Request);
            var result = await _service.GetDataFromDbase(paginationResult.Item2, paginationResult.Item3, paginationResult.Item4, paginationResult.Item5, paginationResult.Item6);
            result.ForEach(r => r.StatusInString = r.Status == 0 ? "Inactive" : "Active");
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
                Reviews data = await _service.GetNewAsync();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet("Edit/{id}")]
        [Authorize(Policy = "ReviewsEditPolicy")]
        public async Task<IActionResult> GetReviewsAsync(int id)
        {
            try
            {
                Reviews data = await _service.GetAsync(id);
                if (data == null) return NotFound("Data not found");
                data.StatusInString = data.Status == 0 ? "Inactive" : "Active";
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }


        [HttpPost("Save")]
        [Authorize(Policy = "ReviewsCreatePolicy")]
        public async Task<IActionResult> SaveAsync([FromForm] Reviews model)
        {

            try
            {
                Reviews entity;
                if (model.ReviewId > 0)
                {
                    entity = await _service.GetAsync(model.ReviewId);

                    entity.Comment = model.Comment;
                    entity.Rating = model.Rating;
                    entity.Status = model.Status;
                    entity.UserId = model.UserId;
                    entity.ProductId = model.ProductId;
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
                return Ok(entity);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        [HttpDelete("Delete/{id}")]
        [Authorize(Policy = "ReviewsDeletePolicy")]
        public async Task<IActionResult> DeleteReviewsAsync([FromRoute] int id)
        {
            try
            {
                Reviews entity = await _service.GetAsync(id);
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
