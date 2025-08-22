using ApplicationCore.DTOs;
using ApplicationCore.Entities.ApplicationUser;
using ApplicationCore.Entities.Authentication;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using ApplicationWeb.HelperAndConstant;
using ApplicationWeb.Security;
using ApplicationWeb.Utility;
using Infrastructure.Interfaces.SetupAndConfigurations;
using Infrastructure.Interfaces.UI;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ApplicationWeb.Areas.Admin.Controllers.APIs.Authentication
{
    [Area("Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserService _userService;
        private readonly IImageProcessing _image;
        private readonly IBusinessSettingService _businessService;
        private readonly IOrderService _orderService;

        public AuthenticateController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            IUserService userService,
            IImageProcessing imageProcessing,
            IBusinessSettingService businessService,
            IOrderService orderService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _userService = userService;
            _image = imageProcessing;
            _businessService = businessService;
            _orderService = orderService;
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromForm] ApplicationCore.Entities.ApplicationUser.User model)
        {
            if (model.Remember && model.IsDecrypt)
            {
                model.UserName = AESEncryption.Decrypt(model.UserName);
                model.Password = AESEncryption.Decrypt(model.Password);
            }
            ApplicationUser user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                return Unauthorized("Invalid username");
            }
            if (model.IsClaimNeeded == true)
            {
                bool res = await _userManager.IsInRoleAsync(user, "SuperUser");
                if (!res)
                {
                    return Unauthorized("You are not authorized to log in admin panel!");
                }
            }


            if (user != null)
            {
                if (user.Banned == 1)
                {
                    return BadRequest("Inactive user!");
                }
                await _signInManager.SignOutAsync();
                var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
                //var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.Remember, false);
                if (result.Succeeded)
                {
                    this.HttpContext.Session.SetString("UserId", user.Id);
                    this.HttpContext.Session.Set("User", user);
                    if (model.Remember) HttpContext.Session.Set("SessionTimeout", BitConverter.GetBytes(TimeSpan.FromDays(1).TotalMinutes));
                    else HttpContext.Session.Set("SessionTimeout", BitConverter.GetBytes(TimeSpan.FromHours(1).TotalMinutes));

                    if (model.Remember)
                    {
                        Response.Cookies.Append("DevenseCredentials", $"{AESEncryption.Encrypt(model.UserName)}|||{AESEncryption.Encrypt(model.Password)}", new CookieOptions
                        {
                            Expires = DateTime.UtcNow.AddDays(30),
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.None
                        });
                    }

                    var claims = await GetValidClaims(user);
                    var token = GetToken(claims, model.Remember);
                    var existingUserClaims = await _userManager.GetClaimsAsync(user);

                    return Ok(new
                    {
                        token = new JwtSecurityTokenHandler().WriteToken(token),
                        expiration = token.ValidTo,
                        //userClaims = existingUserClaims.ToList(),
                        userClaims = model.IsClaimNeeded == true ? existingUserClaims.ToList() : new List<Claim>(),
                        userInfo = new
                        {
                            Name = string.IsNullOrEmpty(user.FirstName) ? user.UserName : (user.FirstName + " " + user.LastName),
                            Photo = "",
                            UserType = user.Discriminator,
                            Email = user.Email,
                            PhoneNumber = user.PhoneNumber
                        }
                    });
                }
                else
                {
                    return Unauthorized("Invalid password");
                }
            }
            return Unauthorized("Invalid username");
        }

        private async Task<List<Claim>> GetValidClaims(ApplicationUser user)
        {
            try
            {
                IdentityOptions _options = new IdentityOptions();
                var claims = new List<Claim>
                {
                    new Claim("Id", user.Id),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(_options.ClaimsIdentity.UserIdClaimType, user.Id.ToString()),
                    new Claim(_options.ClaimsIdentity.UserNameClaimType, user.UserName),
                };

                var userClaims = await _userManager.GetClaimsAsync(user);
                var userRoles = await _userManager.GetRolesAsync(user);

                claims.AddRange(userClaims);
                foreach (var userRole in userRoles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, userRole));
                    var role = await _roleManager.FindByNameAsync(userRole);
                    if (role != null)
                    {
                        var roleClaims = await _roleManager.GetClaimsAsync(role);
                        foreach (Claim roleClaim in roleClaims)
                        {
                            claims.Add(roleClaim);
                        }
                    }
                }
                return claims;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        [Route("register-admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterModel model)
        {
            var userExists = await _userManager.FindByNameAsync(model.Username);
            if (userExists != null)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User already exists!" });

            ApplicationUser user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User creation failed! Please check user details and try again." });

            if (!await _roleManager.RoleExistsAsync(UserRoles.Admin))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
            if (!await _roleManager.RoleExistsAsync(UserRoles.User))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.User));

            if (await _roleManager.RoleExistsAsync(UserRoles.Admin))
            {
                await _userManager.AddToRoleAsync(user, UserRoles.Admin);
            }
            if (await _roleManager.RoleExistsAsync(UserRoles.Admin))
            {
                await _userManager.AddToRoleAsync(user, UserRoles.User);
            }
            return Ok(new Response { Status = "Success", Message = "User created successfully!" });
        }

        private JwtSecurityToken GetToken(List<Claim> authClaims, bool remember)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: remember ? DateTime.UtcNow.AddDays(1) : DateTime.UtcNow.AddHours(1),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }

        [HttpPost]
        [Route("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                string? userId = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized();
                HttpContext.Session.Clear();
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                await _signInManager.SignOutAsync();
                Response.Cookies.Delete("DevenseCredentials");

                return Ok("Logout successful");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("get/{id}")]
        [Authorize]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound("User not found!");

            return Ok(user);
        }

        [HttpPost]
        [Route("update-profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromForm] ApplicationCore.Entities.ApplicationUser.User model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
                return NotFound("User not found!");

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.UserName = model.UserName;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest("Failed! Please check user details and try again!");
            return Ok();
        }

        [HttpPost]
        [Route("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromForm] ApplicationCore.Entities.ApplicationUser.User model)
        {
            if (model.NewPassword != model.ConfirmPassword)
                return BadRequest("New password and confirm passward aren't match!");

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
                return NotFound("User not found!");

            bool isValidPass = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!isValidPass)
                return BadRequest("Your current password is not match!");

            var result = await _userManager.ChangePasswordAsync(user, model.Password, model.ConfirmPassword);
            if (!result.Succeeded)
                return BadRequest("Failed! Please check user details and try again!");

            return Ok();
        }

        [HttpPost]
        [Route("signup")]
        public async Task<IActionResult> SignUp([FromForm] ApplicationCore.Entities.ApplicationUser.User model)
        {
            if (model.Password != model.ConfirmPassword)
                return BadRequest("Password and confirm passward aren't match!");

            var userExists = await _userManager.FindByNameAsync(model.UserName);
            if (userExists != null && userExists.Discriminator == "TempUser")
            {
                userExists.FirstName = model.Name;
                userExists.Email = model.Email;
                userExists.PhoneNumber = model.PhoneNumber;
                userExists.TwoFactorEnabled = false;
                userExists.SecurityStamp = Guid.NewGuid().ToString();
                userExists.Role = "User";
                userExists.CreatedDate = DateTime.UtcNow;
                userExists.Discriminator = "ApplicationUser";

                var result = await _userManager.UpdateAsync(userExists);
                if (!result.Succeeded)
                    return BadRequest("User creation failed! Please check user details and try again.");
            }
            else
            {
                if (userExists != null)
                    return BadRequest("This email already exist!");

                ApplicationUser user = new()
                {
                    UserName = model.UserName,
                    FirstName = model.Name,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    TwoFactorEnabled = false,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    Role = "User",
                    CreatedDate = DateTime.UtcNow,
                    Discriminator = "ApplicationUser"
                };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                    return BadRequest("User creation failed! Please check user details and try again.");
            }

            return Ok("You have successfully signed up.");
        }

        [HttpPost]
        [Route("reset-password")]
        [Authorize]
        public async Task<IActionResult> ResetPassword([FromForm] ApplicationCore.Entities.ApplicationUser.User model)
        {
            try
            {
                if (model.NewPassword != model.ConfirmPassword)
                    return BadRequest(new { error = "New password and confirm passward aren't match!" });

                string? userId = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized();

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return NotFound(new { error = "User not found!" });

                if (user.Discriminator == "GoogleUser" || user.Discriminator == "FacebookUser")
                {
                    var removePasswordResult = await _userManager.RemovePasswordAsync(user);
                    if (removePasswordResult.Succeeded)
                    {
                        var addPasswordResult = await _userManager.AddPasswordAsync(user, model.ConfirmPassword);
                        if (addPasswordResult.Succeeded)
                        {
                            user.Discriminator = "ApplicationUser";
                            await _userManager.UpdateAsync(user);
                        }
                        else
                        {
                            return BadRequest(new { error = "Failed! Please check user details and try again!" });
                        }
                    }
                    else
                    {
                        return BadRequest(new { error = "Failed! Please check user details and try again!" });
                    }
                }
                else
                {
                    bool isValidPass = await _userManager.CheckPasswordAsync(user, model.Password);
                    if (!isValidPass)
                        return BadRequest(new { error = "Your current password is not match!" });

                    var result = await _userManager.ChangePasswordAsync(user, model.Password, model.ConfirmPassword);
                    if (!result.Succeeded)
                        return BadRequest(new { error = "Failed! Please check user details and try again!" });
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        [Route("get-user-info")]
        [Authorize]
        public async Task<IActionResult> GetUserInfoAsync()
        {
            try
            {
                string? userId = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized();

                var user = await _userService.GetUserInfoAsync(userId);

                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("save-profile")]
        [Authorize]
        public async Task<IActionResult> SaveProfileAsync([FromForm] ApplicationCore.Entities.ApplicationUser.User model)
        {
            try
            {
                string? userId = User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized();
                model.Id = userId;

                string fileName = "";
                if (model.AvatarImage != null)
                {
                    decimal fileSize = model.AvatarImage.Length / 1024; //convert to kb
                    if (fileSize > 501)
                    {
                        return BadRequest(new { error = "File size could not be greater than 500 KB!" });
                    }

                    fileName = DateTime.UtcNow.ToString("dddd_dd_MMMM_yyyy_HH_mm_ss") + model.AvatarImage.FileName;
                    string path = _image.GetImagePath(fileName, "Image");
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        model.AvatarImage.CopyTo(stream);
                    }
                    model.Avatar = _image.GetImagePathForDb(path);
                }

                await _userService.SaveProfileAsync(model);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost]
        [Route("forget-password")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return BadRequest("No user found with this email!");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var link = Url.Action("ResetPassword", "Administration", new { token, email = user.Email }, Request.Scheme);

            EmailHelper emailHelper = new EmailHelper();
            bool emailResponse = emailHelper.SendEmailPasswordReset(user.Email, link);

            if (emailResponse)
            {
                return Ok("Email send successfully!");
            }
            else
            {
                return BadRequest("Unable to send email!");
            }
        }

        [HttpPost]
        [Route("social-login")]
        public async Task<IActionResult> SocialLogin([FromForm] SocialInfo info)
        {
            try
            {
                ApplicationUser user = await _userManager.FindByEmailAsync(info.Email);
                if (user != null)
                {
                    #region Exist User

                    if (user.Banned == 1)
                    {
                        return Unauthorized("Inactive user!");
                    }

                    this.HttpContext.Session.SetString("UserId", user.Id);
                    this.HttpContext.Session.Set("User", user);

                    var claims = await GetValidClaims(user);
                    var token = GetToken(claims, false);
                    var existingUserClaims = await _userManager.GetClaimsAsync(user);

                    return Ok(new
                    {
                        token = new JwtSecurityTokenHandler().WriteToken(token),
                        expiration = token.ValidTo,
                        userClaims = existingUserClaims.ToList(),
                        userInfo = new
                        {
                            Name = string.IsNullOrEmpty(user.FirstName) ? user.UserName : (user.FirstName + " " + user.LastName),
                            Photo = "",
                            UserType = user.Discriminator,
                            Email = user.Email,
                            PhoneNumber = user.PhoneNumber
                        }
                    });

                    #endregion Exist User
                }
                else
                {
                    user = new()
                    {
                        UserName = info.Email,
                        FirstName = info.Name,
                        Email = info.Email,
                        TwoFactorEnabled = false,
                        SecurityStamp = Guid.NewGuid().ToString(),
                        Role = "User",
                        CreatedDate = DateTime.UtcNow,
                        Discriminator = (info.SocialType.ToLower() == "google") ? "GoogleUser" : "FacebookUser"
                    };
                    var result = await _userManager.CreateAsync(user);
                    if (result.Succeeded)
                    {
                        user = await _userManager.FindByNameAsync(info.Email);

                        #region New User

                        this.HttpContext.Session.SetString("UserId", user.Id);
                        this.HttpContext.Session.Set("User", user);

                        var claims = await GetValidClaims(user);
                        var token = GetToken(claims, false);
                        var existingUserClaims = await _userManager.GetClaimsAsync(user);

                        return Ok(new
                        {
                            token = new JwtSecurityTokenHandler().WriteToken(token),
                            expiration = token.ValidTo,
                            userClaims = existingUserClaims.ToList(),
                            userInfo = new
                            {
                                Name = string.IsNullOrEmpty(user.FirstName) ? user.UserName : (user.FirstName + " " + user.LastName),
                                Photo = "",
                                UserType = user.Discriminator,
                                Email = user.Email,
                                PhoneNumber = user.PhoneNumber
                            }
                        });

                        #endregion New User
                    }
                    else
                    {
                        return Unauthorized("User creation failed! Please check user details and try again.");
                    }
                }
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPost]
        [Route("demo-login")]
        public async Task<IActionResult> DemoLogin()
        {
            ApplicationUser user = new()
            {
                UserName = $"DemoUser_{Guid.NewGuid()}",
                FirstName = "Guest",
                LastName = "User",
                Email = "",
                PhoneNumber = "",
                TwoFactorEnabled = false,
                SecurityStamp = Guid.NewGuid().ToString(),
                Role = "Guest User",
                CreatedDate = DateTime.UtcNow,
                Discriminator = "ApplicationUser"
            };

            var result = await _userManager.CreateAsync(user, "123");
            if (!result.Succeeded)
            {
                return BadRequest("You can not progress with guest user. Please sign in.");
            }

            user = await _userManager.FindByNameAsync(user.UserName);
            if (user != null)
            {
                await _signInManager.SignOutAsync();
                var res = await _signInManager.CheckPasswordSignInAsync(user, "123", false);
                if (res.Succeeded)
                {
                    HttpContext.Session.SetString("UserId", user.Id);
                    HttpContext.Session.Set("User", user);
                    HttpContext.Session.Set("SessionTimeout", BitConverter.GetBytes(TimeSpan.FromHours(1).TotalMinutes));

                    var claims = await GetValidClaims(user);
                    var token = GetToken(claims, false);
                    //var existingUserClaims = await _userManager.GetClaimsAsync(user);

                    return Ok(new
                    {
                        token = new JwtSecurityTokenHandler().WriteToken(token),
                        expiration = token.ValidTo,
                        userClaims = new List<Claim>(),
                        userInfo = new
                        {
                            Name = string.IsNullOrEmpty(user.FirstName) ? user.UserName : (user.FirstName + " " + user.LastName),
                            Photo = "",
                            UserType = user.Discriminator,
                            Email = user.Email,
                            PhoneNumber = user.PhoneNumber
                        }
                    });
                }
                else
                {
                    return Unauthorized("Invalid password");
                }
            }
            return Unauthorized("Invalid username");
        }

        [HttpPost("create-order-v2")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateOrderV2Async([FromBody] OrderRequest request)
        {
            try
            {
                //Register And Login
                ApplicationUser user = await _userManager.FindByNameAsync(request.Address.Phone);
                if (user == null)
                {
                    user = new()
                    {
                        UserName = request.Address.Phone,
                        FirstName = request.Address.ReceiverName,
                        Email = request.Address.Email,
                        PhoneNumber = request.Address.Phone,
                        TwoFactorEnabled = false,
                        SecurityStamp = Guid.NewGuid().ToString(),
                        Role = "User",
                        CreatedDate = DateTime.UtcNow,
                        Discriminator = "TempUser"
                    };
                    await _userManager.CreateAsync(user, request.Address.Phone);
                    user = await _userManager.FindByNameAsync(request.Address.Phone);
                }
                //Register And Login

                string addressJson = JsonConvert.SerializeObject(request.Address);
                var result = (IDictionary<string, object>)await _orderService.CreateOrderV2Async(user.Id, request.ProductStocks, addressJson, request.CouponId, request.PaymentType, request.PickupId, request.ShippingLocation);

                dynamic order = await _orderService.GetOrderAsync(Convert.ToInt32(result["OrderId"]));

                return Ok(new
                {
                    orderInfo = order
                });
            }
            catch (Exception ex)
            {
                string firstLineOfError = ex.Message.Split(new[] { Environment.NewLine }, StringSplitOptions.None)[0];

                return BadRequest(firstLineOfError);
            }
        }

    }
}