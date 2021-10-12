using System.Linq;
using Newtonsoft.Json.Linq;
using Requestrr.WebApi.Controllers.DownloadClients.Ombi;
using Requestrr.WebApi.Controllers.DownloadClients.Overseerr;
using Requestrr.WebApi.Controllers.DownloadClients.Radarr;
using Requestrr.WebApi.Controllers.DownloadClients.Sonarr;
using Requestrr.WebApi.RequestrrBot;
using Requestrr.WebApi.RequestrrBot.DownloadClients.Radarr;
using Requestrr.WebApi.RequestrrBot.DownloadClients.Sonarr;
using Requestrr.WebApi.RequestrrBot.Movies;
using Requestrr.WebApi.RequestrrBot.TvShows;

namespace Requestrr.WebApi.Controllers.DownloadClients
{
    public static class DownloadClientsSettingsRepository
    {
        public static void SetDisabledClient(MoviesSettings movieSettings)
        {
            SettingsFile.Write(settings =>
            {
                NotificationsFile.ClearAllMovieNotifications();
                settings.Movies.Client = movieSettings.Client;
            });
        }

        public static void SetOmbi(MoviesSettings movieSettings, OmbiSettingsModel ombiSettings)
        {
            SettingsFile.Write(settings =>
            {
                SetOmbiSettings(ombiSettings, settings);
                SetMovieSettings(movieSettings, settings);
            });
        }

        public static void SetOverseerr(MoviesSettings movieSettings, SaveOverseerrMoviesSettingsModel overseerrSettings)
        {
            SettingsFile.Write(settings =>
            {
                SetOverseerrSettings(overseerrSettings, settings);
                settings.DownloadClients.Overseerr.Movies = JToken.FromObject(overseerrSettings.Movies);
                SetMovieSettings(movieSettings, settings);
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

                settings.DownloadClients.Radarr.Categories = JToken.FromObject(radarrSettings.Categories.Select(x => new RadarrCategory
                {
                    Id = x.Id,
                    Name = x.Name,
                    MinimumAvailability = x.MinimumAvailability,
                    ProfileId = x.ProfileId,
                    RootFolder = x.RootFolder,
                    Tags = x.Tags
                }).ToArray());

                settings.DownloadClients.Radarr.SearchNewRequests = radarrSettings.SearchNewRequests;
                settings.DownloadClients.Radarr.MonitorNewRequests = radarrSettings.MonitorNewRequests;

                settings.DownloadClients.Radarr.UseSSL = radarrSettings.UseSSL;
                settings.DownloadClients.Radarr.Version = radarrSettings.Version;

                if (settings.Movies.Client != movieSettings.Client)
                {
                    NotificationsFile.ClearAllMovieNotifications();
                }

                SetMovieSettings(movieSettings, settings);
            });
        }

        public static void SetDisabledClient(TvShowsSettings tvShowsSettings)
        {
            SettingsFile.Write(settings =>
            {
                NotificationsFile.ClearAllTvShowNotifications();
                settings.TvShows.Client = tvShowsSettings.Client;
            });
        }

        public static void SetOmbi(TvShowsSettings tvShowsSettings, OmbiSettingsModel ombiSettings)
        {
            SettingsFile.Write(settings =>
            {
                SetOmbiSettings(ombiSettings, settings);
                SetTvShowSettings(tvShowsSettings, settings);
            });
        }

        public static void SetOverseerr(TvShowsSettings movieSettings, SaveOverseerrTvShowsSettingsModel overseerrSettings)
        {
            SettingsFile.Write(settings =>
            {
                SetOverseerrSettings(overseerrSettings, settings);
                settings.DownloadClients.Overseerr.TvShows = JToken.FromObject(overseerrSettings.TvShows);
                SetTvShowSettings(movieSettings, settings);
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

                settings.DownloadClients.Sonarr.Categories = JToken.FromObject(sonarrSettings.Categories.Select(x => new SonarrCategory
                {
                    Id = x.Id,
                    Name = x.Name,
                    ProfileId = x.ProfileId,
                    RootFolder = x.RootFolder,
                    Tags = x.Tags,
                    LanguageId = x.LanguageId,
                    UseSeasonFolders = x.UseSeasonFolders,
                    SeriesType = x.SeriesType
                }).ToArray());

                settings.DownloadClients.Sonarr.SearchNewRequests = sonarrSettings.SearchNewRequests;
                settings.DownloadClients.Sonarr.MonitorNewRequests = sonarrSettings.MonitorNewRequests;

                settings.DownloadClients.Sonarr.UseSSL = sonarrSettings.UseSSL;
                settings.DownloadClients.Sonarr.Version = sonarrSettings.Version;

                SetTvShowSettings(tvSettings, settings);
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

        private static void SetOverseerrSettings(OverseerrSettingsModel overseerrSettings, dynamic settings)
        {
            settings.DownloadClients.Overseerr.Hostname = overseerrSettings.Hostname;
            settings.DownloadClients.Overseerr.Port = overseerrSettings.Port;
            settings.DownloadClients.Overseerr.ApiKey = overseerrSettings.ApiKey;
            settings.DownloadClients.Overseerr.UseSSL = overseerrSettings.UseSSL;
            settings.DownloadClients.Overseerr.Version = overseerrSettings.Version;
        }

        private static void SetTvShowSettings(TvShowsSettings tvSettings, dynamic settings) 
        {
            if (settings.TvShows.Client != tvSettings.Client)
            {
                NotificationsFile.ClearAllTvShowNotifications();
            }

            settings.TvShows.Client = tvSettings.Client;
            settings.TvShows.Restrictions = tvSettings.Restrictions;
        }

        private static void SetMovieSettings(MoviesSettings movieSettings, dynamic settings)
        {
            if (settings.Movies.Client != movieSettings.Client)
            {
                NotificationsFile.ClearAllMovieNotifications();
            }

            settings.Movies.Client = movieSettings.Client;
        }
    }
}