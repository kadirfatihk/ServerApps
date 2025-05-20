using Microsoft.AspNetCore.Mvc;
using ServerApps.Business.Dtos.IisDtos;
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

    [HttpGet]
    public IActionResult GetWebSites(int page = 1, string searchQuery = "")
    {
        int pageSize = 20;

        // IIS'den uygulamaları ve konfigürasyonları al
        var applications = _iisService.GetAllApplications();
        var configurations = _configurationService.GetConfigurations();

        // Sunucu adlarını IP üzerinden grupla
        var serverNames = configurations
            .GroupBy(x => x.Ip)
            .ToDictionary(x => x.Key, x => x.First().Name);

        // ViewModel listesi oluştur
        var viewModels = applications.Select(app => new ServerAppViewModel
        {
            ApplicationName = app.ApplicationName,
            Ip = app.Ip,
            Port = app.Port,
            Status = app.Status
        }).ToList();

        // Genel arama filtresi
        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            viewModels = viewModels
                .Where(vm =>
                    (vm.ApplicationName != null && vm.ApplicationName.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)) ||
                    (vm.Ip != null && vm.Ip.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)) ||
                    (vm.Port.ToString().Contains(searchQuery)) ||
                    (vm.Status != null && vm.Status.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        // Toplam öğe sayısını ve sayfalama hesaplamalarını yap
        var totalItems = viewModels.Count;
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        page = page < 1 ? 1 : page > totalPages ? totalPages : page;

        // Sayfalama işlemi
        var pagedItems = viewModels
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // ViewBag ile arama sorgusunu aktar
        ViewBag.SearchQuery = searchQuery;

        // Model oluştur ve View'a gönder
        var model = new Tuple<List<ServerAppViewModel>, int, int>(pagedItems, page, totalPages);
        return View("~/Views/WebSite/GetWebSites.cshtml", model);
    }

    [HttpPost]
    public IActionResult StartWebSite([FromForm] StartStopWebSiteDto dto)
    {
        _iisService.StartWebSite(dto.Ip, dto.SiteName);
        return RedirectToAction("GetWebSites"); // Sayfayı yeniden yükle
    }

    [HttpPost]
    public IActionResult StopWebSite([FromForm] StartStopWebSiteDto dto)
    {
        _iisService.StopWebSite(dto.Ip, dto.SiteName);
        return RedirectToAction("GetWebSites"); // Sayfayı yeniden yükle
    }

    [HttpPost]
    public IActionResult UpdatePort([FromForm] UpdatePortDto dto)
    {
        _iisService.UpdateWebSitePort(dto.Ip, dto.SiteName, dto.NewPort);
        return RedirectToAction("GetWebSites"); // Sayfayı yeniden yükle
    }
}
