using Microsoft.Web.Administration;
using Microsoft.Win32.TaskScheduler; // TaskScheduler API'si
using ServerApps.Business.Dtos.IisDtos;
using ServerApps.Business.Usescasess.Configuration;
using ServerApps.Business.Usescasess.IIS;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading.Tasks;

public class IisService : IIisService
{
    private readonly IConfigurationService _configurationService;

    public IisService(IConfigurationService configurationService)
    {
        _configurationService = configurationService;
    }

    public List<GetServerAppDto> GetAllApplications()
    {
        var result = new List<GetServerAppDto>();
        var configurations = _configurationService.GetConfigurations(); // appsettings içindeki config listesi alınır

        foreach (var config in configurations) // Her IP için işlem yapılır
        {
            try
            {
                if (config.Ip == "127.0.0.1" || config.Ip.ToLower().Contains("localhost")) // Lokal bağlantı mı kontrolü
                {
                    // Local IIS için
                    using (var manager = new ServerManager())
                    {
                        foreach (var site in manager.Sites) // Tüm siteleri döner
                        {
                            var binding = site.Bindings.FirstOrDefault(); // İlk binding bilgisi alınır

                            result.Add(new GetServerAppDto
                            {
                                ApplicationName = site.Name, // Site adı
                                Ip = config.Ip,
                                Port = binding?.EndPoint?.Port ?? 0, // Binding portu varsa alınır
                                Status = site.State.ToString() // Site durumu
                            });
                        }
                    }
                }
                else
                {
                    // Uzak sunucu için PowerShell ile bağlan

                    // Şifre secure string’e çevrilir
                    var securePwd = new System.Security.SecureString();
                    foreach (char c in config.Password)
                        securePwd.AppendChar(c);
                    securePwd.MakeReadOnly();

                    var credential = new PSCredential(config.Username, securePwd); // PSCredential oluşturulur

                    // PowerShell Remoting bağlantısı için bilgiler girilir
                    var connectionInfo = new WSManConnectionInfo(
                        new Uri($"http://{config.Ip}:5985/wsman"),
                        "http://schemas.microsoft.com/powershell/Microsoft.PowerShell",
                        credential
                    );

                    connectionInfo.AuthenticationMechanism = AuthenticationMechanism.Default;

                    using var runspace = RunspaceFactory.CreateRunspace(connectionInfo); // Runspace oluşturulur
                    runspace.Open();

                    using var ps = PowerShell.Create();
                    ps.Runspace = runspace;

                    // PowerShell scripti: Get-Website + binding bilgilerini döner
                    ps.AddScript(@"
                        Import-Module WebAdministration
                        Get-Website | ForEach-Object {
                            $bindings = $_.Bindings.Collection | ForEach-Object { $_.bindingInformation }
                            [PSCustomObject]@{
                                Name = $_.Name
                                State = $_.State.ToString()
                                Bindings = $bindings
                            }
                        }
                    ");

                    var powershellResult = ps.Invoke(); // Komut çalıştırılır

                    foreach (var item in powershellResult) // Her site için döngü
                    {
                        var name = item.Members["Name"].Value?.ToString(); // Site adı alınır
                        var state = item.Members["State"].Value?.ToString(); // Durumu alınır
                        var bindingsValue = item.Members["Bindings"].Value; // Binding objesi alınır

                        List<string> bindingsList = new(); // Tüm binding string'leri burada toplanacak

                        // DEĞİŞTİRİLDİ: Binding verisinin tipi kontrol edilerek güvenli dönüştürme yapıldı
                        if (bindingsValue is PSObject psObj && psObj.BaseObject is IEnumerable<object> baseEnumerable)
                        {
                            // PSObject içinde liste varsa, toplanır
                            foreach (var obj in baseEnumerable)
                            {
                                bindingsList.Add(obj.ToString());
                            }
                        }
                        else if (bindingsValue is IEnumerable<object> rawEnumerable)
                        {
                            // Direkt IEnumerable ise listeye eklenir
                            foreach (var obj in rawEnumerable)
                            {
                                bindingsList.Add(obj.ToString());
                            }
                        }
                        else if (bindingsValue != null)
                        {
                            // Tek bir string ise direkt alınır
                            bindingsList.Add(bindingsValue.ToString());
                        }

                        // DEĞİŞTİRİLDİ: Binding string'inden güvenli şekilde port çıkarıldı
                        int port = 0;
                        var firstBinding = bindingsList.FirstOrDefault();
                        if (!string.IsNullOrEmpty(firstBinding))
                        {
                            var parts = firstBinding.Split(':'); // format: IP:PORT:HOSTNAME
                            if (parts.Length >= 2 && int.TryParse(parts[1], out var parsedPort))
                            {
                                port = parsedPort; // Port doğru şekilde alınır
                            }
                        }

                        // Sonuç listeye eklenir
                        result.Add(new GetServerAppDto
                        {
                            ApplicationName = name,
                            Ip = config.Ip,
                            Port = port,
                            Status = state
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                // Hata olursa özel bilgiyle birlikte hata sonucu döndürülür
                result.Add(new GetServerAppDto
                {
                    ApplicationName = "ERROR",
                    Ip = config.Ip,
                    Port = 0,
                    Status = $"Bağlantı hatası: {ex.Message}" // Daha açıklayıcı hata mesajı
                });
            }
        }

        return result; // Tüm siteler dönülür
    }

    public void StartWebSite(string ip, string siteName)
    {
        var configurations = _configurationService.GetConfigurations();
        var config = configurations.FirstOrDefault(c => c.Ip == ip);

        if (config is null)
            throw new Exception("Sunucu yapılandırması bulunamadı.");

        string safeSiteName = siteName.Replace("'", "''");

        if (ip == "127.0.0.1" || ip.ToLower().Contains("localhost"))
        {
            using (var manager = new ServerManager())
            {
                var site = manager.Sites.FirstOrDefault(s => s.Name == siteName);

                if (site is not null)
                    site.Start();
                else
                    throw new Exception($"Site '{siteName}' bulunamadı.");
            }
        }
        else
        {
            ExecutePowerShellCommand(ip, $@"
Import-Module WebAdministration
Start-Website -Name '{safeSiteName}'
");
        }
    }

    public void StopWebSite(string ip, string siteName)
    {
        var configurations = _configurationService.GetConfigurations();
        var config = configurations.FirstOrDefault(c => c.Ip == ip);

        if (config is null)
            throw new Exception("Sunucu yapılandırması bulunamadı.");

        string safeSiteName = siteName.Replace("'", "''");

        if (ip == "127.0.0.1" || ip.ToLower().Contains("localhost"))
        {
            using (var manager = new ServerManager())
            {
                var site = manager.Sites.FirstOrDefault(s => s.Name == siteName);

                if (site is not null)
                    site.Stop();
                else
                    throw new Exception($"Site '{siteName}' bulunamadı.");
            }
        }
        else
        {
            ExecutePowerShellCommand(ip, $@"
Import-Module WebAdministration
Stop-Website -Name '{safeSiteName}'
");
        }
    }

    public void UpdateWebSitePort(string ip, string siteName, int newPort)
    {
        var config = _configurationService.GetConfigurations();

        if (config is null)
            throw new Exception("Sunucu bulunamadı.");

        string safeSiteName = siteName.Replace("'", "''");

        if (ip == "127.0.0.1" || ip.ToLower().Contains("localhost"))
        {
            using var manager = new ServerManager();

            var site = manager.Sites.FirstOrDefault(s => s.Name == siteName);

            if (site is null)
                throw new Exception("Site bulunamadı.");

            var binding = site.Bindings.FirstOrDefault();
            if (binding == null)
                throw new Exception("Binding bulunamadı.");

            binding.BindingInformation = $"*:{newPort}:"; // Yeni port atanır
            manager.CommitChanges();
        }
        else
        {
            string psScript = $@"
Import-Module WebAdministration
$site = Get-Website -Name '{safeSiteName}'
if ($site -eq $null) {{ throw 'Site bulunamadı.' }}
$binding = Get-WebBinding -Name '{safeSiteName}' | Select-Object -First 1
if ($binding -ne $null) {{
    Remove-WebBinding -Name '{safeSiteName}' -BindingInformation $binding.bindingInformation -Protocol $binding.protocol
    New-WebBinding -Name '{safeSiteName}' -Protocol 'http' -Port {newPort} -IPAddress '*'
}}";

            ExecutePowerShellCommand(ip, psScript);
        }
    }


    //Bu metot, verilen IP adresindeki uzak bir Windows sunucusunda PowerShell komutu çalıştırmak için kullanılır.
    private void ExecutePowerShellCommand(string ip, string script)
    {
        // Konfigürasyonları al
        var configuration = _configurationService.GetConfigurations();

        // İlgili IP adresine sahip sunucu yapılandırmasını bul
        var config = configuration.FirstOrDefault(c => c.Ip == ip);

        // Yapılandırma bulunamazsa hata fırlat
        if (config is null)
            throw new Exception("Sunucu yapılandırması bulunamadı.");

        // Şifreyi güvenli hale getirmek için SecureString'e dönüştür
        var securePwd = new System.Security.SecureString();
        foreach (char c in config.Password)
            securePwd.AppendChar(c); // Her karakteri ekle
        securePwd.MakeReadOnly(); // SecureString artık değiştirilemez

        // Kullanıcı adı ve güvenli şifreyle PowerShell için kimlik bilgisi oluştur
        var credential = new PSCredential(config.Username, securePwd);

        // WSMan protokolü ile uzak bağlantı bilgisi oluştur
        var connectionInfo = new WSManConnectionInfo(
            new Uri($"http://{config.Ip}:5985/wsman"), // Uzak sunucunun WSMan URI'si
            "http://schemas.microsoft.com/powershell/Microsoft.PowerShell", // PowerShell namespace
            credential); // Kimlik bilgileri

        // Varsayılan kimlik doğrulama mekanizmasını kullan
        connectionInfo.AuthenticationMechanism = AuthenticationMechanism.Default;

        // PowerShell çalıştırma ortamı (runspace) oluştur ve aç
        using var runspace = RunspaceFactory.CreateRunspace(connectionInfo);
        runspace.Open(); // Bağlantıyı aç

        // PowerShell nesnesi oluştur
        using var ps = PowerShell.Create();
        ps.Runspace = runspace; // Oluşturulan runspace'i kullan

        // Çalıştırılacak PowerShell betiğini ekle
        ps.AddScript(script);

        // PowerShell komutunu çalıştır
        ps.Invoke();
    }

}
