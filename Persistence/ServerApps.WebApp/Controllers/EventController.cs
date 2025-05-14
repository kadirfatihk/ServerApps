using Microsoft.AspNetCore.Mvc;

namespace ServerApps.WebApp.Controllers
{
    public class EventController : Controller
    {
        public IActionResult GetEvents()
        {
            return View();
        }

       
    }
}
