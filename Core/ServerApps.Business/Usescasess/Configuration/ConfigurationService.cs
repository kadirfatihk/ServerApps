using Microsoft.Extensions.Configuration;
using ServerApps.Business.Dtos.ConfigurationDtos;
using ServerApps.Business.Usescasess.Configuration;
//using ServerApps.Core.Configuration;

namespace ServerApps.Business.Configuration
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly IConfiguration _configuration;

        public ConfigurationService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public List<GetConfigurationDto> GetConfigurations()
        {
            var configSection = _configuration.GetSection("Applications"); // "Applications" bölümünü alır
            var configList = new List<GetConfigurationDto>();

            foreach (var section in configSection.GetChildren())
            {
                var values = section.Get<string[]>(); // Her bölümün 3 elemanlı bir dizi olduğunu varsayar (IP, Username, Password)
                if (values.Length == 3)
                {
                    configList.Add(new GetConfigurationDto
                    {
                        Name = section.Key,
                        Ip = values[0],
                        Username = values[1],
                        Password = values[2]
                    });
                }
            }

            return configList;
        }
    }
}