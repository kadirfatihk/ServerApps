using ServerApps.Business.Dtos;
using ServerApps.Business.Usescasess.Configuration;
using ServerApps.Business.Usescasess.Task;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security;

namespace ServerApps.Business.Usescasess.TaskScheduler
{
    public class TaskService : ITaskService
    {
        private readonly IConfigurationService _configurationService;

        public TaskService(IConfigurationService configurationService)
        {
            _configurationService = configurationService;
        }

        public List<TaskInfoApplicationDto> GetAllTasks()
        {
            var result = new List<TaskInfoApplicationDto>();
            var configs = _configurationService.GetConfigurations();

            foreach (var config in configs)
            {
                try
                {
                    if (config.Ip == "127.0.0.1" || config.Ip.ToLower().Contains("localhost"))
                    {
                        // LOCAL PowerShell
                        using (PowerShell ps = PowerShell.Create())
                        {
                            ps.AddScript(GetTaskListScript());
                            var output = ps.Invoke();

                            result.AddRange(ParseTaskResult(output, config.Ip, "LOCAL"));
                        }
                    }
                    else
                    {
                        // UZAK PowerShell
                        var securePwd = new SecureString();
                        foreach (char c in config.Password)
                            securePwd.AppendChar(c);
                        securePwd.MakeReadOnly();

                        var credential = new PSCredential(config.Username, securePwd);

                        var connectionInfo = new WSManConnectionInfo(
                            new Uri($"http://{config.Ip}:5985/wsman"),
                            "http://schemas.microsoft.com/powershell/Microsoft.PowerShell",
                            credential
                        );
                        connectionInfo.AuthenticationMechanism = AuthenticationMechanism.Default;

                        using var runspace = RunspaceFactory.CreateRunspace(connectionInfo);
                        runspace.Open();

                        using var ps = PowerShell.Create();
                        ps.Runspace = runspace;
                        ps.AddScript(GetTaskListScript());

                        var output = ps.Invoke();

                        result.AddRange(ParseTaskResult(output, config.Ip, config.Name));
                    }
                }
                catch (Exception ex)
                {
                    result.Add(new TaskInfoApplicationDto
                    {
                        ApplicationName = config.Name,
                        Ip = config.Ip,
                        TaskName = "ERROR",
                        Status = $"Bağlantı hatası: {ex.Message}"
                    });
                }
            }

            return result;
        }

        private string GetTaskListScript()
        {
            return @"
                Get-ScheduledTask -TaskPath '\' | ForEach-Object {
                    $info = $_ | Get-ScheduledTaskInfo
                    [PSCustomObject]@{
                        TaskName = $_.TaskName
                        State = $_.State.ToString()
                        Trigger = ($_.Triggers | Select-Object -First 1).ToString()
                        LastRunTime = $info.LastRunTime
                        NextRunTime = $info.NextRunTime
                        LastTaskResult = $info.LastTaskResult
                    }
                }
            ";
        }

        private List<TaskInfoApplicationDto> ParseTaskResult(Collection<PSObject> output, string ip, string appName)
        {
            var list = new List<TaskInfoApplicationDto>();

            foreach (var item in output)
            {
                int.TryParse(item.Members["LastTaskResult"]?.Value?.ToString(), out int resultCode);

                list.Add(new TaskInfoApplicationDto
                {
                    ApplicationName = appName,
                    Ip = ip,
                    TaskName = item.Members["TaskName"]?.Value?.ToString(),
                    Status = item.Members["State"]?.Value?.ToString(),
                    Trigger = item.Members["Trigger"]?.Value?.ToString(),
                    LastRunTime = item.Members["LastRunTime"]?.Value as DateTime?,
                    NextRunTime = item.Members["NextRunTime"]?.Value as DateTime?,
                    LastTaskResult = MapTaskResult(resultCode)
                });
            }

            return list;
        }

        private string MapTaskResult(int code)
        {
            return code switch
            {
                0 => "Başarıyla tamamlandı",
                1 => "İşlem çalıştırılamadı",
                267009 => "Görev henüz çalıştırılmadı",
                267014 => "Zamanlayıcı tarafından atlandı",
                _ => $"Hata kodu: {code}"
            };
        }
    }
}
