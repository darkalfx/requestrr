using System;

namespace Requestrr.WebApi.config
{
    public class DownloadClientsSettings
    {
        public DownloadClient[] DownloadClients { get; set; } = Array.Empty<DownloadClient>();
    }

    public abstract class DownloadClient
    {
        public const string Enabled = "Enabled";
        public const string Disabled = "Disabled";
        public const string Sonarr = "Sonarr";
        public const string Radarr = "Radarr";
        public const string Ombi = "Ombi";
        public const string Overseerr = "Overseerr";

        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class RadarrDownloadClient : DownloadClient
    {
        public string Hostname { get; set; }
        public int Port { get; set; }
        public string ApiKey { get; set; }
        public string BaseUrl { get; set; }
        public bool SearchNewRequests { get; set; }
        public bool MonitorNewRequests { get; set; }
        public bool UseSSL { get; set; }
        public string Version { get; set; }
    }

    public class SonarrDownloadClient : DownloadClient
    {
        public string Hostname { get; set; }
        public int Port { get; set; }
        public string ApiKey { get; set; }
        public string BaseUrl { get; set; }
        public bool SearchNewRequests { get; set; }
        public bool MonitorNewRequests { get; set; }
        public bool UseSSL { get; set; }
        public string Version { get; set; }
    }

    public class OverseerrDownloadClient : DownloadClient
    {
        public string Hostname { get; set; } = string.Empty;
        public int Port { get; set; } = 5055;
        public bool UseSSL { get; set; } = false;
        public string ApiKey { get; set; } = string.Empty;
        public string Version { get; set; } = "1";
    }

    public class OmbiDownloadClient : DownloadClient
    {
        public string Hostname { get; set; }
        public int Port { get; set; }
        public bool UseSSL { get; set; }
        public string ApiKey { get; set; }
        public string BaseUrl { get; set; }
        public string Version { get; set; }
    }
}