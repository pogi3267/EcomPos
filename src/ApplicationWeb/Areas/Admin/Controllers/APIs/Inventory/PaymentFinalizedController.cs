using ApplicationCore.Entities.Inventory;
using ApplicationCore.Enums;
using ApplicationWeb.HelperAndConstant;
using Infrastructure.Interfaces.Inventory;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationWeb.Areas.Admin.Controllers.APIs.Inventory
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Authorize]
    public class PaymentFinalizedController : ControllerBase
    {
        // GET
        private readonly IPaymentFinalizedService _service;

        public PaymentFinalizedController(IPaymentFinalizedService service)
        {
            _service = service;
        }

        private string GetCurrentUserId()
        {
            return User.Claims.First(i => i.Type == "Id").Value;
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
                Payment data = await _service.GetAsync((int)model.PaymentId);
                if (data == null) return NotFound();

                data.Approve = model.ApproveState == "pass" ? 1 : 0;
                data.ApproveDate = model.ApproveDate;
                data.ApproveState = model.ApproveState;
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

        //    if (payments.ApproveState == "pass")
        //            {
        //                var custo = (from sp in db.tblSuppliers
        //                             where sp.SupplierId.Equals(payments.SupplierId)
        //                             select new
        //                             {
        //                                 sp.MobileNo,
        //                                 sp.OrganizationName
        //                             }).FirstOrDefault();

        //    var companyInfo = (AutoRice.CompanyInfo)(HttpContext.Current.Session["companyInfo"]);
        //    MobiReach mobiReach = new MobiReach();
        //    TextMessage aMessage = new TextMessage();
        //    aMessage.To = convertNumber(custo.MobileNo);
        //    aMessage.Message = companyInfo.CompName + " " + payments.PayAmount + " তারিখে " + " ইনভয়েস- " + payments.PayInvoiceNo + " এ " + custo.OrganizationName + " কে " +
        //                                          payments.PayType + " এর মাধ্যমে, " + payments.PayAmount.ToString() +
        //                                            "  টাকা প্রদান  করা হয়েছে" + "ব্যাংক " + payments.BankName + ", চেক নং- " +
        //                                                   payments.ChequeNo + ", তারিখ- " + payments.ChequeDate + " ।"; ;
        //                MobiReachResponse aResponse = new MobiReachResponse();
        //    aResponse = mobiReach.SendTextMessage(aMessage);
        //                payments.MessageStatus = aResponse.StatusText;
        //            }
        //            else
        //            {
        //                var custo = (from sp in db.tblSuppliers
        //                             where sp.SupplierId.Equals(payments.SupplierId)
        //                             select new
        //                             {
        //                                 sp.MobileNo,
        //                                 sp.OrganizationName
        //                             }).FirstOrDefault();

        //var companyInfo = (AutoRice.CompanyInfo)(HttpContext.Current.Session["companyInfo"]);
        //MobiReach mobiReach = new MobiReach();
        //TextMessage aMessage = new TextMessage();
        //aMessage.To = convertNumber(custo.MobileNo);
        //aMessage.Message = companyInfo.CompName + " " + payments.PayAmount + " তারিখে " + " ইনভয়েস- " + payments.PayInvoiceNo + " এ " + custo.OrganizationName + " কে " +
        //                                          payments.PayType + " এর মাধ্যমে, " + payments.PayAmount.ToString() +
        //                                            "  টাকা প্রদান  " + "ব্যাংক " + payments.BankName + ", চেক নং- " +
        //                                                   payments.ChequeNo + ", তারিখ- " + payments.ChequeDate + "  ব্যার্থ হয়েছে ।"; ;
        //                MobiReachResponse aResponse = new MobiReachResponse();
        //aResponse = mobiReach.SendTextMessage(aMessage);
        //                payments.MessageStatus = aResponse.StatusText;
        //            }
    }
}
