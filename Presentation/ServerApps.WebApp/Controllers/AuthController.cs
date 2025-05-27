using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using ServerApps.Business.Usescasess.Auth;
using System.Security.Claims;
using ServerApps.Business.Dtos.AuthDtos;

namespace ServerApps.WebApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly IUserService _userService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IUserService userService, ILogger<AuthController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = _userService.ValidateUser(email, password);
            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.EmailAddress)
                };

                if (user.IsAdmin == true)
                {
                    claims.Add(new Claim(ClaimTypes.Role, "Admin"));
                }

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = false
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "“The email or password is incorrect.";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Auth");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ViewBag.Message = "Please enter a valid e-mail address.";
                return View();
            }

            var result = _userService.SendResetPasswordEmail(email, out string errorMessage);

            if (result)
            {
                ViewBag.Message = "The password reset link has been sent to your e-mail address.";
            }
            else
            {
                ViewBag.Message = "Invalid email address or sending error occurred." + errorMessage;
            }

            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                return BadRequest("Missing parameters.");
            }

            var model = new ResetPasswordViewModel
            {
                Email = email,
                Token = token
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model); // validation mesajlarıyla geri döner
            }

            bool result = _userService.ResetPassword(model.Email, model.Token, model.NewPassword, out string errorMessage);

            if (!result)
            {
                // Eğer errorMessage null değilse onu göster
                ModelState.AddModelError("", errorMessage ?? "Invalid email or token. Please check the link in the mail.");
                return View(model);
            }

            TempData["SuccessMessage"] = "Your password has been successfully updated. You can log in now.";
            return RedirectToAction("Login", "Auth");
        }

    }
}
