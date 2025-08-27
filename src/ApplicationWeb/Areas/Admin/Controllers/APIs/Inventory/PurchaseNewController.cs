using ApplicationWeb.HelperAndConstant;
using Infrastructure.Interfaces.Inventory;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationWeb.Areas.Admin.Controllers.APIs.Inventory;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Authorize]
public class PurchaseNewController : ControllerBase
{
    private readonly IPurchaseService _service;
    private readonly IImageProcessing _image;

    public PurchaseNewController(IPurchaseService purchaseService, IImageProcessing imageProcessing)
    {
        _service = purchaseService;
        _image = imageProcessing;
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
        #region default intialization
        var draw = Convert.ToInt32(Request.Form["draw"].FirstOrDefault());
        var start = Request.Form["start"].FirstOrDefault();
        var length = Request.Form["length"].FirstOrDefault();
        var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
        var sortColumnDir = Request.Form["order[0][dir]"].FirstOrDefault();
        var searchValue = Request.Form["search[value]"].FirstOrDefault();
        int pageSize = length != null ? Convert.ToInt32(length) : 0;
        int skip = start != null ? Convert.ToInt32(start) : 0;
        string status = Request.Form["status"].ToString();
        #endregion default intialization

        var result = await _service.GetDataFromDbase(searchValue, pageSize, skip, sortColumn, sortColumnDir, status);
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

}