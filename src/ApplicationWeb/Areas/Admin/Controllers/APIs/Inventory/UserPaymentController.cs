using ApplicationCore.Entities.Inventory;
using ApplicationCore.Enums;
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
public class UserPaymentController : ControllerBase
{
    // GET
    private readonly IPaymentService _service;

    public UserPaymentController(IPaymentService service)
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

    [HttpGet("GetsupplierDueAmount/{supplierId}")]
    public async Task<IActionResult> GetsupplierDueAmount(int supplierId)
    {
        try
        {
            return Ok(await _service.GetSupplierDueAmount(supplierId));
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }

    [HttpGet("Edit/{id}")]
    [Authorize(Policy = "PaymentEditPolicy")]
    public async Task<IActionResult> GetPaymentAsync(int id)
    {
        try
        {
            Payment data = await _service.GetAsync(id);
            if (data == null) return NotFound("Data not found");
            return Ok(data);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }

    [HttpPost("Save")]
    [Authorize(Policy = "PaymentCreatePolicy")]
    public async Task<IActionResult> SaveAsync([FromForm] Payment model)
    {
        try
        {
            Payment entity;
            if (model.PaymentId > 0)
            {
                entity = await _service.GetAsync(Convert.ToInt32(model.PaymentId));
                entity.SupplierId = model.SupplierId;
                entity.BranchId = model.BranchId;
                entity.PayDate = model.PayDate;
                entity.Cltype = model.Cltype;
                entity.NextPayDate = model.NextPayDate;
                entity.PayType = model.PayType;
                entity.BankId = model.BankId;
                entity.AccountNo = model.AccountNo;
                entity.ChequeNo = model.ChequeNo;
                entity.ChequeDate = model.ChequeDate;
                entity.PayAmount = model.PayAmount;
                entity.MessageStatus = model.MessageStatus;
                entity.Updated_At = DateTime.Now;
                entity.Updated_By = GetCurrentUserId();
                entity.EntityState = EntityState.Modified;

                entity.Approve = entity.PayType == "cash" ? 1 : 3;
                entity.ApproveBy = entity.PayType == "cash" ? GetCurrentUserId() : string.Empty;
                entity.ApproveState = entity.PayType == "cash" ? "pass" : string.Empty;
                entity.ApproveDate = entity.PayType == "cash" ? entity.PayDate : null;

                entity.Remarks = entity.PayDate.ToString() + " তারিখে " + " ইনভয়েস- " +
                                              entity.PayInvoiceNo + " এ " +
                                              model.SupplierName + " থেকে " +
                                              entity.PayType + " এর মাধ্যমে, " +
                                              entity.PayAmount.ToString() +
                                              "  টাকা প্রদান";
                if (entity.PayType != "cash")
                {
                    entity.Remarks += ", ব্যাংক " + model.BankName + ", চেক নং- " +
                                               entity.ChequeNo + ", তারিখ- " +
                                               entity.ChequeDate.ToString() + " ।";
                }

            }
            else
            {
                entity = model;
                entity.EntryBy = GetCurrentUserId();
                entity.Created_At = DateTime.Now;
                entity.Created_By = GetCurrentUserId();
                entity.EntityState = EntityState.Added;

                entity.Approve = entity.PayType == "cash" ? 1 : 3;
                entity.ApproveBy = entity.PayType == "cash" ? GetCurrentUserId() : string.Empty;
                entity.ApproveState = entity.PayType == "cash" ? "pass" : string.Empty;
                entity.ApproveDate = entity.PayType == "cash" ? entity.PayDate : null;

                //Generate Invoice number
                var invoice = await _service.InvoiceGenerate();
                entity.PayInvoiceNo = invoice;

                entity.Remarks = entity.PayDate.ToString() + " তারিখে " + " ইনভয়েস- " +
                                              entity.PayInvoiceNo + " এ " +
                                              entity.SupplierName + " থেকে " +
                                              entity.PayType + " এর মাধ্যমে, " +
                                              entity.PayAmount.ToString() +
                                              "  টাকা প্রদান";
                if (entity.PayType != "cash")
                {
                    entity.Remarks += ", ব্যাংক " + entity.BankName + ", চেক নং- " +
                                               entity.ChequeNo + ", তারিখ- " +
                                               entity.ChequeDate.ToString() + " ।";
                }

                //If acash deposit status is true....
                //if (entity.PayType == "cash" && entity.MessageStatus == "pending")
                //{
                //    var custo = await _service.GetSupplierById(entity.SupplierId);


                //    var companyInfo = "PogySoft LTD."; // (AutoRice.CompanyInfo)(HttpContext.Current.Session["companyInfo"]);
                //    SMSGateway mobiReach = new SMSGateway();
                //    TextMessage aMessage = new TextMessage();
                //    aMessage.To = custo.MobileNumber;
                //    aMessage.Message = companyInfo + " " + entity.PayDate + " তারিখে " +
                //                       " ইনভয়েস- " + entity.PayInvoiceNo + " এ " +
                //                       custo.OrganizationName + " থেকে " +
                //                       entity.PayType + " এর মাধ্যমে, " +
                //                       entity.PayAmount.ToString() +
                //                       "  টাকা গ্রহন  করা হয়েছে";
                //    MobiReachResponse aResponse = new MobiReachResponse();
                //    aResponse = mobiReach.SendTextMessage(aMessage);
                //    entity.MessageStatus = aResponse.StatusText;
                //}

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
    [Authorize(Policy = "PaymentDeletePolicy")]
    public async Task<IActionResult> DeletePaymentAsync([FromRoute] int id)
    {
        try
        {
            Payment entity = await _service.GetAsync(id);
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