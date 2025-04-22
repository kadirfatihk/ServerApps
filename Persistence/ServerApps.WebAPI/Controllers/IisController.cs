using Microsoft.AspNetCore.Mvc;
using ServerApps.Business.Usescasess.IIS;
//using ServerApps.Core.IIS;

namespace ServerApps.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IisController : ControllerBase
    {
        private readonly IIisService _iisService;

        public IisController(IIisService iisService)
        {
            _iisService = iisService;
        }

        [HttpGet("applications")]
        public IActionResult GetApplications()
        {
            try
            {
                var apps = _iisService.GetAllApplications();
                return Ok(apps);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
