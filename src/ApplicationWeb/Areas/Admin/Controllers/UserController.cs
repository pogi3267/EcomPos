using ApplicationCore.Entities.ApplicationUser;
using ApplicationWeb.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserController : Controller
    {
        UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        ApplicationDbContext _db;
        public UserController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _db = db;
        }
        public async Task<ActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Register(ApplicationCore.Entities.ApplicationUser.User model)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(model.UserName))
                    ModelState.AddModelError(string.Empty, "Enter username!");

                if (string.IsNullOrEmpty(model.FirstName))
                    ModelState.AddModelError(string.Empty, "Enter firstname!");

                if (string.IsNullOrEmpty(model.Email))
                    ModelState.AddModelError(string.Empty, "Enter email!");

                if (string.IsNullOrEmpty(model.Password))
                    ModelState.AddModelError(string.Empty, "Enter password!");

                if (model.Password != model.ConfirmPassword)
                    ModelState.AddModelError(string.Empty, "Password and confirm passward aren't match!");

                if (model.UserName != null)
                {
                    var userExists = await _userManager.FindByNameAsync(model.UserName);
                    if (userExists != null)
                        ModelState.AddModelError(string.Empty, "this username already exist!");
                }

                if (model.Email != null)
                {
                    var userExists = await _userManager.FindByEmailAsync(model.Email);
                    if (userExists != null)
                        ModelState.AddModelError(string.Empty, "this username already exist!");
                }

                if (ModelState.ErrorCount > 0)
                {
                    return View("/Areas/Admin/Views/User/Create.cshtml");
                }

                ApplicationUser user = new()
                {
                    UserName = model.UserName,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    TwoFactorEnabled = false,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    Role = "User",
                    CreatedDate = DateTime.UtcNow,
                    Discriminator = "ApplicationUser"
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    return RedirectToAction("ListUsers", "Administration");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

            }
            return View("/Areas/Admin/Views/User/Create.cshtml", model);
        }


    }
}
