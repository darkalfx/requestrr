using System;

namespace Requestrr.WebApi.config
{
    public class DownloadClientsSettings
    {
        public OmbiSettings Ombi { get; set; }
        public OverseerrSettings Overseerr { get; set; }
        public RadarrSettings Radarr { get; set; }
        public SonarrSettings Sonarr { get; set; }
    }

    public class OmbiSettings
    {
        public string Hostname { get; set; }
        public int Port { get; set; }
        public string BaseUrl { get; set; }
        public string ApiKey { get; set; }
        public string ApiUsername { get; set; }
        public bool UseSSL { get; set; }
        public string Version { get; set; }
    }

    public class OverseerrSettings
    {
        public string Hostname { get; set; }
        public int Port { get; set; }
        public string ApiKey { get; set; }
        public string DefaultApiUserID { get; set; }
        public bool UseSSL { get; set; }
        public string Version { get; set; }
    }

    public class RadarrSettings
    {
        public string Hostname { get; set; }
        public int Port { get; set; }
        public string ApiKey { get; set; }
        public string BaseUrl { get; set; }
        public RadarrSettingsCategory[] Categories { get; set; } = Array.Empty<RadarrSettingsCategory>();
        public bool UseSSL { get; set; }
        public bool SearchNewRequests { get; set; }
        public bool MonitorNewRequests { get; set; }
        public string Version { get; set; }
    }

    public class RadarrSettingsCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ProfileId { get; set; }
        public string RootFolder { get; set; }
        public string MinimumAvailability { get; set; }
        public int[] Tags { get; set; } = Array.Empty<int>();
    }

    public class SonarrSettings
    {
        public string Hostname { get; set; }
        public int Port { get; set; }
        public string ApiKey { get; set; }
        public string BaseUrl { get; set; }
        public SonarrSettingsCategory[] Categories { get; set; } = Array.Empty<SonarrSettingsCategory>();
        public bool UseSSL { get; set; }
        public bool SearchNewRequests { get; set; }
        public bool MonitorNewRequests { get; set; }
        public string SeriesType { get; set; }
        public string Version { get; set; }
    }

    public class SonarrSettingsCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ProfileId { get; set; }
        public string RootFolder { get; set; }
        public int[] Tags { get; set; } = Array.Empty<int>();
        public int LanguageId { get; set; }
        public bool UseSeasonFolders { get; set; } = true;
        public string SeriesType { get; set; }
    }
}