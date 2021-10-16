using System;

namespace Requestrr.WebApi.RequestrrBot.DownloadClients.Overseerr
{
    public class RadarrServiceSettings
    {
        public RadarrService[] RadarrServices { get; set; } = Array.Empty<RadarrService>();
    }

    public class RadarrService
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ServiceOption[] Profiles { get; set; } = Array.Empty<ServiceOption>();
        public ServiceOption[] RootPaths { get; set; } = Array.Empty<ServiceOption>();
        public ServiceOption[] Tags { get; set; } = Array.Empty<ServiceOption>();
    }

    public class SonarrServiceSettings
    {
        public SonarrService[] SonarrServices { get; set; } = Array.Empty<SonarrService>();
    }

    public class SonarrService
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ServiceOption[] Profiles { get; set; } = Array.Empty<ServiceOption>();
        public ServiceOption[] LanguageProfiles { get; set; } = Array.Empty<ServiceOption>();
        public ServiceOption[] RootPaths { get; set; } = Array.Empty<ServiceOption>();
        public ServiceOption[] Tags { get; set; } = Array.Empty<ServiceOption>();
    }

    public class ServiceOption
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class OverseerrSettings
    {
        public string Hostname { get; set; } = string.Empty;
        public int Port { get; set; } = 5055;
        public bool UseSSL { get; set; } = false;
        public string ApiKey { get; set; } = string.Empty;
        public string Version { get; set; } = "1";
    }

    public class OverseerrMovieCategory : Category
    {
        public string DefaultApiUserId { get; set; }
        public bool Is4K { get; set; } = false;
        public int ServiceId { get; set; } = -1;
        public int ProfileId { get; set; } = -1;
        public string RootFolder { get; set; } = string.Empty;
        public int[] Tags { get; set; } = Array.Empty<int>();
    }

    public class OverseerrTvShowCategory : Category
    {
        public string DefaultApiUserId { get; set; }
        public bool Is4K { get; set; } = false;
        public int ServiceId { get; set; } = -1;
        public int ProfileId { get; set; } = -1;
        public int LanguageProfileId { get; set; } = -1;
        public string RootFolder { get; set; } = string.Empty;
        public int[] Tags { get; set; } = Array.Empty<int>();
    }
}