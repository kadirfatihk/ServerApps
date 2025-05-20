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

        public List<EventLogEntryDto> GetEventLogs(string serverIp, DateTime? startDateTime = null, DateTime? endDateTime = null)
        {
            var result = new List<EventLogEntryDto>();

            try
            {
                // Sunucu IP'sine göre yapılandırma bilgilerini getir
                var config = _configurationService.GetConfigurations()
                    .FirstOrDefault(c => c.Ip == serverIp);

                if (config == null)
                    throw new Exception("Configuration not found for the given server IP.");

                // Kullanıcı şifresini SecureString'e dönüştür
                var securePwd = new System.Security.SecureString();
                foreach (char c in config.Password)
                    securePwd.AppendChar(c);
                securePwd.MakeReadOnly();

                // PSCredential nesnesi oluştur
                var credential = new PSCredential(config.Username, securePwd);

                // Uzak PowerShell bağlantısı için WSManConnectionInfo oluştur
                var connectionInfo = new WSManConnectionInfo(
                    new Uri($"http://{serverIp}:5985/wsman"),
                    "http://schemas.microsoft.com/powershell/Microsoft.PowerShell",
                    credential
                );

                // Varsayılan kimlik doğrulama yöntemi kullanılıyor
                connectionInfo.AuthenticationMechanism = AuthenticationMechanism.Default;

                // Runspace (çalışma alanı) oluştur ve aç
                using var runspace = RunspaceFactory.CreateRunspace(connectionInfo);
                runspace.Open();

                // PowerShell nesnesi oluştur ve runspace'e bağla
                using var ps = PowerShell.Create();
                ps.Runspace = runspace;

                // Eğer başlangıç tarihi belirtilmemişse bugünün tarihi olarak ayarla
                if (!startDateTime.HasValue)
                    startDateTime = DateTime.Today;

                // Eğer bitiş tarihi belirtilmemişse gün sonu olarak ayarla
                if (!endDateTime.HasValue)
                    endDateTime = DateTime.Today.AddDays(1).AddSeconds(-1);

                // PowerShell betiği: Application loglarını al, tarih filtresi uygula ve gerekli alanları seç
                var script = $@"
Get-WinEvent -LogName Application | 
Where-Object {{
    $eventTime = $_.TimeCreated
    {GenerateDateFilter(startDateTime, endDateTime)}
}} |
Select-Object TimeCreated, LevelDisplayName, ProviderName, Id, TaskDisplayName, Message";

                // Betiği PowerShell'e ekle
                ps.AddScript(script);

                // Betiği çalıştır
                var powershellResult = ps.Invoke();

                // Her sonucu Dto olarak dönüştür ve listeye ekle
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

                // Eğer sonuç boşsa hata fırlat
                if (result.Count == 0)
                    throw new Exception("No events found for the specified filter.");
            }
            catch (Exception ex)
            {
                // Hata durumunda açıklayıcı mesaj ile birlikte istisna fırlat
                throw new Exception($"An error occurred while fetching event logs: {ex.Message}", ex);
            }

            return result;
        }

        // PowerShell betiğinde kullanılmak üzere tarih filtresi üretir
        private string GenerateDateFilter(DateTime? startDateTime, DateTime? endDateTime)
        {
            var filter = "";

            if (startDateTime.HasValue)
                filter += $"$eventTime -ge '{startDateTime.Value:yyyy-MM-dd HH:mm}'";

            if (endDateTime.HasValue)
            {
                if (!string.IsNullOrEmpty(filter)) filter += " -and ";
                filter += $"$eventTime -le '{endDateTime.Value:yyyy-MM-dd HH:mm}'";
            }

            return filter;
        }
    }
}
