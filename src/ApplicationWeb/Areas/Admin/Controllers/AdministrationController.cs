using ApplicationCore.Entities;
using ApplicationCore.Entities.ApplicationUser;
using ApplicationCore.Entities.Authentication;
using ApplicationCore.Models;
using ApplicationWeb.Data;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Security.Claims;

namespace ApplicationWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdministrationController : Controller
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private UserManager<ApplicationUser> _userManager;
        private SignInManager<ApplicationUser> signInManager;
        private ApplicationDbContext _db;
        private IMenuMasterService _menuService;

        public AdministrationController(IMenuMasterService menuService,
            ApplicationDbContext db,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _db = db;
            this.roleManager = roleManager;
            this._userManager = userManager;
            this.signInManager = signInManager;
            _menuService = menuService;
        }

        [HttpGet]
        public IActionResult CreateRole()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(CreateRoleViewModel Model)
        {
            if (ModelState.IsValid)
            {
                // We just need to specify a unique role name to create a new role
                IdentityRole identityRole = new IdentityRole
                {
                    Name = Model.RoleName
                };

                // Saves the role in the underlying AspNetRoles table
                IdentityResult result = await roleManager.CreateAsync(identityRole);

                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(ListRoles));
                }

                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(Model);
        }

        [HttpGet]
        public IActionResult ListUsers()
        {
            var users = _userManager.Users;
            return View(users);
        }

        [HttpGet]
        public IActionResult ListRoles()
        {
            var roles = roleManager.Roles;
            return View(roles);
        }

        [HttpGet]
        public async Task<IActionResult> EditRole(string id)
        {
            var role = await roleManager.FindByIdAsync(id);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {id} cannot be found";
                return View("NotFound");
            }

            var Model = new EditRoleViewModel
            {
                Id = role.Id,
                RoleName = role.Name
            };

            foreach (var user in _userManager.Users)
            {
                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    Model.Users.Add(user.UserName);
                }
            }

            return View(Model);
        }

        [HttpPost]
        public async Task<IActionResult> EditRole(EditRoleViewModel Model)
        {
            var role = await roleManager.FindByIdAsync(Model.Id);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {Model.Id} cannot be found";
                return View("NotFound");
            }
            else
            {
                role.Name = Model.RoleName;

                var result = await roleManager.UpdateAsync(role);

                if (result.Succeeded)
                {
                    return RedirectToAction("ListRoles");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(Model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditUsersInRole(string roleId)
        {
            ViewBag.roleId = roleId;

            var role = await roleManager.FindByIdAsync(roleId);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {roleId} cannot be found";
                return View("NotFound");
            }

            var Model = new List<UserRoleViewModel>();

            foreach (var user in _userManager.Users)
            {
                var userRoleViewModel = new UserRoleViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName
                };

                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    userRoleViewModel.IsSelected = true;
                }
                else
                {
                    userRoleViewModel.IsSelected = false;
                }

                Model.Add(userRoleViewModel);
            }

            return View(Model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUsersInRole(List<UserRoleViewModel> Model, string roleId)
        {
            var role = await roleManager.FindByIdAsync(roleId);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {roleId} cannot be found";
                return View("NotFound");
            }

            for (int i = 0; i < Model.Count; i++)
            {
                var user = await _userManager.FindByIdAsync(Model[i].UserId);

                IdentityResult result = null;

                if (Model[i].IsSelected && !(await _userManager.IsInRoleAsync(user, role.Name)))
                {
                    result = await _userManager.AddToRoleAsync(user, role.Name);
                }
                else if (!Model[i].IsSelected && await _userManager.IsInRoleAsync(user, role.Name))
                {
                    result = await _userManager.RemoveFromRoleAsync(user, role.Name);
                }
                else
                {
                    continue;
                }

                if (result.Succeeded)
                {
                    if (i < (Model.Count - 1))
                        continue;
                    else
                        return RedirectToAction("EditRole", new { Id = roleId });
                }
            }

            return RedirectToAction("EditRole", new { Id = roleId });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {id} cannot be found";
                return View("NotFound");
            }

            // GetClaimsAsync retunrs the list of user Claims
            var userClaims = await _userManager.GetClaimsAsync(user);
            // GetRolesAsync returns the list of user Roles
            var userRoles = await _userManager.GetRolesAsync(user);

            var Model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Claims = userClaims.Select(c => c.Value).ToList(),
                Roles = userRoles
            };

            return View("EditUser", Model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditUserViewModel Model)
        {
            var user = await _userManager.FindByIdAsync(Model.Id);

            if (user == null)
            {
                ModelState.AddModelError("", "User not found!");
                return View("EditUser", Model);
            }
            else
            {
                user.PhoneNumber = Model.PhoneNumber;
                user.FirstName = Model.FirstName;
                user.LastName = Model.LastName;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    return RedirectToAction("ListUsers");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View("EditUser", Model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound("User Not found!");
            }
            else
            {
                var result = await _userManager.DeleteAsync(user);

                if (!result.Succeeded)
                {
                    return BadRequest("User not deleted!");
                }

                return Ok();
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await roleManager.FindByIdAsync(id);

            if (role == null)
            {
                return NotFound("Role Not found!");
            }
            else
            {
                var result = await roleManager.DeleteAsync(role);

                if (!result.Succeeded)
                {
                    return BadRequest("Role not deleted!");
                }
                return Ok();
            }
        }

        [HttpPost]
        public IActionResult AutogenerateCode()
        {
            var id = Guid.NewGuid().ToString();

            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            string culture = "or-IN";
            Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName, CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)), new CookieOptions()
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1)
            });

            CultureInfo.CurrentCulture = new CultureInfo(culture);

            User user = new User();
            if (Request.Cookies.TryGetValue("DevenseCredentials", out string rememberMeValue))
            {
                var values = rememberMeValue.Split("|||");
                if (values.Length == 2)
                {
                    user.UserName = values[0]; // AESEncryption.Decrypt(values[0]);
                    user.Password = values[1]; // AESEncryption.Decrypt(values[1]);
                    user.Remember = true;

                    ViewBag.RememberMeEmail = values[0];
                    ViewBag.RememberMePassword = values[1];
                    ViewBag.IsRememberMe = true;
                }
            }

            return View("Login", user);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            ViewBag.message = "";
            return View(new RegisterModel());
        }

        [HttpGet]
        public async Task<IActionResult> ManageUserRoles(string userId)
        {
            ViewBag.userId = userId;

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {userId} cannot be found";
                return View("NotFound");
            }

            var model = new List<UserRoleViewModel>();

            foreach (var role in roleManager.Roles)
            {
                var userRolesViewModel = new UserRoleViewModel
                {
                    RoleId = role.Id,
                    RoleName = role.Name
                };

                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    userRolesViewModel.IsSelected = true;
                }
                else
                {
                    userRolesViewModel.IsSelected = false;
                }

                model.Add(userRolesViewModel);
            }

            return View("ManageUserRoles", model);
        }

        [HttpPost]
        public async Task<IActionResult> ManageUserRoles(List<UserRoleViewModel> model, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {userId} cannot be found";
                return View("NotFound");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.RemoveFromRolesAsync(user, roles);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot remove user existing roles");
                return View(model);
            }

            result = await _userManager.AddToRolesAsync(user,
                model.Where(x => x.IsSelected).Select(y => y.RoleName));

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot add selected roles to user");
                return View(model);
            }

            return RedirectToAction("Edit", new { Id = userId });
        }

        [HttpGet]
        public async Task<IActionResult> ManageUserClaims(string userId)
        {
            List<MenuMaster> menusList = await _menuService.GetMenusForClaims(userId);
            ViewBag.menuList = menusList;

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {userId} cannot be found";
                return View("NotFound");
            }

            // UserManager service GetClaimsAsync method gets all the current claims of the user
            var existingUserClaims = await _userManager.GetClaimsAsync(user);
            UserClaimsViewModel model = new UserClaimsViewModel
            {
                UserId = userId
            };
            var dictionary = new Dictionary<string, int>();
            int claimIndex = 0;
            if (menusList != null)
            {
                foreach (var item in menusList)
                {
                    if (!string.IsNullOrWhiteSpace(item.PageName))
                    {
                        foreach (Claim claim in ClaimStore.AllClaims)
                        {
                            var claimName = item.PageName + "." + claim.Type;

                            CustomSelectList userClaim = new CustomSelectList
                            {
                                ClaimType = claimName
                            };

                            if (existingUserClaims.Any(c => c.Type == claimName && c.Value == "true"))
                            {
                                userClaim.Selected = true;
                            }
                            dictionary.Add(claimName, claimIndex);
                            model.Cliams.Add(userClaim);
                            claimIndex++;
                        }
                    }
                }
            }
            ViewBag.claimInfo = dictionary;
            return View("ManageUserClaims", model);
        }

        [HttpPost]
        public async Task<IActionResult> ManageUserClaims(UserClaimsViewModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.UserId))
            {
                return BadRequest("Invalid model data");
            }

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {model.UserId} cannot be found";
                return View("NotFound");
            }

            try
            {
                // Get all existing claims for the user
                var claims = await _userManager.GetClaimsAsync(user);
                // Remove all existing claims
                var result = await _userManager.RemoveClaimsAsync(user, claims);

                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", "Cannot remove user existing claims");
                    // Re-populate ViewBag.menuList if returning to view on error
                    ViewBag.menuList = await _menuService.GetMenusForClaims(model.UserId);
                    return View(model);
                }

                // Add selected claims
                if (model.Cliams != null)
                {
                    // Filter for claims that are selected (true) and create new Claim objects
                    var newClaims = model.Cliams
                                         .Where(c => c.Selected) // Only add claims that were selected
                                         .Select(c => new Claim(c.ClaimType, "true")); // Value should be "true" for selected

                    result = await _userManager.AddClaimsAsync(user, newClaims);

                    if (!result.Succeeded)
                    {
                        ModelState.AddModelError("", "Cannot add selected claims to user");
                        // Re-populate ViewBag.menuList if returning to view on error
                        ViewBag.menuList = await _menuService.GetMenusForClaims(model.UserId);
                        return View(model);
                    }
                }

                // Redirect to the Edit action after successful update
                return RedirectToAction("Edit", new { Id = model.UserId });
            }
            catch (Exception ex)
            {
                // Log the error (e.g., using a logging framework)
                Console.WriteLine($"Error managing user claims: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while saving claims");
                // Re-populate ViewBag.menuList if returning to view on error
                ViewBag.menuList = await _menuService.GetMenusForClaims(model.UserId);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ManageMenuPermission(string userId)
        {
            List<MenuMaster> menusList = await _menuService.GetMenusForPermission(userId);
            MenuMaster model = new MenuMaster
            {
                UserId = userId,
                Menues = menusList.Where(x => x.ParentId > 0).ToList(),
                ParentMenues = menusList.Where(x => x.ParentId == 0).ToList()
            };

            return View("ManageMenuPermission", model);
        }

        [HttpPost]
        public async Task<IActionResult> ManageMenuPermission(MenuMaster model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                ModelState.AddModelError("", "Not a valid user!");
                return RedirectToAction("Edit", new { Id = model.UserId });
            }

            await _menuService.SaveMenuPermissionAsync(model.Menues.Where(x => x.ParentId > 0).ToList(), model.UserId);

            return RedirectToAction("Edit", new { Id = model.UserId });
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Profile()
        {
            ViewBag.PageName = "UserProfile";
            ViewBag.MenuId = "profileMenu";
            return View(new User());
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ChangePassword()
        {
            ViewBag.PageName = "ChangePassword";
            ViewBag.MenuId = "changePasswordMenu";
            return View(new User());
        }

        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([Required] string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                ViewBag.Message = "This is not a valid email!";
                return View();
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var link = Url.Action("ResetPassword", "Administration", new { token, email = user.Email }, Request.Scheme);

            EmailHelper emailHelper = new EmailHelper();
            bool emailResponse = emailHelper.SendEmailPasswordReset(user.Email, link);

            if (emailResponse)
                return RedirectToAction("ForgotPasswordConfirmation");
            else
            {
                // log email failed
                ViewBag.Message = "Unable to send email!";
                return View();
            }
        }

        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult ResetPassword(string token, string email)
        {
            var model = new ResetPassword { Token = token, Email = email };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPassword resetPassword)
        {
            if (!ModelState.IsValid)
                return View(resetPassword);

            if (resetPassword.Password != resetPassword.ConfirmPassword)
            {
                ModelState.AddModelError("", "Password and confirm password should equal!");
                return View(resetPassword);
            }

            var user = await _userManager.FindByEmailAsync(resetPassword.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "No user found by this email!");
                return View(resetPassword);
            }

            var resetPassResult = await _userManager.ResetPasswordAsync(user, resetPassword.Token, resetPassword.Password);
            if (!resetPassResult.Succeeded)
            {
                foreach (var error in resetPassResult.Errors)
                    ModelState.AddModelError(error.Code, error.Description);
                return View();
            }

            return RedirectToAction("ResetPasswordConfirmation");
        }

        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult GoogleLogin()
        {
            string redirectUrl = Url.Action("GoogleResponse", "Administration");
            var properties = signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);
            return new ChallengeResult("Google", properties);
        }

        [AllowAnonymous]
        public async Task<IActionResult> GoogleResponse()
        {
            try
            {
                ExternalLoginInfo info = await signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                    return RedirectToAction(nameof(Login));

                var result = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false);
                string[] userInfo = { info.Principal.FindFirst(ClaimTypes.Name).Value, info.Principal.FindFirst(ClaimTypes.Email).Value };
                if (result.Succeeded)
                {
                    return Ok(new { Message = "Success!" });
                }
                else
                {
                    ApplicationUser user = new ApplicationUser
                    {
                        Email = info.Principal.FindFirst(ClaimTypes.Email).Value,
                        UserName = info.Principal.FindFirst(ClaimTypes.Email).Value
                    };

                    IdentityResult identResult = await _userManager.CreateAsync(user);
                    if (identResult.Succeeded)
                    {
                        identResult = await _userManager.AddLoginAsync(user, info);
                        if (identResult.Succeeded)
                        {
                            await signInManager.SignInAsync(user, false);
                            return Ok(new { Message = "Success!" });
                        }
                    }
                    return BadRequest(new { Message = "Error!" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [AllowAnonymous]
        public IActionResult StripePayment()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult PaypalPayment()
        {
            return View();
        }
    }
}