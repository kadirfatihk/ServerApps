using Microsoft.Win32.TaskScheduler;
using ServerApps.Business.Dtos;
using ServerApps.Business.Usescasess.IIS;
using ServerApps.Business.Usescasess.Task;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security;

public class TaskService : ITaskService
{
    private readonly IIisService _iisService;

    // Constructor: IIS servisi dışarıdan bağımlılık olarak alınır.
    // IIisService, IIS üzerindeki web sitelerinin bilgilerine erişim sağlar.
    public TaskService(IIisService iisService)
    {
        _iisService = iisService;
    }

    // Web sitelerinin tüm görevlerini almak için kullanılan ana metod.
    // Her bir web sitesi için Task Scheduler'dan görevler alınır.
    public List<TaskInfoApplicationDto> GetAllTasksFromWebSites()
    {
        var tasks = new List<TaskInfoApplicationDto>();  // Görev bilgilerini tutacak liste oluşturuluyor.
        var sites = _iisService.GetAllApplications();  // IIS üzerindeki tüm web sitesi bilgilerini almak için servis çağrılır.

        // Tüm siteler üzerinde döngü başlatılır.
        foreach (var site in sites)
        {
            try
            {
                // Eğer site localdeki bir site ise (127.0.0.1 ya da localhost)
                if (site.Ip == "127.0.0.1" || site.Ip.ToLower().Contains("localhost"))
                {
                    // LOCAL sunucu işlemleri: Local makinede IIS Task Scheduler'ı kullanarak görevler alınır.
                    using var ts = new Microsoft.Win32.TaskScheduler.TaskService();  // TaskService ile lokal görevler alınabilir.
                    TaskFolder folder = ts.GetFolder($@"\{site.ApplicationName}");  // IIS web sitesinin adını kullanarak ilgili Task Scheduler klasörüne ulaşılır.

                    // Eğer ilgili klasör veya içinde görevler yoksa, devam edilir (bir sonraki siteya geçilir).
                    if (folder == null || folder.Tasks == null)
                        continue;

                    // İlgili klasördeki görevler tek tek alınır.
                    foreach (var task in folder.Tasks)
                    {
                        // Görev bilgileri DTO'ya eklenir.
                        tasks.Add(new TaskInfoApplicationDto
                        {
                            TaskName = task.Name,  // Görev adı
                            Status = task.State.ToString(),  // Görevin mevcut durumu (Çalışıyor/Çalışmıyor vb.)
                            Trigger = task.Definition.Triggers.Count > 0 ? FormatTrigger(task.Definition.Triggers[0]) : "No Trigger",  // Görev tetikleyicisi varsa, formatlanarak eklenir.
                            NextRunTime = task.NextRunTime,  // Bir sonraki çalıştırma zamanı
                            LastRunTime = task.LastRunTime,  // Son çalıştırma zamanı
                            ApplicationName = site.ApplicationName,  // Uygulama adı
                            Ip = site.Ip,  // Uygulamanın IP adresi
                            LastTaskResult = InterpretLastTaskResult(task.LastTaskResult)  // Son çalıştırma sonucu anlamlı şekilde yorumlanır.
                        });
                    }
                }
                else
                {
                    // UZAK sunucu işlemleri: Uzak bir sunucu üzerinde PowerShell komutları kullanılarak görevler alınır.
                    string script = $@"
                        $folder = Get-ScheduledTask -TaskPath '\{site.ApplicationName}\' -ErrorAction SilentlyContinue
                        if ($folder) {{
                            $folder | ForEach-Object {{
                                $task = $_
                                $trigger = $task.Triggers | Select-Object -First 1
                                $info = [PSCustomObject]@{{
                                    Name = $task.TaskName
                                    State = $task.State.ToString()
                                    NextRunTime = $task.NextRunTime
                                    LastRunTime = $task.LastRunTime
                                    LastTaskResult = $task.LastTaskResult
                                    Trigger = $trigger
                                }}
                                $info
                            }}
                        }}
                    ";

                    // PowerShell scriptini uzaktaki sunucuda çalıştırmak için InvokeRemoteCommand metodu çağrılır.
                    var results = InvokeRemoteCommand(site.Ip, "admin", "password", script);  // Uzak sunucuya bağlanarak PowerShell scripti çalıştırılır.

                    // PowerShell sonucunda dönen her bir görev bilgisi üzerinden işlemler yapılır.
                    foreach (var result in results)
                    {
                        // PowerShell çıktısından elde edilen veriler DTO'ya eklenir.
                        tasks.Add(new TaskInfoApplicationDto
                        {
                            TaskName = result.Members["Name"]?.Value?.ToString(),  // Görev adı
                            Status = result.Members["State"]?.Value?.ToString(),  // Görev durumu
                            Trigger = FormatTrigger(result.Members["Trigger"]?.Value),  // Tetikleyici bilgisi
                            NextRunTime = ParseDateTime(result.Members["NextRunTime"]?.Value),  // Bir sonraki çalışma zamanı
                            LastRunTime = ParseDateTime(result.Members["LastRunTime"]?.Value),  // Son çalışma zamanı
                            ApplicationName = site.ApplicationName,  // Uygulama adı
                            Ip = site.Ip,  // IP adresi
                            LastTaskResult = InterpretLastTaskResult(Convert.ToInt32(result.Members["LastTaskResult"]?.Value))  // Son görev sonucu yorumlanır.
                        });
                    }
                }
            }
            catch
            {
                // Hata durumunda, bu site için görev alınamıyor ve bir sonraki siteya geçiliyor.
                continue;
            }
        }

        // Tüm görevler toplandıktan sonra, liste döndürülür.
        return tasks;
    }

    // Task Scheduler'dan dönen sonucun anlamlı bir şekilde yorumlanmasını sağlar.
    // Bu metod, görev sonucunun hata kodunu alıp açıklamasını döndürür.
    private string InterpretLastTaskResult(int result)
    {
        string hexCode = $"(0x{result:X})";  // Sonuç kodunu hex formatında alır.
        string message;

        // Eğer sonuç 0 ise, işlem başarıyla tamamlanmış demektir.
        if (result == 0)
            message = "İşlem başarıyla tamamlandı";
        // Eğer sonuç 267011 ise, görev henüz çalıştırılmamış demektir.
        else if (result == 267011)
            message = "Görev henüz çalıştırılmadı";
        else
        {
            try
            {
                // Diğer hata kodları için, ilgili hata mesajı alınır.
                message = new Win32Exception(result).Message;
            }
            catch
            {
                // Eğer hata mesajı alınamazsa, sadece hata kodu döndürülür.
                message = $"Kod: {result}";
            }
        }

        // Hata mesajı ve hex kodu birleştirilir ve döndürülür.
        return $"{message}. {hexCode}";
    }

    // Tetikleyicilerin formatlanmasını sağlayan metod.
    // Farklı tetikleyici türlerine göre anlamlı bir format üretir.
    private string FormatTrigger(object triggerObj)
    {
        if (triggerObj is Trigger trigger)
        {
            // Zaman tabanlı tetikleyicinin formatlanması.
            return trigger switch
            {
                TimeTrigger timeTrigger => $"{timeTrigger.StartBoundary:dd.MM.yyyy HH:mm} tarihinde {timeTrigger.StartBoundary:HH:mm} saatinde",  // Zamanla tetiklenen görevler.
                DailyTrigger dailyTrigger => $"{dailyTrigger.StartBoundary:dd.MM.yyyy} tarihinde {dailyTrigger.StartBoundary:HH:mm} saatinde (Günlük)",  // Günlük tetikleyiciler.
                WeeklyTrigger weeklyTrigger => $"{weeklyTrigger.StartBoundary:dd.MM.yyyy} tarihinde {weeklyTrigger.StartBoundary:HH:mm} saatinde (Haftalık - {weeklyTrigger.DaysOfWeek})",  // Haftalık tetikleyiciler.
                _ => trigger.ToString()  // Diğer tüm tetikleyici türleri için string döndürülür.
            };
        }

        // Eğer tetikleyici objesi null ise veya desteklenmeyen bir türse, "No Trigger" döndürülür.
        return triggerObj?.ToString() ?? "No Trigger";
    }

    // PowerShell'den dönen tarih bilgisini DateTime'a dönüştürür.
    private DateTime ParseDateTime(object value)
    {
        // Eğer tarih bilgisi geçerliyse, DateTime'a dönüştürülür. Geçerli değilse, DateTime.MinValue döndürülür.
        return DateTime.TryParse(value?.ToString(), out var dt) ? dt : DateTime.MinValue;
    }

    // Uzak sunucuya bağlanarak PowerShell komutunu çalıştırır ve dönen sonuçları listeler.
    private List<PSObject> InvokeRemoteCommand(string ip, string username, string password, string script)
    {
        var securePwd = new SecureString();
        foreach (char c in password)
            securePwd.AppendChar(c);  // Şifreyi güvenli bir şekilde SecureString'e dönüştürür.
        securePwd.MakeReadOnly();  // Şifrenin değiştirilmesini engeller.

        var credential = new PSCredential(username, securePwd);  // Kullanıcı adı ve şifreyi birleştirerek kimlik bilgileri oluşturulur.

        var connectionInfo = new WSManConnectionInfo(
            new Uri($"http://{ip}:5985/wsman"),  // Bağlantı adresi
            "http://schemas.microsoft.com/powershell/Microsoft.PowerShell",  // PowerShell URI'si
            credential)  // Kullanıcı kimlik bilgileri
        {
            AuthenticationMechanism = AuthenticationMechanism.Default  // Varsayılan kimlik doğrulama mekanizması kullanılır.
        };

        using var runspace = RunspaceFactory.CreateRunspace(connectionInfo);  // Uzak sunucuya bağlanılmak için runspace oluşturulur.
        runspace.Open();  // Runspace açılır.

        using var ps = PowerShell.Create();  // Yeni bir PowerShell nesnesi oluşturulur.
        ps.Runspace = runspace;  // Runspace'e bağlanılır.
        ps.AddScript(script);  // PowerShell scripti eklenir.

        return ps.Invoke().ToList();  // Script çalıştırılır ve sonuçlar döndürülür.
    }
}
