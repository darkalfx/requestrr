namespace Requestrr.WebApi.RequestrrBot.DownloadClients.Overseerr
{
    public class OverseerrSettingsProvider
    {
        public OverseerrSettings Provide()
        {
            dynamic settings = SettingsFile.Read();

            return new OverseerrSettings
            {
                ApiKey = settings.DownloadClients.Overseerr.ApiKey,
                DefaultApiUserID = settings.DownloadClients.Overseerr.DefaultApiUserID,
                Hostname = settings.DownloadClients.Overseerr.Hostname,
                Port = settings.DownloadClients.Overseerr.Port,
                UseSSL = (bool)settings.DownloadClients.Overseerr.UseSSL,
                Version = settings.DownloadClients.Overseerr.Version,
            };
        }
    }
}