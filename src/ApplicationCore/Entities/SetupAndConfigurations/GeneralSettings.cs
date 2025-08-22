using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Http;
using System;

namespace ApplicationCore.Entities.SetupAndConfigurations
{
    [Table("GeneralSettings")]
    public class GeneralSettings : BaseEntity
    {
        public GeneralSettings()
        {
            SystemTimezone = DateTime.UtcNow;
        }
        [Key]
        public int GeneralSettingsId { get; set; }
        public string SystemName { get; set; }
        public string SystemLogoWhite { get; set; }
        public string SystemLogoBlack { get; set; }
        public DateTime? SystemTimezone { get; set; }
        public string LoginPageBackground { get; set; }
        public string PhoneNumber { get; set; }
        public string SceoundPhoneNumber { get; set; }
        public string TelephonNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string ZipCode { get; set; }
        public string Linkedin { get; set; }
        public string Twitter { get; set; }
        public string Facebook { get; set; }
        public string Youtube { get; set; }
        public string Titel { get; set; }
        public string AboutUs { get; set; }
        [Write(false)]
        public IFormFile SystemLogoWhiteImage { get; set; }
        [Write(false)]
        public IFormFile SystemLogoBlackImage { get; set; }
        [Write(false)]
        public IFormFile LoginPageBackgroundImage { get; set; }

    }
}
