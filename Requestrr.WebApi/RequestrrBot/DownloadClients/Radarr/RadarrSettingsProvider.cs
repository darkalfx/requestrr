using System.Collections.Generic;

namespace Requestrr.WebApi.RequestrrBot.DownloadClients.Radarr
{
    public class RadarrSettingsProvider
    {
        public IDictionary<int, RadarrDownloadClientSettings> Provide()
        {
            dynamic settings = SettingsFile.Read();
            return settings.DownloadClients.Where(x => x.ClientType == "RadarrDownloadClient").ToDictionary(x => x.Id, x => x);
        }
    }
}