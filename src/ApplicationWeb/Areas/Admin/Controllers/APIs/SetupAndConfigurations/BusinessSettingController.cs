using ApplicationCore.Entities.Marketing;
using ApplicationCore.Entities.Products;
using ApplicationCore.Entities.SetupAndConfigurations;
using ApplicationCore.Enums;
using ApplicationWeb.HelperAndConstant;
using ApplicationWeb.Security;
using Infrastructure.Interfaces.Marketing;
using Infrastructure.Interfaces.Public;
using Infrastructure.Interfaces.SetupAndConfigurations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;
using System.Text.Json;
using static Dapper.SqlMapper;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace EcomarceOnlineShop.Areas.Admin.Controllers.APIs.SetupAndConfigurations
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Authorize]
    public class BusinessSettingController : ControllerBase
    {
        private readonly IBusinessSettingService _service;
        private readonly IGeneralService _generalService;
        private readonly IFlashDealService _flashDealService;
        private readonly IImageProcessing _image;

        public BusinessSettingController(IBusinessSettingService service, IGeneralService generalService, IFlashDealService flashDealService, IImageProcessing imageProcessing)
        {
            _service = service;
            _generalService = generalService;
            _flashDealService = flashDealService;
            _image = imageProcessing;
        }

        [HttpGet("Get-SmtpSetting/{type}")]
        public async Task<IActionResult> GetAsync(string type)
        {
            try
            {
                BusinessSetting data = await _service.GetAsync(type);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        [HttpGet("Get-SmtpSettings/{types}")]
        public async Task<IActionResult> GetsAsync(string types)
        {
            try
            {
                var data = await _service.GetsAsync(types);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        [HttpPost("Save")]
        public async Task<IActionResult> SaveAsync([FromForm] BusinessSetting model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Type))
                    throw new Exception("Type couldn't be empty!!");
                BusinessSetting entity = await _service.GetAsync(model.Type);
                if (entity == null)
                {
                    entity = model;
                    entity.Created_At = DateTime.UtcNow;
                    entity.EntityState = EntityState.Added;
                }
                else
                {
                    entity.Type = model.Type;
                    entity.Value = model.Value;
                    entity.Lang = model.Lang;
                    entity.Updated_At = DateTime.UtcNow;
                    entity.EntityState = EntityState.Modified;
                }
                await _service.SaveAsync(entity);
                return Ok(entity);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        [HttpPost("SaveMultiple")]
        public async Task<IActionResult> SaveMultipleAsync(List<BusinessSetting> models)
        {
            try
            {
                List<BusinessSetting> entities = new List<BusinessSetting>();
                foreach (var model in models)
                {
                    if (string.IsNullOrEmpty(model.Type))
                        throw new Exception("Type couldn't be empty!!");

                    BusinessSetting entity = await _service.GetAsync(model.Type);
                    if (entity == null)
                    {
                        entity = model;
                        entity.Created_At = DateTime.UtcNow;
                        entity.EntityState = EntityState.Added;
                    }
                    else
                    {
                        entity.Type = model.Type;
                        entity.Value = model.Value;
                        entity.Lang = model.Lang;
                        entity.Updated_At = DateTime.UtcNow;
                        entity.EntityState = EntityState.Modified;
                    }
                    entities.Add(entity);
                }
                await _service.SaveMultipleAsync(entities);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        [HttpGet("Get-Settings/{types}")]
        public async Task<IActionResult> GetsSettingsAsync(string types)
        {
            try
            {
                var data = await _service.GetsAsync(types);
                BusinessSetting businessSetting = await _service.GetsImagesAsync();
                businessSetting.Settings = data;

                return Ok(businessSetting);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        [HttpGet("Get-Image-Settings/{types}")]
        public async Task<IActionResult> GetsImageSettingsAsync(string types)
        {
            try
            {
                BusinessSetting businessSetting = await _service.GetsImagesAsync();
                return Ok(businessSetting);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        [HttpGet("Get-setting-value/{type}/{key}")]
        public async Task<IActionResult> GetSettingValueAsync(string type, string key)
        {
            try
            {
                BusinessSetting businessSetting = await _service.GetAsync(type);

                using JsonDocument document = JsonDocument.Parse(businessSetting.Value);
                JsonElement root = document.RootElement;

                string? stripeKey = root.GetProperty(key).GetString();
                return Ok(stripeKey);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        [HttpGet("Get-timezone-Settings/{types}")]
        public async Task<IActionResult> GetsTimeZoneSettingsAsync(string types)
        {
            try
            {
                var data = await _service.GetsAsync(types);
                BusinessSetting businessSetting = new BusinessSetting();
                businessSetting.Settings = data;

                businessSetting.Timezones = TimeZoneInfo.GetSystemTimeZones().ToList();

                return Ok(businessSetting);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        [HttpGet("Get-home-Settings/{types}")]
        public async Task<IActionResult> GetsHomePageSettingsAsync(string types)
        {
            try
            {
                var data = await _service.GetsAsync(types);
                BusinessSetting businessSetting = new BusinessSetting();
                businessSetting.Settings = data;

                List<Category> categories = await _generalService.GetCategoriesAsync();
                List<FlashDeal> flashDeals = await _flashDealService.GetActiveFlashDEals();
                businessSetting.Categories = categories;
                businessSetting.FlashDeals = flashDeals;

                return Ok(businessSetting);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        [HttpPost("SaveImages")]
        public async Task<IActionResult> SaveImagesAsync([FromForm] BusinessSetting model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Type))
                    throw new Exception("Type couldn't be empty!!");
                BusinessSetting entity = await _service.GetAsync(model.Type);

                if (entity == null)
                {
                    entity = model;
                    entity.Created_At = DateTime.UtcNow;
                    entity.EntityState = EntityState.Added;
                }
                else
                {
                    entity.Type = model.Type;
                    //entity.Value = model.Value;
                    entity.Lang = model.Lang;
                    entity.Updated_At = DateTime.UtcNow;
                    entity.EntityState = EntityState.Modified;
                }

                List<string> fileNames = new List<string>();
                if (model.Images.Count > 0)
                {
                    foreach (var item in model.Images)
                    {
                        string fileName = DateTime.UtcNow.ToString("dddd_dd_MMMM_yyyy_HH_mm_ss") + item.FileName;
                        string path = _image.GetImagePath(fileName, "Image");
                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            item.CopyTo(stream);
                        }
                        fileNames.Add(_image.GetImagePathForDb(path));
                    }
                    entity.Value = string.Join(",", fileNames.Select(x => x));
                }

                await _service.SaveAsync(entity);
                return Ok(entity);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("SaveBannerImages")]
        public async Task<IActionResult> SaveBannerImagesAsync([FromForm] BusinessSetting model)
        {
            try
            {
                List<BusinessSetting> entities = new List<BusinessSetting>();

                if (string.IsNullOrEmpty(model.Type) || string.IsNullOrEmpty(model.Type2) || string.IsNullOrEmpty(model.Type3))
                    throw new Exception("Type couldn't be empty!!");

                BusinessSetting entity = await _service.GetAsync(model.Type);
                if (entity == null)
                {
                    entity = new BusinessSetting();
                    entity.Type = model.Type;
                    entity.Value = "";
                    entity.Created_At = DateTime.UtcNow;
                    entity.EntityState = EntityState.Added;
                }
                else
                {
                    entity.Type = model.Type;
                    entity.Updated_At = DateTime.UtcNow;
                    entity.EntityState = EntityState.Modified;
                }

                List<string> fileNames = new List<string>();
                if (model.Images.Count > 0)
                {
                    foreach (var item in model.Images)
                    {
                        string fileName = DateTime.UtcNow.ToString("dddd_dd_MMMM_yyyy_HH_mm_ss") + item.FileName;
                        string path = _image.GetImagePath(fileName, "Image");
                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            item.CopyTo(stream);
                        }
                        fileNames.Add(_image.GetImagePathForDb(path));
                    }
                    entity.Value = string.Join(",", fileNames.Select(x => x));
                }
                entities.Add(entity);


                entity = await _service.GetAsync(model.Type2);
                if (entity == null)
                {
                    entity = new BusinessSetting();
                    entity.Type = model.Type2;
                    entity.Value = "";
                    entity.Created_At = DateTime.UtcNow;
                    entity.EntityState = EntityState.Added;
                }
                else
                {
                    entity.Type = model.Type2;
                    entity.Updated_At = DateTime.UtcNow;
                    entity.EntityState = EntityState.Modified;
                }

                fileNames = new List<string>();
                if (model.Images2.Count > 0)
                {
                    foreach (var item in model.Images2)
                    {
                        string fileName = DateTime.UtcNow.ToString("dddd_dd_MMMM_yyyy_HH_mm_ss") + item.FileName;
                        string path = _image.GetImagePath(fileName, "Image");
                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            item.CopyTo(stream);
                        }
                        fileNames.Add(_image.GetImagePathForDb(path));
                    }
                    entity.Value = string.Join(",", fileNames.Select(x => x));
                }
                entities.Add(entity);

                entity = await _service.GetAsync(model.Type3);
                if (entity == null)
                {
                    entity = new BusinessSetting();
                    entity.Type = model.Type3;
                    entity.Value = "";
                    entity.Created_At = DateTime.UtcNow;
                    entity.EntityState = EntityState.Added;
                }
                else
                {
                    entity.Type = model.Type3;
                    entity.Updated_At = DateTime.UtcNow;
                    entity.EntityState = EntityState.Modified;
                }

                fileNames = new List<string>();
                if (model.Images3.Count > 0)
                {
                    foreach (var item in model.Images3)
                    {
                        string fileName = DateTime.UtcNow.ToString("dddd_dd_MMMM_yyyy_HH_mm_ss") + item.FileName;
                        string path = _image.GetImagePath(fileName, "Image");
                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            item.CopyTo(stream);
                        }
                        fileNames.Add(_image.GetImagePathForDb(path));
                    }
                    entity.Value = string.Join(",", fileNames.Select(x => x));
                }
                entities.Add(entity);


                await _service.SaveMultipleAsync(entities);
                return Ok();

            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [AllowAnonymous]
        [HttpGet("get-key")]
        public async Task<IActionResult> GetKeyValueAsync(int key) 
        {
            try
            {
                string keyType = "", pk = "", sk = "";
                if (key == 1)
                {
                    keyType = "google_login";
                    pk = "clientId";
                    sk = "clientSecret";
                }
                else if (key == 2)
                {
                    keyType = "facebook_login";
                    pk = "clientId";
                    sk = "clientSecret";
                }
                else if (key == 3)
                {
                    keyType = "stripe_payment";
                    pk = "stripeKey";
                    sk = "stripeSecret";
                }
                else if (key == 4)
                {
                    keyType = "paypal_payment";
                    pk = "clientId";
                    sk = "clientSecret";
                }

                if (string.IsNullOrEmpty(keyType) || string.IsNullOrEmpty(pk) || string.IsNullOrEmpty(sk))
                {
                    return BadRequest("Invalid request!");
                }

                BusinessSetting businessSetting = await _service.GetAsync(keyType);
                if (string.IsNullOrEmpty(businessSetting.Value))
                {
                    return BadRequest("No value found!");
                }

                using JsonDocument document = JsonDocument.Parse(businessSetting.Value);
                JsonElement root = document.RootElement;

                string? pkValue = root.GetProperty(pk).GetString();
                string? skValue = root.GetProperty(sk).GetString();

                if (key == 4)
                {
                    if (string.IsNullOrEmpty(pkValue) || string.IsNullOrEmpty(skValue))
                    {
                        keyType = "paypal_sandbox";
                        businessSetting = await _service.GetAsync(keyType);
                        if (string.IsNullOrEmpty(businessSetting.Value))
                        {
                            return BadRequest("No value found!");
                        }

                        root = JsonDocument.Parse(businessSetting.Value).RootElement;

                        pkValue = root.GetProperty(pk).GetString();
                        skValue = root.GetProperty(sk).GetString();
                    }
                }

                if (string.IsNullOrEmpty(pkValue) || string.IsNullOrEmpty(skValue))
                {
                    return BadRequest("No value found!");
                }

                return Ok(new { pk = AESEncryption.Encrypt(pkValue), sk = AESEncryption.Encrypt(skValue) });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }
      
        [HttpPost("Send-Test-Mail/{type}/{testMail}")]
        public IActionResult SendEmail(string type, string testMail)
        {
            try
            {
                // Configure SMTP client
                using (var client = new SmtpClient("smtp.gmail.com", 587))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential("saurove@diu.edu.bd", "132152752.");

                    // Create the email message
                    var mailMessage = new MailMessage();
                    mailMessage.From = new MailAddress("saurove@diu.edu.bd");
                    mailMessage.To.Add("saurove07@gmail.com");
                    mailMessage.Subject = "Hello";
                    mailMessage.Body = "This is the email body.";

                    // Send the email
                    client.Send(mailMessage);
                }

                return Ok("Email sent successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while sending the email: {ex.Message}");
            }
        }
    }
}