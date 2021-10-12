using System;
using System.Linq;
using Requestrr.WebApi.RequestrrBot.DownloadClients;
using Requestrr.WebApi.RequestrrBot.DownloadClients.Ombi;
using Requestrr.WebApi.RequestrrBot.DownloadClients.Overseerr;
using Requestrr.WebApi.RequestrrBot.DownloadClients.Radarr;
using Requestrr.WebApi.RequestrrBot.DownloadClients.Sonarr;
using Requestrr.WebApi.RequestrrBot.Extensions;

namespace Requestrr.WebApi.RequestrrBot.ChatClients.Discord
{
    public class DiscordSettingsProvider
    {
        public DiscordSettings Provide()
        {
            dynamic settings = SettingsFile.Read();

            return new DiscordSettings
            {
                BotToken = settings.ChatClients.Discord.BotToken,
                MovieDownloadClient = settings.Movies.Client,
                MovieDownloadClientConfigurationHash = ComputeMovieClientConfigurationHashCode(settings),
                TvShowDownloadClient = settings.TvShows.Client,
                TvShowDownloadClientConfigurationHash = ComputeTvClientConfigurationHashCode(settings),
                StatusMessage = settings.ChatClients.Discord.StatusMessage,
                MonitoredChannels = settings.ChatClients.Discord.MonitoredChannels.ToObject<string[]>(),
                TvShowRoles = settings.ChatClients.Discord.TvShowRoles.ToObject<string[]>(),
                MovieRoles = settings.ChatClients.Discord.MovieRoles.ToObject<string[]>(),
                ClientID = settings.ChatClients.Discord.ClientId,
                EnableRequestsThroughDirectMessages = settings.ChatClients.Discord.EnableRequestsThroughDirectMessages,
                AutomaticallyNotifyRequesters = settings.ChatClients.Discord.AutomaticallyNotifyRequesters,
                NotificationMode = settings.ChatClients.Discord.NotificationMode,
                NotificationChannels = settings.ChatClients.Discord.NotificationChannels.ToObject<string[]>(),
                AutomaticallyPurgeCommandMessages = settings.ChatClients.Discord.AutomaticallyPurgeCommandMessages,
            };
        }

        public int ComputeMovieClientConfigurationHashCode(dynamic settings)
        {
            HashCode hash = new HashCode();

            if (settings.Movies.Client == DownloadClient.Radarr)
            {
                var clientSettings = new RadarrSettingsProvider().Provide();

                hash.Add(clientSettings.Categories.Select(x => x.Name).GetSequenceHashCode());
                hash.Add(clientSettings.Hostname);
                hash.Add(clientSettings.Port);
                hash.Add(clientSettings.ApiKey);
                hash.Add(clientSettings.UseSSL);
                hash.Add(clientSettings.Version);
            }
            else if (settings.Movies.Client == DownloadClient.Ombi)
            {
                var clientSettings = new OmbiSettingsProvider().Provide();

                hash.Add(clientSettings.ApiKey);
                hash.Add(clientSettings.Hostname);
                hash.Add(clientSettings.Port);
                hash.Add(clientSettings.UseSSL);
                hash.Add(clientSettings.Version);
            }
            else if (settings.Movies.Client == DownloadClient.Overseerr)
            {
                var clientSettings = new OverseerrSettingsProvider().Provide();

                hash.Add(clientSettings.Movies.Categories.Select(x => x.Name).GetSequenceHashCode());
                hash.Add(clientSettings.ApiKey);
                hash.Add(clientSettings.Hostname);
                hash.Add(clientSettings.Port);
                hash.Add(clientSettings.UseSSL);
                hash.Add(clientSettings.Version);
            }
            else
            {
                hash.Add(DownloadClient.Disabled);
            }

            return hash.ToHashCode();
        }

        public int ComputeTvClientConfigurationHashCode(dynamic settings)
        {
            HashCode hash = new HashCode();

            if (settings.TvShows.Client == DownloadClient.Sonarr)
            {
                var clientSettings = new SonarrSettingsProvider().Provide();

                hash.Add(clientSettings.Categories.Select(x => x.Name).GetSequenceHashCode());
                hash.Add(clientSettings.Hostname);
                hash.Add(clientSettings.Port);
                hash.Add(clientSettings.ApiKey);
                hash.Add(clientSettings.UseSSL);
                hash.Add(clientSettings.Version);
            }
            else if (settings.TvShows.Client == DownloadClient.Ombi)
            {
                var clientSettings = new OmbiSettingsProvider().Provide();

                hash.Add(clientSettings.ApiKey);
                hash.Add(clientSettings.Hostname);
                hash.Add(clientSettings.Port);
                hash.Add(clientSettings.UseSSL);
                hash.Add(clientSettings.Version);
            }
            else if (settings.TvShows.Client == DownloadClient.Overseerr)
            {
                var clientSettings = new OverseerrSettingsProvider().Provide();

                hash.Add(clientSettings.TvShows.Categories.Select(x => x.Name).GetSequenceHashCode());
                hash.Add(clientSettings.ApiKey);
                hash.Add(clientSettings.Hostname);
                hash.Add(clientSettings.Port);
                hash.Add(clientSettings.UseSSL);
                hash.Add(clientSettings.Version);
            }
            else
            {
                hash.Add(DownloadClient.Disabled);
            }

            return hash.ToHashCode();
        }
    }
}