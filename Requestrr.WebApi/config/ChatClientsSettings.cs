namespace Requestrr.WebApi.Config
{
    public class ChatClientsSettings
    {
        public DiscordSettings Discord { get; set; }
    }

    public class DiscordSettings
    {
        public string BotToken { get; set; }
        public string ClientId { get; set; }
        public string StatusMessage { get; set; }
        public bool EnableDirectMessageSupport { get; set; }
    }
}