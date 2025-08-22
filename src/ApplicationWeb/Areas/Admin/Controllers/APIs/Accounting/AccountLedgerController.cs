using ApplicationCore.Entities.Accounting;
using ApplicationCore.Enums;
using ApplicationWeb.HelperAndConstant;
using Infrastructure.Interfaces.Accounting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationWeb.Areas.Admin.Controllers.APIs.Accounting;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Authorize]
public class AccountLedgerController : ControllerBase
{
    // GET
    private readonly IAccountLedgerService _service;

    public AccountLedgerController(IAccountLedgerService service)
    {
        _service = service;
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

    [HttpGet("Edit/{id}")]
    [Authorize(Policy = "ChartOfAccountEditPolicy")]
    public async Task<IActionResult> GetAccountLedgerAsync(int id)
    {
        try
        {
            AccountLedger data = await _service.GetAsync(id);
            if (data == null) return NotFound("Data not found");
            return Ok(data);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }

    [HttpPost("Save")]
    [Authorize(Policy = "ChartOfAccountCreatePolicy")]
    public async Task<IActionResult> SaveAsync([FromForm] AccountLedger model)
    {
        try
        {
            AccountLedger entity;
            if (model.Id > 0)
            {
                entity = await _service.GetAsync(model.Id);
                entity.ParentName = model.ParentName;
                entity.Posted = model.Posted;
                entity.EntityState = EntityState.Modified;
                entity.Updated_At = DateTime.Now;
                entity.Updated_By = Global.GetCurrentUser().Id;
            }
            else
            {
                entity = model;
                entity.Created_At = DateTime.Now;
                entity.EntityState = EntityState.Added;
                entity.Created_By = Global.GetCurrentUser().Id;
            }
            int id=await _service.SaveAsync(entity);
            if(id > 0)
            {
                List<AccountLedger> acclist = _service.GetParentIdList(id);
                _service.SaveChartOfCoa(acclist, id);
            }


            return Ok(entity);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }

    [HttpDelete("Delete/{id}")]
    [Authorize(Policy = "ChartOfAccountDeletePolicy")]
    public async Task<IActionResult> DeleteAccountLedgerAsync([FromRoute] int id)
    {
        try
        {
            AccountLedger entity = await _service.GetAsync(id);
            if (entity == null) return NotFound("Data not found!");
            if(entity.Posted) return BadRequest("Posted ledger can't be deleted!");
            entity.EntityState = EntityState.Deleted;
            await _service.SaveAsync(entity);
            return Ok(id);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }

    [HttpGet("GetAccountLedgerCode/{code}")]
    public async Task<IActionResult> GetAccountLedgerCode(string code)
    {
        try
        {
            string ledgerCode = await _service.CodeGenarator(code);
            if (ledgerCode == null) return NotFound("Data not found");
            return Ok(ledgerCode);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }
    [HttpGet("GetParentLedger")]
    public async Task<IActionResult> GetParentLedger()
    {
        try
        {
            var data = await _service.GetAsync();
            if (data == null) return NotFound("Data not found");
            return Ok(data);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }
    [HttpGet("GetAccountLedgerByParentId/{parentId}")]
    public async Task<IActionResult> GetAccountLedgerByParentId(int parentId)
    {
        try
        {
            var data = await _service.GetAccountLedgerByParentId(parentId);
            if (data == null) return NotFound("Data not found");
            return Ok(data);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }
}
