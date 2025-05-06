using Microsoft.AspNetCore.Mvc;
using ServerApps.Business.Dtos;
using ServerApps.Business.Usescasess.Task;
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

        public IActionResult GetTask(string searchQuery, int page = 1)
        {
            int pageSize = 10;

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
            return View(model);
        }
    }
}
