using ApplicationCore.Entities.ApplicationUser;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ApplicationWeb.Areas.Admin.Controllers.APIs
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManageUserClaimController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public ManageUserClaimController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        [HttpGet("GetUserClaim")]
        public async Task<IActionResult> GetUserClaim()
        {
            string userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var user = await userManager.FindByIdAsync(userId);

            if (user == null) return NotFound();

            // UserManager service GetClaimsAsync method gets all the current claims of the user
            var existingUserClaims = await userManager.GetClaimsAsync(user);

            var model = new UserClaim
            {
                UserId = userId
            };

            foreach (Claim claim in ClaimStore.AllClaims)
            {
                CustomSelectList userClaim = new CustomSelectList
                {
                    ClaimType = claim.Type
                };
                if (existingUserClaims.Any(c => c.Type == claim.Type))
                {
                    userClaim.Selected = true;
                }

                model.ApplicationClaims.Add(userClaim);
            }

            return Ok(model);
        }
    }
}