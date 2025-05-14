using ServerApps.Business.Dtos.ConfigurationDtos;
using System.Collections.Generic;

namespace ServerApps.Business.Usescasess.Configuration
{
    public interface IConfigurationService
    {
        List<GetConfigurationDto> GetConfigurations();
    }
}