using System.ComponentModel.DataAnnotations;

namespace Requestrr.WebApi.Controllers
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

        public string MonitoredChannels { get; set; }

        public bool EnableDirectMessageSupport { get; set; }
    }
}
