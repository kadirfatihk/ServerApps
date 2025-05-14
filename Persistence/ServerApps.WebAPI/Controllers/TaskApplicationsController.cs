using Microsoft.AspNetCore.Mvc;
using ServerApps.Business.Dtos.TaskDtos;
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
            var tasks = _taskService.GetAllTasks();
            return Ok(tasks);
        }

        [HttpPost("disable")]
        public IActionResult Disable([FromBody] TaskControlRequestDto dto)
        {
            var result = _taskService.DisableTask(dto.Ip, dto.TaskName);
            return Ok(result);
        }

        [HttpPost("ready")]
        public IActionResult Ready([FromBody] TaskControlRequestDto dto)
        {
            var result = _taskService.EnableTask(dto.Ip, dto.TaskName);
            return Ok(result);
        }

        [HttpPost("running")]
        public IActionResult Running([FromBody] TaskControlRequestDto dto)
        {
            var result = _taskService.StartTask(dto.Ip, dto.TaskName);
            return Ok(result);
        }

    }
}
