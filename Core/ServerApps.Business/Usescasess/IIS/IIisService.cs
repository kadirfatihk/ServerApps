using System.Collections.Generic;
using ServerApps.Business.Dtos;

namespace ServerApps.Business.Usescasess.IIS
{
    public interface IIisService
    {
        List<GetServerAppDto> GetAllApplications();
    }
}
