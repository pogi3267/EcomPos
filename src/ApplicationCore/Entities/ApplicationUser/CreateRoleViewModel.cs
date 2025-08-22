using System.ComponentModel.DataAnnotations;

namespace ApplicationCore.Entities.ApplicationUser
{
    public class CreateRoleViewModel
    {
        [Required]
        [Display(Name = "Role")]
        public string RoleName { get; set; }
    }
}
