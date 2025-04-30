using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerApps.Business.Dtos
{
    public class TaskInfoApplicationDto
    {
        public string ApplicationName { get; set; } //site adı
        public string Ip { get; set; }  // ip adresi
        public string TaskName { get; set; }    // görev adı
        public string Status { get; set; }  // görev durumu
        public string Trigger { get; set; } // tetikleyici
        public DateTime NextRunTime { get; set; }   // bir sonraki çalışma zamanı
        public DateTime LastRunTime { get; set; }   // son çalışma zamanı
        public string LastTaskResult { get; set; } // son görev sonucu
    }

}
