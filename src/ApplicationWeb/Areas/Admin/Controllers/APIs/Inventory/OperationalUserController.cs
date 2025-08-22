using ApplicationCore.Entities.Inventory;
using ApplicationCore.Enums;
using ApplicationWeb.HelperAndConstant;
using Infrastructure.Interfaces.Inventory;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationWeb.Areas.Admin.Controllers.APIs.Inventory
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Authorize]
    public class OperationalUserController : ControllerBase
    {
        private readonly IOperationalUserService _service;
        private readonly IImageProcessing _imageProcessing;

        public OperationalUserController(IOperationalUserService service, IImageProcessing imageProcessing)
        {
            _service = service;
            _imageProcessing = imageProcessing;
        }

        private string GetCurrentUserId()
        {
            return User.Claims.First(i => i.Type == "Id").Value;
        }

        [HttpGet("GetInitial")]
        public async Task<IActionResult> GetInitial()
        {
            try
            {
                return Ok(await _service.GetInitial());
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("list")]
        public async Task<IActionResult> GetListPostAsync()
        {
            var paginationResult = DataTableHandler.PaginationHandler(Request);
            string roleName = Convert.ToString(Request.Form["roleName"]);
            var result = await _service.GetListAsync(paginationResult.Item2, paginationResult.Item3, paginationResult.Item4, paginationResult.Item5, paginationResult.Item6, roleName);
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
        [Authorize(Policy = "OperationalUserSetupEditPolicy")]
        public async Task<IActionResult> GetOperationalUserAsync(int id)
        {
            try
            {
                OperationalUser data = await _service.GetAsync(id);
                if (data == null) return NotFound("Data not found");
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("Save")]
        [Authorize(Policy = "OperationalUserSetupCreatePolicy")]
        public async Task<IActionResult> SaveAsync([FromForm] OperationalUser model)
        {
            try
            {
                OperationalUser entity = model;
                if (model.Photo != null)
                {
                    var path = _imageProcessing.GetImagePath(model.Photo, "img");
                    using var stream = new FileStream(path, FileMode.Create);
                    model.Photo.CopyTo(stream);
                    entity.ImageUrl = _imageProcessing.GetImagePathForDb(path);
                }

                if (entity.OperationalUserId > 0)
                {
                    entity.Updated_At = DateTime.Now;
                    entity.Updated_By = GetCurrentUserId();
                    entity.EntityState = EntityState.Modified;
                }
                else
                {
                    entity.Created_By = GetCurrentUserId();
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
        [Authorize(Policy = "OperationalUserSetupDeletePolicy")]
        public async Task<IActionResult> DeleteOperationalUserAsync([FromRoute] int id)
        {
            try
            {
                OperationalUser entity = await _service.GetAsync(id);
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
