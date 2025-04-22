using Microsoft.AspNetCore.Mvc;
using ServerApps.Business.Dtos;
using ServerApps.Business.Usescasess.Configuration;
using ServerApps.Business.Usescasess.IIS;
using System.Net.Http;

public class HomeController : Controller
{
    private readonly IIisService _iisService;
    private readonly IConfigurationService _configurationService;

    public HomeController(IIisService iisService, IConfigurationService configurationService)
    {
        _iisService = iisService;
        _configurationService = configurationService;
    }

    //public IActionResult Index()
    //{
    //    List<GetServerAppDto> applications = _iisService.GetAllApplications();
    //    return View(applications);
    //}

    //public IActionResult Index()
    //{
    //    List<GetServerAppDto> applications = _iisService.GetAllApplications();

    //    // IP adresine göre gruplandýrma
    //    Dictionary<string, List<GetServerAppDto>> groupedApplications = applications
    //        .GroupBy(app => app.Ip)
    //        .ToDictionary(
    //            group => group.Key,
    //            group => group.ToList()
    //        );

    //    return View(groupedApplications);
    //}

    public IActionResult Index()
    {
        List<GetServerAppDto> applications = _iisService.GetAllApplications();

        // IP ve isimleri bir sözlükte sakla
        Dictionary<string, string> serverNames = new Dictionary<string, string>();
        var configurations = _configurationService.GetConfigurations();
        foreach (var config in configurations)
        {
            if (!serverNames.ContainsKey(config.Ip))
            {
                serverNames[config.Ip] = config.Name;
            }
        }

        // IP adresine göre gruplandýrma
        Dictionary<string, List<GetServerAppDto>> groupedApplications = applications
            .GroupBy(app => app.Ip)
            .ToDictionary(
                group => group.Key,
                group => group.ToList()
            );

        // View'a göndermek için bir model oluþtur.
        var viewModel = new Tuple<Dictionary<string, List<GetServerAppDto>>, Dictionary<string, string>>(groupedApplications, serverNames);
        return View(viewModel);
    }
}