using Requestrr.WebApi.RequestrrBot.DownloadClients.Ombi;
using Requestrr.WebApi.RequestrrBot.DownloadClients.Overseerr;
using Requestrr.WebApi.RequestrrBot.DownloadClients.Radarr;
using Requestrr.WebApi.RequestrrBot.DownloadClients.Sonarr;

namespace Requestrr.WebApi.config
{
    public class DownloadClientsSettings
    {
        public OmbiSettings Ombi { get; set; }
        public OverseerrSettings Overseerr { get; set; }
        public RadarrSettings Radarr { get; set; }
        public SonarrSettings Sonarr { get; set; }
    }
}