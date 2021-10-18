using System.Collections.Generic;

namespace Requestrr.WebApi.RequestrrBot.DownloadClients.Sonarr
{
    public class SonarrSettingsProvider
    {
        public IDictionary<int, SonarrDownloadClientSettings> Provide()
        {
            dynamic settings = SettingsFile.Read();
            return settings.DownloadClients.Where(x => x.ClientType == "RadarrDownloadClient").ToDictionary(x => x.Id, x => x);
        }
    }
}