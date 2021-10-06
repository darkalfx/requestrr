namespace Requestrr.WebApi.RequestrrBot.DownloadClients.Ombi
{
    public class OmbiSettingsProvider
    {
        public OmbiSettings Provide()
        {
            dynamic settings = SettingsFile.Read();

            return new OmbiSettings
            {
                ApiKey = settings.DownloadClients.Ombi.ApiKey,
                ApiUsername = settings.DownloadClients.Ombi.ApiUsername,
                Hostname = settings.DownloadClients.Ombi.Hostname,
                BaseUrl = settings.DownloadClients.Ombi.BaseUrl,
                Port = settings.DownloadClients.Ombi.Port,
                UseSSL = (bool)settings.DownloadClients.Ombi.UseSSL,
                Version = settings.DownloadClients.Ombi.Version,
            };
        }
    }
}