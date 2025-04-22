//using Microsoft.AspNetCore.Mvc;
//using ServerApps.Business.Usescasess.Configuration;
//using ServerApps.Business.Dtos;

//namespace ServerApps.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class ConfigurationController : ControllerBase
//    {
//        private readonly IConfigurationService _configService;

//        public ConfigurationController(IConfigurationService configService)
//        {
//            _configService = configService;
//        }

//        [HttpGet]
//        public ActionResult<List<GetConfigurationDto>> GetConfiguration()
//        {
//            try
//            {
//                var configDtos = _configService.GetAllServerConfiguration();

//                if (configDtos == null || configDtos.Count == 0)
//                    return NotFound("No configurations found.");

//                return Ok(configDtos);
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, $"Internal server error: {ex.Message}");
//            }
//        }
//    }
//}
