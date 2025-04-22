using Microsoft.Web.Administration;
using ServerApps.Business.Dtos;
using ServerApps.Business.Usescasess.Configuration;
using ServerApps.Business.Usescasess.IIS;
using ServerApps.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

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
        var configurations = _configurationService.GetConfigurations();

        foreach (var config in configurations)
        {
            try
            {
                if (config.Ip == "127.0.0.1" || config.Ip.ToLower().Contains("localhost"))
                {
                    // Local IIS için
                    using (var manager = new ServerManager())
                    {
                        foreach (var site in manager.Sites)
                        {
                            var binding = site.Bindings.FirstOrDefault();

                            result.Add(new GetServerAppDto
                            {
                                ApplicationName = site.Name,
                                Ip = config.Ip,
                                Port = binding?.EndPoint?.Port ?? 0,
                                Status = site.State.ToString()
                            });
                        }
                    }
                }
                else
                {
                    // Uzak sunucu için PowerShell
                    var securePwd = new System.Security.SecureString();
                    foreach (char c in config.Password) securePwd.AppendChar(c);
                    securePwd.MakeReadOnly();

                    var credential = new PSCredential(config.Username, securePwd);

                    var connectionInfo = new WSManConnectionInfo(
                        new Uri($"https://{config.Ip}:5985/wsman"), // HTTPS bağlantısı
                        "http://schemas.microsoft.com/powershell/Microsoft.PowerShell",
                        credential
                    );

                    connectionInfo.AuthenticationMechanism = AuthenticationMechanism.Default;

                    using var runspace = RunspaceFactory.CreateRunspace(connectionInfo);
                    runspace.Open();

                    using var ps = PowerShell.Create();
                    ps.Runspace = runspace;

                    ps.AddScript("Import-Module WebAdministration; Get-Website | Select Name, State, Bindings");

                    var powershellResult = ps.Invoke();

                    foreach (var item in powershellResult)
                    {
                        dynamic bindings = item.Members["Bindings"].Value;
                        string bindingInfo = bindings[0]?.ToString(); // "http/*:80:"
                        string portStr = bindingInfo?.Split(':')[1] ?? "0";

                        result.Add(new GetServerAppDto
                        {
                            ApplicationName = item.Members["Name"].Value.ToString(),
                            Ip = config.Ip,
                            Port = int.TryParse(portStr, out var p) ? p : 0,
                            Status = item.Members["State"].Value.ToString()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                result.Add(new GetServerAppDto
                {
                    ApplicationName = "ERROR",
                    Ip = config.Ip,
                    Port = 0,
                    Status = $"Bağlantı hatası: {ex.Message}"
                });
            }
        }

        return result;
    }
}
