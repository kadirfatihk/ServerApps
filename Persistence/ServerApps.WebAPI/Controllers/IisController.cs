using Microsoft.AspNetCore.Mvc;
using ServerApps.Business.Dtos.IisDtos;
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

        [HttpPost("start-website")]
        public IActionResult StartWebsite([FromBody] StartStopWebSiteDto dto)
        {
            try
            {
                _iisService.StartWebSite(dto.Ip, dto.SiteName);
                return Ok(new { message = $"Site '{dto.SiteName}' başlatıldı." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("stop-website")]
        public IActionResult StopWebsite([FromBody] StartStopWebSiteDto dto)
        {
            try
            {
                _iisService.StopWebSite(dto.Ip, dto.SiteName);
                return Ok(new { message = $"Site '{dto.SiteName}' durduruldu." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("update-port")]
        public IActionResult UpdatePort([FromBody] UpdatePortDto dto)
        {
            try
            {
                _iisService.UpdateWebSitePort(dto.Ip, dto.SiteName, dto.NewPort);
                return Ok(new { message = "Port başarıyla güncellendi." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

    }
}
