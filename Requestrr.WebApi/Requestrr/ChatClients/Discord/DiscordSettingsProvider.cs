using System;
using System.Linq;
using Requestrr.WebApi.Requestrr.DownloadClients;

namespace Requestrr.WebApi.Requestrr.ChatClients
{
    public class DiscordSettingsProvider
    {
        public DiscordSettings Provide()
        {
            dynamic settings = SettingsFile.Read();
            string monitoredChannels = settings.BotClient.MonitoredChannels;

            return new DiscordSettings
            {
                BotToken = settings.ChatClients.Discord.BotToken,
                CommandPrefix = settings.BotClient.CommandPrefix,
                MovieDownloadClient = settings.Movies.Client,
                MovieCommand = settings.Movies.Command,
                MovieDownloadClientConfigurationHash = ComputeMovieClientConfigurationHashCode(settings),
                TvShowDownloadClient = settings.TvShows.Client,
                TvShowCommand = settings.TvShows.Command,
                TvShowDownloadClientConfigurationHash = ComputeTvClientConfigurationHashCode(settings),
                StatusMessage = settings.ChatClients.Discord.StatusMessage,
                MonitoredChannels = monitoredChannels.Split(" ").Where(s => !string.IsNullOrWhiteSpace(s)).ToArray(),
                ClientID = settings.ChatClients.Discord.ClientId,
                EnableDirectMessageSupport = settings.ChatClients.Discord.EnableDirectMessageSupport
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