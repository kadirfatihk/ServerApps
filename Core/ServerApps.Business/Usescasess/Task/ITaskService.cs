using ServerApps.Business.Dtos.TaskDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerApps.Business.Usescasess.Task
{
    public interface ITaskService
    {
        List<TaskInfoApplicationDto> GetAllTasks();
        string StartTask(string ip, string taskName);
        string DisableTask(string ip, string taskName);
        public string EnableTask(string ip, string taskName);
    }
}
