namespace Requestrr.WebApi.Requestrr.DownloadClients
{
    public class SonarrSettingsProvider
    {
        public SonarrSettings Provide()
        {
            dynamic settings = SettingsFile.Read();

            return new SonarrSettings
            {
                Hostname = settings.DownloadClients.Sonarr.Hostname,
                BaseUrl = settings.DownloadClients.Sonarr.BaseUrl,
                Port = (int)settings.DownloadClients.Sonarr.Port,
                ApiKey = settings.DownloadClients.Sonarr.ApiKey,
                TvProfileId = settings.DownloadClients.Sonarr.TvProfileId,
                TvRootFolder = settings.DownloadClients.Sonarr.TvRootFolder,
                TvLanguageId = settings.DownloadClients.Sonarr.TvLanguageId,
                TvTags = settings.DownloadClients.Sonarr.TvTags.ToObject<int[]>(),
                TvUseSeasonFolders = settings.DownloadClients.Sonarr.TvUseSeasonFolders,
                AnimeProfileId = settings.DownloadClients.Sonarr.AnimeProfileId,
                AnimeRootFolder = settings.DownloadClients.Sonarr.AnimeRootFolder,
                AnimeLanguageId = settings.DownloadClients.Sonarr.AnimeLanguageId,
                AnimeTags = settings.DownloadClients.Sonarr.AnimeTags.ToObject<int[]>(),
                AnimeUseSeasonFolders = settings.DownloadClients.Sonarr.AnimeUseSeasonFolders,
                SearchNewRequests  = settings.DownloadClients.Sonarr.SearchNewRequests,
                MonitorNewRequests  = settings.DownloadClients.Sonarr.MonitorNewRequests,
                UseSSL = (bool)settings.DownloadClients.Sonarr.UseSSL,
                Version = settings.DownloadClients.Sonarr.Version,
            };
        }
    }
}