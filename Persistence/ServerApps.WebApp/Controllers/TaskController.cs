using Microsoft.AspNetCore.Mvc;
using ServerApps.Business.Dtos.TaskDtos;
using ServerApps.Business.Usescasess.Task;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerApps.WebApp.Controllers
{
    public class TaskController : Controller
    {
        private readonly ITaskService _taskService;

        public TaskController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        public IActionResult GetTasks(string searchQuery, int page = 1)
        {
            int pageSize = 20;

            List<TaskInfoApplicationDto> allTasks = _taskService.GetAllTasks();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                allTasks = allTasks.Where(task =>
                    (task.ApplicationName != null && task.ApplicationName.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)) ||
                    (task.Ip != null && task.Ip.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)) ||
                    (task.TaskName != null && task.TaskName.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)) ||
                    (task.Status != null && task.Status.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)) ||
                    (task.Trigger != null && task.Trigger.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)) ||
                    (task.LastTaskResult != null && task.LastTaskResult.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)) ||
                    (task.LastRunTime.HasValue && task.LastRunTime.Value.ToString("dd.MM.yyyy HH:mm").Contains(searchQuery)) ||
                    (task.NextRunTime.HasValue && task.NextRunTime.Value.ToString("dd.MM.yyyy HH:mm").Contains(searchQuery))
                ).ToList();
            }

            int totalTasks = allTasks.Count;
            int totalPages = (int)Math.Ceiling(totalTasks / (double)pageSize);
            List<TaskInfoApplicationDto> pagedTasks = allTasks.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var model = new Tuple<List<TaskInfoApplicationDto>, int, int>(pagedTasks, page, totalPages);
            ViewBag.SearchQuery = searchQuery;

            return View("~/Views/Task/GetTasks.cshtml", model);

        }

        [HttpPost]
        public IActionResult ChangeTaskStatus(string taskName, string ipAddress, string status)
        {
            try
            {
                // Parametrelerin kontrolü
                if (string.IsNullOrEmpty(taskName) || string.IsNullOrEmpty(ipAddress) || string.IsNullOrEmpty(status))
                {
                    return Json(new { success = false, message = "Eksik parametre gönderildi." });
                }

                // Durum geçerliliği kontrolü
                var validStatuses = new[] { "disabled", "ready", "running" };
                if (!validStatuses.Contains(status.ToLower()))
                {
                    return Json(new { success = false, message = "Geçersiz durum: " + status });
                }

                string resultMessage;

                // Durum değişikliği
                switch (status.ToLower())
                {
                    case "disabled":
                        resultMessage = _taskService.DisableTask(ipAddress, taskName);
                        break;
                    case "ready":
                        resultMessage = _taskService.EnableTask(ipAddress, taskName);
                        break;
                    case "running":
                        resultMessage = _taskService.StartTask(ipAddress, taskName);
                        break;
                    default:
                        return Json(new { success = false, message = "Geçersiz durum: " + status });
                }

                return Json(new { success = true, message = resultMessage });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Hata oluştu: " + ex.Message });
            }
        }
    }
}
