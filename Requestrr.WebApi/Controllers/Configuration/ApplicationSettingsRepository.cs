using Requestrr.WebApi.RequestrrBot;
using Requestrr.WebApi.config;

namespace Requestrr.WebApi.Controllers.Configuration
{
    public static class ApplicationSettingsRepository
    {
        public static void Update(ApplicationSettings applicationSettings)
        {
            SettingsFile.Write(settings =>
            {
                settings.Port = applicationSettings.Port;
                settings.BaseUrl = applicationSettings.BaseUrl;
            });
        }
    }
}