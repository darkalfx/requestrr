namespace Requestrr.WebApi.RequestrrBot.DownloadClients.Overseerr
{
    public class OverseerrTestSettings
    {
        public string Hostname { get; set; } = string.Empty;
        public int Port { get; set; } = 5055;
        public bool UseSSL { get; set; } = false;
        public string ApiKey { get; set; } = string.Empty;
        public string DefaultApiUserId { get; set; }
        public string Version { get; set; } = "1";
    }
}