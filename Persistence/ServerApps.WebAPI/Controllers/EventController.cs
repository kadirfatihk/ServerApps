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
        public ActionResult<List<EventLogEntryDto>> GetEventLogs([FromQuery] string serverIp, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] TimeSpan? startTime, [FromQuery] TimeSpan? endTime)
        {
            try
            {
                if (string.IsNullOrEmpty(serverIp))
                {
                    return BadRequest("Server IP is required.");
                }

                // Default olarak bugünün tarihi verilecek
                if (!startDate.HasValue)
                    startDate = DateTime.Today;

                if (!endDate.HasValue)
                    endDate = DateTime.Today;

                var logs = _eventService.GetEventLogs(serverIp, startDate, endDate, startTime, endTime);

                return Ok(logs); // Event logs döndürülür
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}
