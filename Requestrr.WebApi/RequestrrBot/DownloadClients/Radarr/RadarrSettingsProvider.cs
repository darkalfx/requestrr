namespace Requestrr.WebApi.RequestrrBot.DownloadClients.Radarr
{
    public class RadarrSettingsProvider
    {
        public RadarrSettings Provide()
        {
            dynamic settings = SettingsFile.Read();

            return new RadarrSettings
            {
                Hostname = settings.DownloadClients.Radarr.Hostname,
                BaseUrl = settings.DownloadClients.Radarr.BaseUrl,
                Port = (int)settings.DownloadClients.Radarr.Port,
                ApiKey = settings.DownloadClients.Radarr.ApiKey,
                MovieProfileId = (int)settings.DownloadClients.Radarr.MovieProfileId,
                MovieRootFolder = settings.DownloadClients.Radarr.MovieRootFolder,
                MovieMinimumAvailability = settings.DownloadClients.Radarr.MovieMinimumAvailability,
                MovieTags = settings.DownloadClients.Radarr.MovieTags.ToObject<int[]>(),
                AnimeProfileId = (int)settings.DownloadClients.Radarr.AnimeProfileId,
                AnimeRootFolder = settings.DownloadClients.Radarr.AnimeRootFolder,
                AnimeMinimumAvailability = settings.DownloadClients.Radarr.AnimeMinimumAvailability,
                AnimeTags = settings.DownloadClients.Radarr.AnimeTags.ToObject<int[]>(),
                SearchNewRequests  = settings.DownloadClients.Radarr.SearchNewRequests,
                MonitorNewRequests  = settings.DownloadClients.Radarr.MonitorNewRequests,
                UseSSL = (bool)settings.DownloadClients.Radarr.UseSSL,
                Version = settings.DownloadClients.Radarr.Version,
            };
        }
    }
}