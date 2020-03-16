using System;
using System.Linq;

namespace Requestrr.WebApi.RequestrrBot.ChatClients.Discord
{
    public class DiscordSettings
    {
        public string BotToken { get; set; }
        public string ClientID { get; set; }
        public string StatusMessage { get; set; }
        public string[] MonitoredChannels { get; set; }
        public string[] TvShowRoles { get; set; }
        public string[] MovieRoles { get; set; }
        public string CommandPrefix { get; set; }
        public string MovieDownloadClient { get; set; }
        public string MovieCommand { get; set; }
        public int MovieDownloadClientConfigurationHash { get; set; }
        public string TvShowDownloadClient { get; set; }
        public string TvShowCommand { get; set; }
        public int TvShowDownloadClientConfigurationHash { get; set; }
        public bool EnableDirectMessageSupport { get; set; }

        public override bool Equals(object obj)
        {
            return obj is DiscordSettings settings &&
                   BotToken == settings.BotToken &&
                   ClientID == settings.ClientID &&
                   StatusMessage == settings.StatusMessage &&
                   MonitoredChannels.SequenceEqual(settings.MonitoredChannels) &&
                   TvShowRoles.SequenceEqual(settings.TvShowRoles) &&
                   MovieRoles.SequenceEqual(settings.MovieRoles) &&
                   CommandPrefix == settings.CommandPrefix &&
                   MovieDownloadClient == settings.MovieDownloadClient &&
                   MovieCommand == settings.MovieCommand &&
                   MovieDownloadClientConfigurationHash == settings.MovieDownloadClientConfigurationHash &&
                   TvShowDownloadClient == settings.TvShowDownloadClient &&
                   TvShowCommand == settings.TvShowCommand &&
                   TvShowDownloadClientConfigurationHash == settings.TvShowDownloadClientConfigurationHash &&
                   EnableDirectMessageSupport == settings.EnableDirectMessageSupport;
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(BotToken);
            hash.Add(ClientID);
            hash.Add(StatusMessage);
            hash.Add(MonitoredChannels);
            hash.Add(MovieRoles);
            hash.Add(TvShowRoles);
            hash.Add(CommandPrefix);
            hash.Add(MovieDownloadClient);
            hash.Add(MovieCommand);
            hash.Add(MovieDownloadClientConfigurationHash);
            hash.Add(TvShowDownloadClient);
            hash.Add(TvShowCommand);
            hash.Add(TvShowDownloadClientConfigurationHash);
            hash.Add(EnableDirectMessageSupport);
            return hash.ToHashCode();
        }
    }
}