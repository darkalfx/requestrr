using Requestrr.WebApi.config;
using Requestrr.WebApi.RequestrrBot;
using Requestrr.WebApi.RequestrrBot.DownloadClients.Ombi;
using Requestrr.WebApi.RequestrrBot.DownloadClients.Overseerr;
using Requestrr.WebApi.RequestrrBot.DownloadClients.Radarr;
using Requestrr.WebApi.RequestrrBot.DownloadClients.Sonarr;

namespace Requestrr.WebApi.Controllers.DownloadClients
{
    public class DownloadClientsSettingsProvider
    {
        public DownloadClientsSettings Provide()
        {
            dynamic settings = SettingsFile.Read();

            return new DownloadClientsSettings
            {
                Ombi = new OmbiSettingsProvider().Provide(),
                Overseerr = new OverseerrSettingsProvider().Provide(),
                Radarr = new RadarrSettingsProvider().Provide(),
                Sonarr = new SonarrSettingsProvider().Provide(),
            };
        }
    }
}