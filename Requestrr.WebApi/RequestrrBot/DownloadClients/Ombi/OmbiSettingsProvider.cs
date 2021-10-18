using System.Collections.Generic;

namespace Requestrr.WebApi.RequestrrBot.DownloadClients.Ombi
{
    public class OmbiSettingsProvider
    {
        public IDictionary<int, OmbiDownloadClientSettings> Provide()
        {
            dynamic settings = SettingsFile.Read();
            return settings.DownloadClients.Where(x => x.ClientType == "RadarrDownloadClient").ToDictionary(x => x.Id, x => x);
        }
    }
}