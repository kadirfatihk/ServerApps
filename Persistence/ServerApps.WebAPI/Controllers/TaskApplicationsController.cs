using Microsoft.AspNetCore.Mvc;
using ServerApps.Business.Dtos;
using ServerApps.Business.Usescasess.Task;
//using ServerApps.Business.Usescasess.TaskScheduler; // TaskService interface'inin namespace'i

namespace ServerApps.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskApplicationController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TaskApplicationController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        // GET api/taskapplication/tasks
        [HttpGet("tasks")]
        public ActionResult<List<TaskInfoApplicationDto>> GetTasks()
        {
            var tasks = _taskService.GetAllTasksFromWebSites();
            return Ok(tasks);
        }
    }
}
