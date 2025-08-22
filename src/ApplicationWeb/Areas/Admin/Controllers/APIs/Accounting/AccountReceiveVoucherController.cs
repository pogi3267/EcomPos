using ApplicationCore.Entities.Accounting;
using ApplicationCore.Enums;
using ApplicationWeb.HelperAndConstant;
using Infrastructure.Interfaces.Accounting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationWeb.Areas.Admin.Controllers.APIs.Accounting
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Authorize]
    public class AccountReceiveVoucherController : ControllerBase
    {
        private readonly IReceiveVoucherService _service;

        public AccountReceiveVoucherController(IReceiveVoucherService service)
        {
            _service = service;
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
        [HttpGet("GetLedgerBalanceById/{accountLedgerId}")]
        public async Task<IActionResult> GetLedgerBalanceById(int accountLedgerId)
        {
            try
            {
                return Ok(await _service.GetLedgerBalanceById(accountLedgerId));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet("GetAccountHeadBySupplierId/{supplierId}")]
        public async Task<IActionResult> GetAccountHeadBySupplierId(int supplierId)
        {
            try
            {
                return Ok(await _service.GetAccountHeadBySupplierId(supplierId));
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
        private string GetCurrentUserId()
        {
            return User.Claims.First(i => i.Type == "Id").Value;
        }

        [HttpPost("Save")]
        //[Authorize(Policy = "ChartOfAccountCreatePolicy")]
        public async Task<IActionResult> SaveAsync([FromForm] AccountVoucher model)
        {
            try
            {
                AccountVoucher entity = model;
                string userId = GetCurrentUserId();
                entity.AccouVoucherTypeAutoID = (int)AccountVoucherType.RECEIEVED;

                

                if (entity.AccountVoucherId > 0)
                {
                    entity = await _service.GetAsync(model.AccountVoucherId);

                    entity.CustomerId = entity.SupplierId;
                    entity.PayeeTo = model.PayeeTo;
                    entity.BranchId = model.BranchId;
                    entity.LedgerBalance = model.LedgerBalance;


                    entity.Updated_By = userId;
                    entity.EntityState = EntityState.Modified;
                    return Ok(await _service.UpdateAsync(entity));
                }
                else
                {
                    entity.VoucherNumber = await _service.InvoiceGenerate();
                    entity.CustomerId = model.SupplierId;
                    entity.SupplierId = null;
                    entity.Created_By = userId;
                    entity.Created_At = DateTime.Now;
                    entity.EntityState = EntityState.Added;
                }
                int id = await _service.SaveAsync(entity);

                return Ok(entity);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet("Edit/{id}")]
        //[Authorize(Policy = "AgentEditPolicy")]
        public async Task<IActionResult> GetReceiveVoucherAsync(int id)
        {
            try
            {
                AccountVoucher data = await _service.GetAsync(id);
                if (data == null) return NotFound("Data not found");
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpDelete("Delete/{id}")]
        //[Authorize(Policy = "AgentEditPolicy")]
        public async Task<IActionResult> DeleteReceiveVoucherAsync(int id)
        {
            try
            {
                if (id < 0) return NotFound("Invalid Id");
                int data = await _service.DeleteAsync(id, GetCurrentUserId());
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}

