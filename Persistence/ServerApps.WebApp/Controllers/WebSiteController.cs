using Microsoft.AspNetCore.Mvc;
using ServerApps.Business.Usescasess.Configuration;
using ServerApps.Business.Usescasess.IIS;
using ServerApps.WebApp.ViewModels;
using System.Collections.Generic;
using System.Linq;

public class WebSiteController : Controller
{
    private readonly IIisService _iisService;
    private readonly IConfigurationService _configurationService;

    public WebSiteController(IIisService iisService, IConfigurationService configurationService)
    {
        _iisService = iisService;
        _configurationService = configurationService;
    }

    public IActionResult WebSite(int page = 1, string searchIp = "")
    {
        int pageSize = 10;

        var applications = _iisService.GetAllApplications();
        var configurations = _configurationService.GetConfigurations();

        var serverNames = configurations
            .GroupBy(x => x.Ip)
            .ToDictionary(x => x.Key, x => x.First().Name);

        var viewModels = applications.Select(app => new ServerAppViewModel
        {
            ApplicationName = app.ApplicationName,
            Ip = app.Ip,
            Port = app.Port,
            Status = app.Status,
            //ServerName = serverNames.ContainsKey(app.Ip) ? serverNames[app.Ip] : "Bilinmeyen"
        }).ToList();

        // Arama filtresi
        if (!string.IsNullOrWhiteSpace(searchIp))
        {
            viewModels = viewModels
                .Where(vm => vm.Ip != null && vm.Ip.Contains(searchIp))
                .ToList();
        }

        var totalItems = viewModels.Count;
        var totalPages = (int)System.Math.Ceiling(totalItems / (double)pageSize);

        page = page < 1 ? 1 : page > totalPages ? totalPages : page;

        var pagedItems = viewModels
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        ViewBag.SearchIp = searchIp;

        var model = new Tuple<List<ServerAppViewModel>, int, int>(pagedItems, page, totalPages);
        return View(model);
    }
}
