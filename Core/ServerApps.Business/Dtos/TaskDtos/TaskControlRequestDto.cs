using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerApps.Business.Dtos.TaskDtos
{
    public class TaskControlRequestDto
    {
        public string Ip { get; set; }
        public string TaskName { get; set; }
    }
}
