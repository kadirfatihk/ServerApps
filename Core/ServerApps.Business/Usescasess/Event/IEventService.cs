using ServerApps.Business.Dtos.EventDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerApps.Business.Usescasess.Event
{
    public interface IEventService
    {
        List<EventLogEntryDto> GetEventLogs(string serverIp, DateTime? startDateTime = null, DateTime? endDateTime = null);
    }
}
