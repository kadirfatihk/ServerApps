using Microsoft.AspNetCore.Mvc;
using ServerApps.Business.Dtos;
using ServerApps.Business.Usescasess.Configuration;
using ServerApps.Business.Usescasess.IIS;
using ServerApps.WebApp.ViewModels;
using System.Collections.Generic;
using System.Linq;

public class HomeController : Controller
{
    private readonly IIisService _iisService;
    private readonly IConfigurationService _configurationService;

    public HomeController(IIisService iisService, IConfigurationService configurationService)
    {
        _iisService = iisService;
        _configurationService = configurationService;
    }

    // Sayfalama i�in parametre alaca��z
    public IActionResult Index(int page = 1) // Varsay�lan olarak 1. sayfa g�sterilecek
    {
        int pageSize = 10; // Her sayfada 10 ��e g�sterilecek

        var applications = _iisService.GetAllApplications();
        var configurations = _configurationService.GetConfigurations();

        var serverNames = configurations
            .GroupBy(x => x.Ip)
            .ToDictionary(x => x.Key, x => x.First().Name);

        // Uygulamalar� view model'e d�n��t�r�yoruz
        var viewModels = applications.Select(app => new ServerAppViewModel
        {
            ApplicationName = app.ApplicationName,
            Ip = app.Ip,
            Port = app.Port,
            Status = app.Status,
            //ServerName = serverNames.ContainsKey(app.Ip) ? serverNames[app.Ip] : "Bilinmeyen"
        }).ToList();

        // Sayfalama: veriyi belirtilen sayfaya g�re b�l�yoruz
        var pagedItems = viewModels.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        // Sayfa say�s� hesaplama
        var totalPages = (int)Math.Ceiling(viewModels.Count / (double)pageSize);

        // Modelde sayfalama bilgilerini de g�nderiyoruz
        var model = new Tuple<List<ServerAppViewModel>, int, int>(pagedItems, page, totalPages);

        return View(model);
    }
}
