using Microsoft.AspNetCore.Mvc;
using ServerApps.Business.Dtos.EventDtos;
using ServerApps.Business.Usescasess.Configuration;
using ServerApps.Business.Usescasess.Event;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerApps.WebApp.Controllers
{
    public class EventController : Controller
    {
        private readonly IConfigurationService _configurationService;
        private readonly IEventService _eventService;

        public EventController(IConfigurationService configurationService, IEventService eventService)
        {
            _configurationService = configurationService;
            _eventService = eventService;
        }

        public IActionResult GetEvents(int page = 1, int pageSize = 20)
        {
            var configurations = _configurationService.GetConfigurations();
            if (configurations == null || !configurations.Any())
            {
                ViewBag.ErrorMessage = "Sunucu listesi alınamadı.";
                return View(new List<ServerApps.Business.Dtos.ConfigurationDtos.GetConfigurationDto>());
            }

            var totalCount = configurations.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var pagedConfigurations = configurations
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(pagedConfigurations);
        }

        public IActionResult GetListEvent(
            string ip,
            DateTime? startDateTime,
            DateTime? endDateTime,
            string[] levels = null,
            int page = 1,
            int pageSize = 20)
        {
            var allEvents = new List<EventLogEntryDto>();

            if (string.IsNullOrWhiteSpace(ip))
            {
                ViewBag.ErrorMessage = "IP adresi boş olamaz.";
                return View(allEvents);
            }

            try
            {
                allEvents = _eventService.GetEventLogs(ip, startDateTime, endDateTime);

                if (levels != null && levels.Any())
                {
                    allEvents = allEvents
                        .Where(e => levels.Contains(e.Level, StringComparer.OrdinalIgnoreCase))
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
            }

            var totalCount = allEvents.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            var pagedEvents = allEvents
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.IpAddress = ip;
            ViewBag.StartDateTime = startDateTime?.ToString("yyyy-MM-ddTHH:mm") ?? "";
            ViewBag.EndDateTime = endDateTime?.ToString("yyyy-MM-ddTHH:mm") ?? "";
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SelectedLevels = levels ?? new string[0];

            return View(pagedEvents);
        }

    }
}
