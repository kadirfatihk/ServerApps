using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using ServerApps.Business.Dtos.EventDtos;
using ServerApps.Business.Dtos.ConfigurationDtos;
using ServerApps.Business.Usescasess.Configuration;
using ServerApps.Business.Usescasess.Event;

namespace ServerApps.Business.Usescasess
{
    public class EventService : IEventService
    {
        private readonly IConfigurationService _configurationService;

        public EventService(IConfigurationService configurationService)
        {
            _configurationService = configurationService;
        }

        public List<EventLogEntryDto> GetEventLogs(string serverIp, DateTime? startDate = null, DateTime? endDate = null, TimeSpan? startTime = null, TimeSpan? endTime = null)
        {
            var result = new List<EventLogEntryDto>();

            try
            {
                var config = _configurationService.GetConfigurations()
                    .FirstOrDefault(c => c.Ip == serverIp); // serverIp'ye göre yapılandırmayı al

                if (config == null)
                {
                    throw new Exception("Configuration not found for the given server IP.");
                }

                // PowerShell bağlantısı oluşturuluyor
                var securePwd = new System.Security.SecureString();
                foreach (char c in config.Password)
                    securePwd.AppendChar(c);
                securePwd.MakeReadOnly();

                var credential = new PSCredential(config.Username, securePwd);

                var connectionInfo = new WSManConnectionInfo(
                    new Uri($"http://{serverIp}:5985/wsman"),
                    "http://schemas.microsoft.com/powershell/Microsoft.PowerShell",
                    credential
                );

                connectionInfo.AuthenticationMechanism = AuthenticationMechanism.Default;

                using var runspace = RunspaceFactory.CreateRunspace(connectionInfo);
                runspace.Open();

                using var ps = PowerShell.Create();
                ps.Runspace = runspace;

                // PowerShell scripti: Event log sorgulama
                var script = $@"
                    Get-WinEvent -LogName Application | 
                    Where-Object {{
                        $eventTime = $_.TimeCreated
                        {GenerateDateFilter(startDate, endDate, startTime, endTime)}
                    }} |
                    Select-Object TimeCreated, LevelDisplayName, ProviderName, Id, TaskDisplayName, Message";
                ps.AddScript(script);

                var powershellResult = ps.Invoke(); // Komut çalıştırılır

                // Sonuçları işleyelim
                foreach (var item in powershellResult)
                {
                    result.Add(new EventLogEntryDto
                    {
                        TimeCreated = item.Members["TimeCreated"].Value as DateTime?,
                        Level = item.Members["LevelDisplayName"].Value?.ToString(),
                        Source = item.Members["ProviderName"].Value?.ToString(),
                        Id = item.Members["Id"].Value?.ToString(),
                        TaskCategory = item.Members["TaskDisplayName"].Value?.ToString(),
                        Message = item.Members["Message"].Value?.ToString()
                    });
                }

                if (result.Count == 0)
                {
                    throw new Exception("No events found for the specified filter.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while fetching event logs: {ex.Message}", ex);
            }

            return result;
        }

        private string GenerateDateFilter(DateTime? startDate, DateTime? endDate, TimeSpan? startTime, TimeSpan? endTime)
        {
            var dateFilter = string.Empty;

            if (startDate.HasValue)
            {
                dateFilter += "$eventTime -ge '" + startDate.Value.ToString("yyyy-MM-dd") + " " + (startTime?.ToString(@"hh\:mm") ?? "00:00") + "'";
            }
            if (endDate.HasValue)
            {
                if (!string.IsNullOrEmpty(dateFilter)) dateFilter += " -and ";
                dateFilter += "$eventTime -le '" + endDate.Value.ToString("yyyy-MM-dd") + " " + (endTime?.ToString(@"hh\:mm") ?? "23:59") + "'";
            }

            return dateFilter;
        }
    }
}
