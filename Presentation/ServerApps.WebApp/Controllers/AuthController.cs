using Microsoft.AspNetCore.Authentication.Cookies; // Cookie tabanlı kimlik doğrulama için gerekli
using Microsoft.AspNetCore.Authentication; // Authentication işlemleri için gerekli
using Microsoft.AspNetCore.Mvc; // MVC Controller sınıfı için gerekli
using ServerApps.Business.Usescasess.Auth; // Kullanıcı servisinin tanımı
using System.Security.Claims; // Kullanıcı claim bilgileri için gerekli
using ServerApps.Business.Dtos.AuthDtos; // ResetPasswordViewModel gibi DTO sınıflar için

namespace ServerApps.WebApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly IUserService _userService; // Kullanıcı işlemlerini yöneten servis
        private readonly ILogger<AuthController> _logger; // Loglama servisi

        public AuthController(IUserService userService, ILogger<AuthController> logger)
        {
            _userService = userService; // Servis enjekte ediliyor
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login() // Login sayfasını getirir
        {
            return View(); // Giriş ekranını döndürür
        }

        [HttpPost]
        public IActionResult Login(string email, string password) // Kullanıcı giriş post isteği
        {
            var user = _userService.ValidateUser(email, password); // Kullanıcının e-posta ve şifresini kontrol eder
            if (user != null)
            {
                var claims = new List<Claim> // Kullanıcıya ait claim bilgileri oluşturulur
                {
                    new Claim(ClaimTypes.Name, user.EmailAddress) // Kullanıcının adı olarak e-posta atanır
                };

                if (user.IsAdmin == true) // Eğer kullanıcı admin ise
                {
                    claims.Add(new Claim(ClaimTypes.Role, "Admin")); // Role claim'i eklenir
                }

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme); // ClaimsIdentity oluşturulur

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = false // Oturum tarayıcı kapanınca sonlanır
                };

                // Kullanıcıyı cookie ile oturum açtırır
                HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                return RedirectToAction("Index", "Home"); // Giriş başarılıysa anasayfaya yönlendirilir
            }

            ViewBag.Error = "“The email or password is incorrect."; // Giriş başarısızsa hata mesajı döner
            return View();
        }

        [HttpPost]
        public  IActionResult Logout() // Kullanıcı çıkış işlemi
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); // Cookie'yi temizler
            return RedirectToAction("Login", "Auth"); // Giriş sayfasına yönlendirir
        }

        [HttpGet]
        public IActionResult ForgotPassword() // Şifremi unuttum sayfasını getirir
        {
            return View();
        }

        [HttpPost]
        public IActionResult ForgotPassword(string email) // Şifremi unuttum formu gönderildiğinde
        {
            if (string.IsNullOrWhiteSpace(email)) // E-posta boşsa hata mesajı göster
            {
                ViewBag.Message = "Please enter a valid e-mail address.";
                return View();
            }

            var result = _userService.SendResetPasswordEmail(email, out string errorMessage); // Şifre sıfırlama bağlantısı gönderilir

            if (result)
            {
                ViewBag.Message = "The password reset link has been sent to your e-mail address."; // Başarılıysa bilgi mesajı
            }
            else
            {
                ViewBag.Message = "Invalid email address or sending error occurred." + errorMessage; // Hata varsa mesaj döndürülür
            }

            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string email, string token) // Maildeki link ile gelen şifre sıfırlama sayfası
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token)) // Parametreler eksikse hata döner
            {
                return BadRequest("Missing parameters.");
            }

            var model = new ResetPasswordViewModel
            {
                Email = email, // ViewModel'e e-posta atanır
                Token = token  // Token atanır
            };

            return View(model); // Model ile birlikte şifre sıfırlama sayfası açılır
        }

        [HttpPost]
        public IActionResult ResetPassword(ResetPasswordViewModel model) // Şifre sıfırlama formu post isteği
        {
            if (!ModelState.IsValid) // Model doğrulama hataları varsa
            {
                return View(model); // Hatalarla birlikte geri döner
            }

            bool result = _userService.ResetPassword(model.Email, model.Token, model.NewPassword, out string errorMessage); // Şifre sıfırlama işlemi yapılır

            if (!result)
            {
                // Eğer errorMessage null değilse onu göster
                ModelState.AddModelError("", errorMessage ?? "Invalid email or token. Please check the link in the mail.");
                return View(model); // Hatalarla birlikte geri döner
            }

            TempData["SuccessMessage"] = "Your password has been successfully updated. You can log in now."; // Başarı mesajı
            return RedirectToAction("Login", "Auth"); // Giriş sayfasına yönlendirir
        }

        
    }
}
