using Microsoft.AspNetCore.Mvc;
using ServerApps.Business.Dtos;
using ServerApps.Business.Usescasess.Task;
using System.Collections.Generic;

namespace ServerApps.WebApp.Controllers
{
    public class TaskController : Controller
    {
        private readonly ITaskService _taskService;

        // Constructor injection ile TaskService'i alıyoruz
        public TaskController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        // GET: /Task/GetTask
        public IActionResult GetTask(string searchIp, int page = 1)
        {
            // Varsayılan değerler
            int pageSize = 10; // Sayfa başına görev sayısı
            int totalTasks = 0;

            // Tüm görevleri al
            List<TaskInfoApplicationDto> allTasks = _taskService.GetAllTasksFromWebSites();

            // IP adresine göre arama yapılacaksa filtreleme
            if (!string.IsNullOrEmpty(searchIp))
            {
                allTasks = allTasks.FindAll(task => task.Ip.Contains(searchIp));
            }

            // Sayfalama işlemleri
            int totalPages = (int)Math.Ceiling(allTasks.Count / (double)pageSize);
            List<TaskInfoApplicationDto> pagedTasks = allTasks.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // Model için gereken verileri hazırlıyoruz
            var model = new Tuple<List<TaskInfoApplicationDto>, int, int>(pagedTasks, page, totalPages);

            // View'a veri gönderiyoruz
            ViewBag.SearchIp = searchIp; // Arama kutusundaki değeri korumak için
            return View(model);
        }
    }
}
