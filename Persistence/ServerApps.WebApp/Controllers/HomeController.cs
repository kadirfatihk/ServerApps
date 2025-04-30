using Microsoft.AspNetCore.Mvc;

public class HomeController : Controller
{
    // Ana sayfa, sadece iki buton olacak
    public IActionResult Index()
    {
        return View();
    }
}
