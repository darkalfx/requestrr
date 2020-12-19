using Newtonsoft.Json.Linq;
using Requestrr.WebApi.config;
using Requestrr.WebApi.Controllers.DownloadClients.Lidarr;
using Requestrr.WebApi.Controllers.DownloadClients.Ombi;
using Requestrr.WebApi.Controllers.DownloadClients.Radarr;
using Requestrr.WebApi.Controllers.DownloadClients.Sonarr;
using Requestrr.WebApi.RequestrrBot;
using Requestrr.WebApi.RequestrrBot.Music;
using Requestrr.WebApi.RequestrrBot.TvShows;

namespace Requestrr.WebApi.Controllers.DownloadClients
{
    public static class DownloadClientsSettingsRepository
    {
        public static void SetDisabledClient(MoviesSettings movieSettings)
        {
            SettingsFile.Write(settings =>
            {
                settings.Movies.Client = movieSettings.Client;
            });
        }

        public static void SetOmbi(MoviesSettings movieSettings, OmbiSettingsModel ombiSettings)
        {
            SettingsFile.Write(settings =>
            {
                SetOmbiSettings(ombiSettings, settings);

                settings.Movies.Client = movieSettings.Client;
                settings.Movies.Command = movieSettings.Command;
            });
        }

        public static void SetRadarr(MoviesSettings movieSettings, RadarrSettingsModel radarrSettings)
        {
            SettingsFile.Write(settings =>
            {
                settings.DownloadClients.Radarr.Hostname = radarrSettings.Hostname;
                settings.DownloadClients.Radarr.Port = radarrSettings.Port;
                settings.DownloadClients.Radarr.ApiKey = radarrSettings.ApiKey;
                settings.DownloadClients.Radarr.BaseUrl = radarrSettings.BaseUrl;

                settings.DownloadClients.Radarr.MovieProfileId = radarrSettings.MovieProfile;
                settings.DownloadClients.Radarr.MovieRootFolder = radarrSettings.MoviePath;
                settings.DownloadClients.Radarr.MovieMinimumAvailability = radarrSettings.MovieMinAvailability;
                settings.DownloadClients.Radarr.MovieTags = JToken.FromObject(radarrSettings.MovieTags);

                settings.DownloadClients.Radarr.AnimeProfileId = radarrSettings.AnimeProfile;
                settings.DownloadClients.Radarr.AnimeRootFolder = radarrSettings.AnimePath;
                settings.DownloadClients.Radarr.AnimeMinimumAvailability = radarrSettings.AnimeMinAvailability;
                settings.DownloadClients.Radarr.AnimeTags = JToken.FromObject(radarrSettings.AnimeTags);

                settings.DownloadClients.Radarr.SearchNewRequests = radarrSettings.SearchNewRequests;
                settings.DownloadClients.Radarr.MonitorNewRequests = radarrSettings.MonitorNewRequests;

                settings.DownloadClients.Radarr.UseSSL = radarrSettings.UseSSL;
                settings.DownloadClients.Radarr.Version = radarrSettings.Version;

                settings.Movies.Client = movieSettings.Client;
                settings.Movies.Command = movieSettings.Command;
            });
        }

        public static void SetDisabledClient(TvShowsSettings tvShowsSettings)
        {
            SettingsFile.Write(settings =>
            {
                settings.TvShows.Client = tvShowsSettings.Client;
            });
        }

        public static void SetOmbi(TvShowsSettings tvShowsSettings, OmbiSettingsModel ombiSettings)
        {
            SettingsFile.Write(settings =>
            {
                SetOmbiSettings(ombiSettings, settings);

                settings.TvShows.Client = tvShowsSettings.Client;
                settings.TvShows.Command = tvShowsSettings.Command;
                settings.TvShows.Restrictions = tvShowsSettings.Restrictions;
            });
        }

        public static void SetSonarr(TvShowsSettings tvSettings, SonarrSettingsModel sonarrSettings)
        {
            SettingsFile.Write(settings =>
            {
                settings.DownloadClients.Sonarr.Hostname = sonarrSettings.Hostname;
                settings.DownloadClients.Sonarr.Port = sonarrSettings.Port;
                settings.DownloadClients.Sonarr.ApiKey = sonarrSettings.ApiKey;
                settings.DownloadClients.Sonarr.BaseUrl = sonarrSettings.BaseUrl;

                settings.DownloadClients.Sonarr.TvRootFolder = sonarrSettings.TvPath;
                settings.DownloadClients.Sonarr.TvProfileId = sonarrSettings.TvProfile;
                settings.DownloadClients.Sonarr.TvTags = JToken.FromObject(sonarrSettings.TvTags);
                settings.DownloadClients.Sonarr.TvLanguageId = sonarrSettings.TvLanguage;
                settings.DownloadClients.Sonarr.TvUseSeasonFolders = sonarrSettings.TvUseSeasonFolders;

                settings.DownloadClients.Sonarr.AnimeRootFolder = sonarrSettings.AnimePath;
                settings.DownloadClients.Sonarr.AnimeProfileId = sonarrSettings.AnimeProfile;
                settings.DownloadClients.Sonarr.AnimeTags = JToken.FromObject(sonarrSettings.AnimeTags);
                settings.DownloadClients.Sonarr.AnimeLanguageId = sonarrSettings.AnimeLanguage;
                settings.DownloadClients.Sonarr.AnimeUseSeasonFolders = sonarrSettings.AnimeUseSeasonFolders;

                settings.DownloadClients.Sonarr.SearchNewRequests = sonarrSettings.SearchNewRequests;
                settings.DownloadClients.Sonarr.MonitorNewRequests = sonarrSettings.MonitorNewRequests;

                settings.DownloadClients.Sonarr.UseSSL = sonarrSettings.UseSSL;
                settings.DownloadClients.Sonarr.Version = sonarrSettings.Version;

                settings.TvShows.Client = tvSettings.Client;
                settings.TvShows.Command = tvSettings.Command;
                settings.TvShows.Restrictions = tvSettings.Restrictions;
            });
        }

        public static void SetDisabledClient(MusicSettings musicSettings)
        {
            SettingsFile.Write(settings =>
            {
                settings.Music.Client = musicSettings.Client;
            });
        }

        public static void SetLidarr(MusicSettings musicSettings, LidarrSettingsModel lidarrSettings)
        {
            SettingsFile.Write(settings =>
            {
                settings.DownloadClients.Lidarr.Hostname = lidarrSettings.Hostname;
                settings.DownloadClients.Lidarr.Port = lidarrSettings.Port;
                settings.DownloadClients.Lidarr.ApiKey = lidarrSettings.ApiKey;
                settings.DownloadClients.Lidarr.BaseUrl = lidarrSettings.BaseUrl;

                settings.DownloadClients.Lidarr.MusicRootFolder = lidarrSettings.MusicPath;
                settings.DownloadClients.Lidarr.MusicProfileId = lidarrSettings.MusicProfile;
                settings.DownloadClients.Lidarr.MusicMetadataProfileId = lidarrSettings.MusicMetadataProfile;
                settings.DownloadClients.Lidarr.MusicTags = JToken.FromObject(lidarrSettings.MusicTags);
                settings.DownloadClients.Lidarr.MusicUseAlbumFolders = lidarrSettings.MusicUseAlbumFolders;

                settings.DownloadClients.Lidarr.SearchNewRequests = lidarrSettings.SearchNewRequests;
                settings.DownloadClients.Lidarr.MonitorNewRequests = lidarrSettings.MonitorNewRequests;
                settings.DownloadClients.Lidarr.UseSSL = lidarrSettings.UseSSL;

                settings.Music.Client = musicSettings.Client;
                settings.Music.Command = musicSettings.Command;
            });
        }

        private static void SetOmbiSettings(OmbiSettingsModel ombiSettings, dynamic settings)
        {
            settings.DownloadClients.Ombi.Hostname = ombiSettings.Hostname;
            settings.DownloadClients.Ombi.Port = ombiSettings.Port;
            settings.DownloadClients.Ombi.ApiKey = ombiSettings.ApiKey;
            settings.DownloadClients.Ombi.ApiUsername = ombiSettings.ApiUsername;
            settings.DownloadClients.Ombi.BaseUrl = ombiSettings.BaseUrl;
            settings.DownloadClients.Ombi.UseSSL = ombiSettings.UseSSL;
            settings.DownloadClients.Ombi.Version = ombiSettings.Version;
        }
    }
}