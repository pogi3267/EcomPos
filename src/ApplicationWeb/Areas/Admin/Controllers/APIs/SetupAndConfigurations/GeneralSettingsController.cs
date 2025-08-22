using ApplicationCore.Entities.SetupAndConfigurations;
using ApplicationCore.Enums;
using ApplicationWeb.HelperAndConstant;
using Infrastructure.Interfaces.SetupAndConfigurations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcomarceOnlineShop.Areas.Admin.Controllers.APIs.SetupAndConfigurations
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Authorize]
    public class GeneralSettingsController : ControllerBase
    {
        private readonly IGeneralSettings _service;
        private readonly IImageProcessing _image;

        public GeneralSettingsController(IGeneralSettings service, IImageProcessing imageProcessing)
        {
            _service = service;
            _image = imageProcessing;
        }

        [HttpGet("GetData")]
        public async Task<IActionResult> GetAsync()
        {
            try
            {
                GeneralSettings data = await _service.GetAsync();
                if (data == null) return NotFound("Data not found");
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("Save")]
        [Authorize(Policy = "GeneralSettingsCreatePolicy")]
        public async Task<IActionResult> SaveAsync([FromForm] GeneralSettings model)
        {
            try
            {
                GeneralSettings entity = model;
                if (entity.GeneralSettingsId > 0)
                {
                    entity.Updated_At = DateTime.UtcNow;
                    entity.EntityState = EntityState.Modified;
                    var existingObject = await _service.GetAsync();
                    entity.SystemLogoWhite = existingObject.SystemLogoWhite;
                    entity.SystemLogoBlack = existingObject.SystemLogoBlack;
                    entity.LoginPageBackground = existingObject.LoginPageBackground;
                }
                else
                {
                    entity.SystemTimezone = DateTime.UtcNow;
                    entity.Created_At = DateTime.UtcNow;
                    entity.EntityState = EntityState.Added;
                }
                if (entity.SystemLogoWhiteImage != null)
                {
                    string fileName = DateTime.UtcNow.ToString("dddd_dd_MMMM_yyyy_HH_mm_ss") + entity.SystemLogoWhiteImage.FileName;
                    string path = _image.GetImagePath(fileName, "Image");
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        model.SystemLogoWhiteImage.CopyTo(stream);
                    }
                    entity.SystemLogoWhite = _image.GetImagePathForDb(path);
                }

                if (entity.SystemLogoBlackImage != null)
                {
                    string fileName = DateTime.UtcNow.ToString("dddd_dd_MMMM_yyyy_HH_mm_ss") + entity.SystemLogoBlackImage.FileName;
                    string path = _image.GetImagePath(fileName, "Image");
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        model.SystemLogoBlackImage.CopyTo(stream);
                    }
                    entity.SystemLogoBlack = _image.GetImagePathForDb(path);
                }
                if (entity.LoginPageBackgroundImage != null)
                {
                    string fileName = DateTime.UtcNow.ToString("dddd_dd_MMMM_yyyy_HH_mm_ss") + entity.LoginPageBackgroundImage.FileName;
                    string path = _image.GetImagePath(fileName, "Image");
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        model.LoginPageBackgroundImage.CopyTo(stream);
                    }
                    entity.LoginPageBackground = _image.GetImagePathForDb(path);
                }
                await _service.SaveAsync(entity);
                return Ok(entity);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}