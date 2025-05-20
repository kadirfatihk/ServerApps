using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerApps.Business.Dtos.EventDtos
{
    public class EventLogEntryDto
    {
        public DateTime? TimeCreated { get; set; }       
        public string Level { get; set; }                 
        public string Source { get; set; }               
        public string Id { get; set; }                  
        public string TaskCategory { get; set; }        
        public string Message { get; set; }
    }
}
