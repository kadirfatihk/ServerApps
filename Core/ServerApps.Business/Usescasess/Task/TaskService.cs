using Microsoft.Win32.TaskScheduler;
using ServerApps.Business.Dtos;
using ServerApps.Business.Usescasess.IIS;
using ServerApps.Business.Usescasess.Task;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

public class TaskService : ITaskService
{
    private readonly IIisService _iisService;

    public TaskService(IIisService iisService)
    {
        _iisService = iisService;
    }

    public List<TaskInfoApplicationDto> GetAllTasksFromWebSites()
    {
        var tasks = new List<TaskInfoApplicationDto>();

        var sites = _iisService.GetAllApplications(); // Web sitelerini al

        using (var ts = new Microsoft.Win32.TaskScheduler.TaskService())
        {
            foreach (var site in sites)
            {
                try
                {
                    var siteName = site.ApplicationName;

                    TaskFolder folder = ts.GetFolder($@"\{siteName}");

                    if (folder == null || folder.Tasks == null)
                        continue;

                    foreach (var task in folder.Tasks)
                    {
                        var triggerText = task.Definition.Triggers.Count > 0
                            ? FormatTrigger(task.Definition.Triggers[0])
                            : "No Trigger";

                        tasks.Add(new TaskInfoApplicationDto
                        {
                            TaskName = task.Name,
                            Status = task.State.ToString(),
                            Trigger = triggerText,
                            NextRunTime = task.NextRunTime,
                            LastRunTime = task.LastRunTime,
                            ApplicationName = siteName,
                            Ip = site.Ip, // Site üzerinden alınan gerçek IP
                            LastTaskResult = InterpretLastTaskResult(task.LastTaskResult)
                        });
                    }
                }
                catch
                {
                    continue;
                }
            }
        }

        return tasks;
    }

    //private string GetApplicationIp(string siteName)
    //{
    //    // Gerekirse appsettings üzerinden siteName'e göre IP alınabilir
    //    return "127.0.0.1";
    //}

    //private string GetLastTaskResultMessage(int resultCode)
    //{
    //    return resultCode switch
    //    {
    //        0 => "Başarıyla tamamlandı",
    //        1 => "Genel hata",
    //        267009 => "Görev zamanlanmadı",
    //        _ => $"Kod: {resultCode}"
    //    };
    //}

    private string InterpretLastTaskResult(int result)
    {
        if (result == 0)
            return "işlem başarıyla tamamlandı";

        if (result == 267011)
            return "Görev henüz çalıştırılmadı";

        try
        {
            string message = new Win32Exception(result).Message;
            return string.IsNullOrWhiteSpace(message) ? $"Kod: {result}" : message;
        }
        catch
        {
            return $"Kod: {result}";
        }
    }


    private string FormatTrigger(Trigger trigger)
    {
        try
        {
            return trigger switch
            {
                TimeTrigger timeTrigger => $"Belirli zaman: {timeTrigger.StartBoundary.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture)}",
                DailyTrigger dailyTrigger => $"Günlük: {dailyTrigger.StartBoundary.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture)}",
                WeeklyTrigger weeklyTrigger => $"Haftalık: {weeklyTrigger.DaysOfWeek} - {weeklyTrigger.StartBoundary.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture)}",
                _ => trigger.ToString()
            };
        }
        catch
        {
            return trigger.ToString(); // Hata varsa ham halini döndür
        }
    }
}
