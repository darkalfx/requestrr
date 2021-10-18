using System;
using Requestrr.WebApi.config;

namespace Requestrr.WebApi.RequestrrBot.DownloadClients.Sonarr
{
    public class SonarrDownloadClientSettings : DownloadClientSettings
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

    public class SonarrCategory : Category
    {
        public int ProfileId { get; set; }
        public string RootFolder { get; set; }
        public int[] Tags { get; set; } = Array.Empty<int>();
        public int LanguageId { get; set; }
        public bool UseSeasonFolders { get; set; } = true;
        public string SeriesType { get; set; }
    }
}