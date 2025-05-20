using Microsoft.AspNetCore.Mvc;
using System.Globalization;

public class HomeController : Controller
{
    public IActionResult Index(string culture = "en")
    {
        return View();
    }
}
