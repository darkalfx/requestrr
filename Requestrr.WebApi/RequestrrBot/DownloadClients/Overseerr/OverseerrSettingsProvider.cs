using System.Collections.Generic;

namespace Requestrr.WebApi.RequestrrBot.DownloadClients.Overseerr
{
    public class OverseerrSettingsProvider
    {
        public IDictionary<int, OverseerrDownloadClientSettings> Provide()
        {
            dynamic settings = SettingsFile.Read();
            return settings.DownloadClients.Where(x => x.ClientType == "RadarrDownloadClient").ToDictionary(x => x.Id, x => x);
        }
    }
}