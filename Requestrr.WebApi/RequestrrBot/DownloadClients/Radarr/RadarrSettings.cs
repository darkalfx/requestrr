using System;
using Requestrr.WebApi.config;

namespace Requestrr.WebApi.RequestrrBot.DownloadClients.Radarr
{
    public class RadarrDownloadClientSettings : DownloadClientSettings
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
    
    public class RadarrCategory : Category
    {
        public int ProfileId { get; set; }
        public string RootFolder { get; set; }
        public string MinimumAvailability { get; set; }
        public int[] Tags { get; set; } = Array.Empty<int>();
    }
}