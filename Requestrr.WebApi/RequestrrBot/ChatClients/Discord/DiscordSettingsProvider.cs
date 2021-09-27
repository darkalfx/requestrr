using System;
using Requestrr.WebApi.RequestrrBot.DownloadClients;

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
                hash.Add((string)settings.DownloadClients.Radarr.Hostname);
                hash.Add((int)settings.DownloadClients.Radarr.Port);
                hash.Add((string)settings.DownloadClients.Radarr.ApiKey);
                hash.Add((bool)settings.DownloadClients.Radarr.UseSSL);
                hash.Add((string)settings.DownloadClients.Radarr.Version);
            }
            else if (settings.Movies.Client == DownloadClient.Ombi)
            {
                hash.Add((string)settings.DownloadClients.Ombi.ApiKey);
                hash.Add((string)settings.DownloadClients.Ombi.Hostname);
                hash.Add((int)settings.DownloadClients.Ombi.Port);
                hash.Add((bool)settings.DownloadClients.Ombi.UseSSL);
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
                hash.Add((string)settings.DownloadClients.Sonarr.Hostname);
                hash.Add((int)settings.DownloadClients.Sonarr.Port);
                hash.Add((string)settings.DownloadClients.Sonarr.ApiKey);
                hash.Add((bool)settings.DownloadClients.Sonarr.UseSSL);
                hash.Add((string)settings.DownloadClients.Sonarr.Version);
            }
            else if (settings.TvShows.Client == DownloadClient.Ombi)
            {
                hash.Add((string)settings.DownloadClients.Ombi.ApiKey);
                hash.Add((string)settings.DownloadClients.Ombi.Hostname);
                hash.Add((int)settings.DownloadClients.Ombi.Port);
                hash.Add((bool)settings.DownloadClients.Ombi.UseSSL);
            }
            else
            {
                hash.Add(DownloadClient.Disabled);
            }

            return hash.ToHashCode();
        }
    }
}