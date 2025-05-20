using Microsoft.AspNetCore.Mvc;
using ServerApps.Business.Dtos.EventDtos;
using ServerApps.Business.Usescasess;
using ServerApps.Business.Usescasess.Event;
using System;

namespace ServerApps.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventController : ControllerBase
    {
        private readonly IEventService _eventService;

        public EventController(IEventService eventService)
        {
            _eventService = eventService;
        }

        [HttpGet("GetEventLogs")]
        public ActionResult<List<EventLogEntryDto>> GetEventLogs(
    [FromQuery] string serverIp,
    [FromQuery] DateTime? startDateTime,
    [FromQuery] DateTime? endDateTime)
        {
            try
            {
                if (string.IsNullOrEmpty(serverIp))
                    return BadRequest("Server IP is required.");

                // Varsayılan olarak bugünün tarihini kullan
                if (!startDateTime.HasValue)
                    startDateTime = DateTime.Today;

                if (!endDateTime.HasValue)
                    endDateTime = DateTime.Today.AddDays(1).AddSeconds(-1); // Bugünün sonu

                var logs = _eventService.GetEventLogs(serverIp, startDateTime, endDateTime);
                return Ok(logs);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

    }
}
