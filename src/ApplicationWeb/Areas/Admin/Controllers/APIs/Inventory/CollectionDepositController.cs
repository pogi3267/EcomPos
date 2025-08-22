using ApplicationCore.Entities.Inventory;
using ApplicationCore.Enums;
using ApplicationWeb.HelperAndConstant;
using Infrastructure.Interfaces.Inventory;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace ApplicationWeb.Areas.Admin.Controllers.APIs.Inventory
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Authorize]
    public class CollectionDepositController : ControllerBase
    {
        // GET
        private readonly ICollectionDepositService _service;

        public CollectionDepositController(ICollectionDepositService service)
        {
            _service = service;
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
            try
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
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
            
        }

        [HttpGet("Edit/{id}")]
        //[Authorize(Policy = "CollectionEditPolicy")]
        public async Task<IActionResult> GetCollectionAsync(int id)
        {
            try
            {
                Collection data = await _service.GetAsync(id);
                if (data == null) return NotFound("Data not found");
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("Save")]
        //[Authorize(Policy = "CollectionCreatePolicy")]
        public async Task<IActionResult> SaveAsync([FromForm] Collection model)
        {
            try
            {
                Collection data = await _service.GetAsync((int)model.CollectionId);
                if (data == null) return NotFound();

                data.DepositBankId = model.DepositBankId;
                data.DepositAccountNumberId = model.DepositAccountNumberId;
                data.DepositDate = model.DepositDate;
                data.DepositStatus = "Y";
                data.EntityState = EntityState.Modified;
                data.Updated_At = DateTime.Now;
                data.Updated_By = Global.GetCurrentUser().Id;
                    
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
