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
        public string[] MusicRoles { get; set; }
        public string CommandPrefix { get; set; }
        public string MovieDownloadClient { get; set; }
        public string MovieCommand { get; set; }
        public int MovieDownloadClientConfigurationHash { get; set; }
        public string TvShowDownloadClient { get; set; }
        public string TvShowCommand { get; set; }
        public int TvShowDownloadClientConfigurationHash { get; set; }
        public string MusicDownloadClient { get; set; }
        public string MusicCommand { get; set; }
        public int MusicDownloadClientConfigurationHash { get; set; }
        public bool EnableRequestsThroughDirectMessages { get; set; }
        public bool AutomaticallyNotifyRequesters { get; set; }
        public string NotificationMode { get; set; }
        public string[] NotificationChannels { get; set; }
        public bool AutomaticallyPurgeCommandMessages { get; set; }
        public bool DisplayHelpCommandInDMs { get; set; }

        public override bool Equals(object obj)
        {
            return obj is DiscordSettings settings &&
                   BotToken == settings.BotToken &&
                   ClientID == settings.ClientID &&
                   StatusMessage == settings.StatusMessage &&
                   MonitoredChannels.SequenceEqual(settings.MonitoredChannels) &&
                   TvShowRoles.SequenceEqual(settings.TvShowRoles) &&
                   MovieRoles.SequenceEqual(settings.MovieRoles) &&
                   MusicRoles.SequenceEqual(settings.MusicRoles) &&
                   CommandPrefix == settings.CommandPrefix &&
                   MovieDownloadClient == settings.MovieDownloadClient &&
                   MovieCommand == settings.MovieCommand &&
                   MovieDownloadClientConfigurationHash == settings.MovieDownloadClientConfigurationHash &&
                   TvShowDownloadClient == settings.TvShowDownloadClient &&
                   TvShowCommand == settings.TvShowCommand &&
                   TvShowDownloadClientConfigurationHash == settings.TvShowDownloadClientConfigurationHash &&
                   MusicDownloadClient == settings.MusicDownloadClient &&
                   MusicCommand == settings.MusicCommand &&
                   MusicDownloadClientConfigurationHash == settings.MusicDownloadClientConfigurationHash &&
                   EnableRequestsThroughDirectMessages == settings.EnableRequestsThroughDirectMessages &&
                   AutomaticallyNotifyRequesters == settings.AutomaticallyNotifyRequesters &&
                   NotificationMode == settings.NotificationMode &&
                   NotificationChannels.SequenceEqual(settings.NotificationChannels) &&
                   AutomaticallyPurgeCommandMessages == settings.AutomaticallyPurgeCommandMessages &&
                   DisplayHelpCommandInDMs == settings.DisplayHelpCommandInDMs;
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
            hash.Add(MusicRoles);
            hash.Add(CommandPrefix);
            hash.Add(MovieDownloadClient);
            hash.Add(MovieCommand);
            hash.Add(MovieDownloadClientConfigurationHash);
            hash.Add(TvShowDownloadClient);
            hash.Add(TvShowCommand);
            hash.Add(TvShowDownloadClientConfigurationHash);
            hash.Add(MusicDownloadClient);
            hash.Add(MusicCommand);
            hash.Add(MusicDownloadClientConfigurationHash);
            hash.Add(EnableRequestsThroughDirectMessages);
            hash.Add(AutomaticallyNotifyRequesters);
            hash.Add(NotificationMode);
            hash.Add(NotificationChannels);
            hash.Add(AutomaticallyPurgeCommandMessages);
            hash.Add(DisplayHelpCommandInDMs);
            return hash.ToHashCode();
        }
    }
}