using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerApps.Business.Dtos.TaskDtos
{
    public class TaskStatusUpdateDto
    {
        public string TaskName { get; set; }
        public string Status { get; set; }
    }
}
