using Microsoft.Web.Administration;
using ServerApps.Business.Dtos;
using ServerApps.Business.Usescasess.Configuration;
using ServerApps.Business.Usescasess.IIS;
using ServerApps.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System;

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

                    // Şifre secure string'e çevrilir
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

                        // Eski hali:
                        // var port = bindings.FirstOrDefault()?.ToString().Split(':')[1]; // Bu hali patlamaya açıktı

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
                    Status = $"Bağlantı hatası: {ex.Message}" // DEĞİŞTİRİLDİ: Daha açıklayıcı hata mesajı
                });
            }
        }

        return result; // Tüm siteler dönülür
    }
}