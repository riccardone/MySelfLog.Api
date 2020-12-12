using Microsoft.Extensions.Configuration;
using MySelfLog.Contracts.Api;

namespace MySelfLog.Api.Services
{
    public class BusSettings : IBusSettings
    {
        private readonly IConfiguration _configuration;

        public BusSettings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string Link => _configuration.Get<AppSettings>().Link;
        public string Name => _configuration.Get<AppSettings>().ConnectionName;
    }
}
