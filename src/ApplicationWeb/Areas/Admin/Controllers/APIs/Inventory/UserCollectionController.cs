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
public class UserCollectionController : ControllerBase
{
    // GET
    private readonly ICollectionService _service;

    public UserCollectionController(ICollectionService service)
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

    [HttpGet("GetCustomerDueAmount/{customerId}")]
    public async Task<IActionResult> GetCustomerDueAmount(int customerId)
    {
        try
        {
            return Ok(await _service.GetCustomerDueAmount(customerId));
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }

    [HttpGet("Edit/{id}")]
    [Authorize(Policy = "CollectionEditPolicy")]
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
    [Authorize(Policy = "CollectionCreatePolicy")]
    public async Task<IActionResult> SaveAsync([FromForm] Collection model)
    {
        try
        {
            Collection entity;
            if (model.CollectionId > 0)
            {
                entity = await _service.GetAsync(Convert.ToInt32(model.CollectionId));
                entity.CustomerId = model.CustomerId;
                entity.BranchId = model.BranchId;
                entity.CollectionDate = model.CollectionDate;
                entity.ClType = model.ClType;
                entity.SalesNo = model.SalesNo;
                entity.MessageStatus = model.MessageStatus;
                entity.Collectiontype = model.Collectiontype;
                entity.BankId = model.BankId;
                entity.ChequeNo = model.ChequeNo;
                entity.ChequeDate = model.ChequeDate;
                entity.CollectionAmount = model.CollectionAmount;
                entity.EntityState = EntityState.Modified;
                entity.Updated_At = DateTime.Now;
                entity.Updated_By = GetCurrentUserId();

                entity.Approved = entity.Collectiontype == "cash" ? true : false;
                entity.FinalizedBy = entity.Collectiontype == "cash" ? GetCurrentUserId() : string.Empty;
                entity.FinalizedStatus = entity.Collectiontype == "cash" ? "pass" : string.Empty;
                entity.FinalizedDate = entity.Collectiontype == "cash" ? entity.CollectionDate : null;
                entity.DepositStatus = entity.Collectiontype == "cash" ? "Y" : "N";

                entity.ColRemark = entity.CollectionDate.ToString() + " তারিখে " + " ইনভয়েস- " +
                                              entity.InvoiceNoCollection + " এ " +
                                              model.CustomerName + " থেকে " +
                                              entity.Collectiontype + " এর মাধ্যমে, " +
                                              entity.CollectionAmount.ToString() +
                                              "  টাকা গ্রহন";
                if (entity.Collectiontype != "cash")
                {
                    entity.ColRemark += ", ব্যাংক " + model.BankName + ", চেক নং- " +
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

                entity.Approved = entity.Collectiontype == "cash" ? true : false;
                entity.FinalizedBy = entity.Collectiontype == "cash" ? GetCurrentUserId() : string.Empty;
                entity.FinalizedStatus = entity.Collectiontype == "cash" ? "pass" : string.Empty;
                entity.FinalizedDate = entity.Collectiontype == "cash" ? entity.CollectionDate : null;
                entity.DepositStatus = entity.Collectiontype == "cash" ? "Y" : "N";

                //Generate Invoice number
                var invoice = await _service.InvoiceGenerate();
                entity.InvoiceNoCollection = invoice;

                entity.ColRemark = entity.CollectionDate.ToString() + " তারিখে " + " ইনভয়েস- " +
                                              entity.InvoiceNoCollection + " এ " +
                                              entity.CustomerName + " থেকে " +
                                              entity.Collectiontype + " এর মাধ্যমে, " +
                                              entity.CollectionAmount.ToString() +
                                              "  টাকা গ্রহন";
                if (entity.Collectiontype != "cash")
                {
                    entity.ColRemark += ", ব্যাংক " + entity.BankName + ", চেক নং- " +
                                               entity.ChequeNo + ", তারিখ- " +
                                               entity.ChequeDate.ToString() + " ।";
                }

                //If acash deposit status is true....
                //if (entity.Collectiontype == "cash" && entity.MessageStatus == "pending")
                //{
                //    var custo = await _service.GetCustomerById(entity.CustomerId);


                //    var companyInfo = "PogySoft LTD."; // (AutoRice.CompanyInfo)(HttpContext.Current.Session["companyInfo"]);
                //    SMSGateway mobiReach = new SMSGateway();
                //    TextMessage aMessage = new TextMessage();
                //    aMessage.To = custo.MobileNumber;
                //    aMessage.Message = companyInfo + " " + entity.CollectionDate + " তারিখে " +
                //                       " ইনভয়েস- " + entity.InvoiceNoCollection + " এ " +
                //                       custo.OrganizationName + " থেকে " +
                //                       entity.Collectiontype + " এর মাধ্যমে, " +
                //                       entity.CollectionAmount.ToString() +
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
    [Authorize(Policy = "CollectionDeletePolicy")]
    public async Task<IActionResult> DeleteCollectionAsync([FromRoute] int id)
    {
        try
        {
            Collection entity = await _service.GetAsync(id);
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