using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerApps.Business.Dtos.EventDtos
{
    public class EventLogEntryDto
    {
        public DateTime? TimeCreated { get; set; }         // Date and Time
        public string Level { get; set; }                  // Level
        public string Source { get; set; }                 // Source
        public string Id { get; set; }                     // Event ID
        public string TaskCategory { get; set; }           // Task Category
        public string Message { get; set; }
    }
}
