using ServerApps.Business.Dtos; // Görev bilgilerini taşımak için kullanılan DTO sınıfını içe aktarır
using ServerApps.Business.Usescasess.Configuration; // Sunucu yapılandırmalarını getiren servis sınıfını içe aktarır
using ServerApps.Business.Usescasess.Task; // ITaskService arayüzünü içe aktarır
using System; // Temel .NET sınıfları için namespace
using System.Collections.Generic; // List gibi koleksiyon sınıfları için gerekli namespace
using System.Collections.ObjectModel; // PowerShell çıktılarında kullanılan koleksiyon tipi
using System.Management.Automation; // PowerShell komutlarını çalıştırmak için gerekli namespace
using System.Management.Automation.Runspaces; // Uzak PowerShell oturumu oluşturmak için gerekli sınıflar
using System.Security; // Güvenli parola nesnesi için kullanılan namespace

namespace ServerApps.Business.Usescasess.TaskScheduler
{
    public class TaskService : ITaskService // ITaskService arayüzünü uygulayan TaskService sınıfı
    {
        private readonly IConfigurationService _configurationService; // Uygulama yapılandırmalarını sağlayan servis

        public TaskService(IConfigurationService configurationService) // Constructor ile yapılandırma servisi alınır
        {
            _configurationService = configurationService;
        }

        public List<TaskInfoApplicationDto> GetAllTasks() // Tüm görevleri listeleyen ana metot
        {
            var result = new List<TaskInfoApplicationDto>(); // Sonuçların tutulacağı liste
            var configs = _configurationService.GetConfigurations(); // appsettings.json'dan tüm sunucu konfigürasyonlarını al

            foreach (var config in configs) // Her bir sunucu için döngü başlat
            {
                try
                {
                    if (config.Ip == "127.0.0.1" || config.Ip.ToLower().Contains("localhost"))                 // Eğer local sunucuysa
                    {
                        // LOCAL PowerShell
                        using (PowerShell ps = PowerShell.Create()) // Yeni bir PowerShell oturumu başlat
                        {
                            ps.AddScript(GetTaskListScript()); // Görev listeleme PowerShell script'ini ekle
                            var output = ps.Invoke(); // Script'i çalıştır ve sonucu al

                            result.AddRange(ParseTaskResult(output, config.Ip, "Local1")); // Çıktıyı ayrıştır ve listeye ekle
                        }
                    }
                    else // Eğer uzak sunucuysa
                    {
                        // UZAK PowerShell
                        var securePwd = new SecureString(); // Güvenli parola nesnesi oluştur
                        foreach (char c in config.Password) // Parolayı karakter karakter ekle
                            securePwd.AppendChar(c);
                        securePwd.MakeReadOnly(); // Parolayı sadece okunabilir yap

                        var credential = new PSCredential(config.Username, securePwd); // PSCredential nesnesi oluştur

                        var connectionInfo = new WSManConnectionInfo( // Uzak bağlantı bilgileri oluştur
                            new Uri($"http://{config.Ip}:5985/wsman"), // Uzak sunucu IP ve WSMan endpoint
                            "http://schemas.microsoft.com/powershell/Microsoft.PowerShell", // PowerShell endpoint URI
                            credential // Kullanıcı adı ve parola bilgisi
                        );
                        connectionInfo.AuthenticationMechanism = AuthenticationMechanism.Default; // Varsayılan kimlik doğrulama kullanılır

                        using var runspace = RunspaceFactory.CreateRunspace(connectionInfo); // Uzak oturum için runspace oluştur
                        runspace.Open(); // Runspace'i başlat

                        using var ps = PowerShell.Create(); // Yeni bir PowerShell oturumu oluştur
                        ps.Runspace = runspace; // Runspace'i PowerShell oturumuna bağla
                        ps.AddScript(GetTaskListScript()); // Görevleri listeleyecek script'i ekle

                        var output = ps.Invoke(); // Script'i çalıştır ve sonucu al

                        result.AddRange(ParseTaskResult(output, config.Ip, config.Name)); // Sonuçları ayrıştır ve listeye ekle
                    }
                }
                catch (Exception ex) // Hata durumunda
                {
                    result.Add(new TaskInfoApplicationDto // Hata bilgisiyle birlikte bir DTO nesnesi oluştur ve ekle
                    {
                        ApplicationName = config.Name,
                        Ip = config.Ip,
                        TaskName = "ERROR",
                        Status = $"Bağlantı hatası: {ex.Message}"
                    });
                }
            }

            return result; // Tüm görev listesini döndür
        }

        private string GetTaskListScript() // PowerShell script'ini döndüren metot
        {
            return @"
                Get-ScheduledTask -TaskPath '\' | ForEach-Object {  # Sadece root (ana dizin) altındaki görevleri al
                    $info = $_ | Get-ScheduledTaskInfo             # Görev bilgilerini al
                    [PSCustomObject]@{                              # Yeni bir özel nesne oluştur
                        TaskName = $_.TaskName                     # Görev adı
                        State = $_.State.ToString()               # Görev durumu (Ready, Running vs.)
                        Trigger = ($_.Triggers | Select-Object -First 1).ToString() # İlk tetikleyici bilgisini al
                        LastRunTime = $info.LastRunTime           # Son çalıştırılma zamanı
                        NextRunTime = $info.NextRunTime           # Bir sonraki çalıştırma zamanı
                        LastTaskResult = $info.LastTaskResult     # Son çalıştırma sonucu (exit code)
                    }
                }
            ";
        }

        private List<TaskInfoApplicationDto> ParseTaskResult(Collection<PSObject> output, string ip, string appName) // PowerShell çıktısını parse eden metot
        {
            var list = new List<TaskInfoApplicationDto>(); // Yeni bir görev DTO listesi oluştur

            foreach (var item in output) // Her bir PowerShell çıktısı için döngü
            {
                int.TryParse(item.Members["LastTaskResult"]?.Value?.ToString(), out int resultCode); // Çıkış kodunu al ve sayıya çevir

                list.Add(new TaskInfoApplicationDto // DTO nesnesi oluştur ve bilgileri ata
                {
                    ApplicationName = appName,
                    Ip = ip,
                    TaskName = item.Members["TaskName"]?.Value?.ToString(), // Görev adı
                    Status = item.Members["State"]?.Value?.ToString(), // Görev durumu
                    Trigger = item.Members["Trigger"]?.Value?.ToString(), // Tetikleyici bilgisi
                    LastRunTime = item.Members["LastRunTime"]?.Value as DateTime?, // Son çalıştırma zamanı
                    NextRunTime = item.Members["NextRunTime"]?.Value as DateTime?, // Bir sonraki çalıştırma zamanı
                    LastTaskResult = MapTaskResult(resultCode) // Çıkış kodunu açıklamalı ifadeye çevir
                });
            }

            return list; // Oluşturulan listeyi döndür
        }

        private string MapTaskResult(int code) // Görev çıkış kodunu açıklamalı duruma çeviren metot
        {
            return code switch
            {
                0 => "Başarıyla tamamlandı", // 0: Başarı
                1 => "İşlem çalıştırılamadı", // 1: Hata
                267009 => "Görev henüz çalıştırılmadı", // 267009: Henüz çalıştırılmamış görev
                267014 => "Zamanlayıcı tarafından atlandı", // 267014: Atlanmış görev
                _ => $"Hata kodu: {code}" // Diğer tüm kodlar için varsayılan açıklama
            };
        }


    }
}
