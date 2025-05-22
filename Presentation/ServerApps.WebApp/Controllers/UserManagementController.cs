using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerApps.Business.Usescasess.Auth;
using ServerApps.Persistence.Models;
using System.Linq;

namespace ServerApps.WebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        private readonly IUserService _userService;

        public UserManagementController(IUserService userService)
        {
            _userService = userService;
        }

        public IActionResult UserList()
        {
            var users = _userService.GetAllUsers().ToList();
            return View(users);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            _userService.DeleteUser(id);
            return RedirectToAction("UserList");
        }

        [HttpGet]
        public IActionResult AddUser()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddUser(string firstName,string lastName,string jobTitle ,string emailAddress, string password, bool isAdmin)
        {
            if (string.IsNullOrEmpty(emailAddress) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "E-posta ve şifre boş olamaz.");
                return View();
            }

            var user = new User
            {
                FirstName = firstName,
                LastName = lastName,
                JobTitle = jobTitle,
                EmailAddress = emailAddress,
                PasswordHash = password,
                IsAdmin = isAdmin
            };

            _userService.AddUser(user);

            return RedirectToAction("UserList");
        }

        [HttpPost]
        public IActionResult UpdateAdmin(int id, bool isAdmin)
        {
            _userService.UpdateUserAdminStatus(id, isAdmin);
            return RedirectToAction("UserList");
        }
    }
}
