using System.Collections.Generic;
using ServerApps.Business.Dtos.IisDtos;

namespace ServerApps.Business.Usescasess.IIS
{
    public interface IIisService
    {
        List<GetServerAppDto> GetAllApplications();
        void StartWebSite(string ip, string siteName);  // Web sitesini başlatmak için metod
        void StopWebSite(string ip, string siteName);    // Web sitesini durdurmak için metod
        void UpdateWebSitePort(string ip, string siteName, int newPort);
    }
}
