using System.ComponentModel.DataAnnotations;

namespace Requestrr.WebApi.Controllers.ChatClients
{
    public class ChatClientsSettingsModel
    {
        [Required]
        public string Client { get; set; }

        [Required]
        public string ClientId { get; set; }

        [Required]
        public string BotToken { get; set; }

        public string StatusMessage { get; set; }

        public string CommandPrefix { get; set; }

        public string[] MonitoredChannels { get; set; }

        public string[] TvShowRoles { get; set; }

        public string[] MovieRoles { get; set; }

        public bool EnableRequestsThroughDirectMessages { get; set; }

        public bool AutomaticallyNotifyRequesters { get; set; }

        public string NotificationMode { get; set; }

        public string[] NotificationChannels { get; set; }

        public bool AutomaticallyPurgeCommandMessages { get; set; }

        public bool DisplayHelpCommandInDMs { get; set; }
    }

    public class ChatClientTestSettingsModel
    {
        [Required]
        public string BotToken { get; set; }
    }
}
