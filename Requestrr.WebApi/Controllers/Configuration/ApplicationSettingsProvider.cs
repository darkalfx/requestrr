using Requestrr.WebApi.RequestrrBot;
using Requestrr.WebApi.config;

namespace Requestrr.WebApi.Controllers.Configuration
{
    public class ApplicationSettingsProvider
    {
        public ApplicationSettings Provide()
        {
            dynamic settings = SettingsFile.Read();

            return new ApplicationSettings
            {
                Port = (int)settings.Port,
                BaseUrl = (string)settings.BaseUrl,
                DisableAuthentication = (bool)settings.DisableAuthentication,
            };
        }
    }
}