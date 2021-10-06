using System;

namespace Requestrr.WebApi.RequestrrBot.DownloadClients.Radarr
{
    public class RadarrSettings
    {
        public string Hostname { get; set; }
        public int Port { get; set; }
        public string ApiKey { get; set; }
        public string BaseUrl { get; set; }
        public RadarrCategory[] Categories { get; set; } = Array.Empty<RadarrCategory>();
        public bool SearchNewRequests { get; set; }
        public bool MonitorNewRequests { get; set; }
        public bool UseSSL { get; set; }
        public string Version { get; set; }
    }

    public class RadarrCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ProfileId { get; set; }
        public string RootFolder { get; set; }
        public string MinimumAvailability { get; set; }
        public int[] Tags { get; set; } = Array.Empty<int>();
    }
}