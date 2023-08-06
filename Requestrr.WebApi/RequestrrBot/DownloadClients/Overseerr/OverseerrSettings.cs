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

        public bool UseMovieIssue { get; set; } = false;
        public bool UseTVIssue { get; set; } = false;

        public string ApiKey { get; set; } = string.Empty;
        public OverseerrMovieSettings Movies { get; set; } = new OverseerrMovieSettings();
        public OverseerrTvShowSettings TvShows { get; set; } = new OverseerrTvShowSettings();
        public string Version { get; set; } = "1";
    }

    public class OverseerrMovieSettings
    {
        public string DefaultApiUserId { get; set; }
        public OverseerrMovieCategory[] Categories { get; set; } = Array.Empty<OverseerrMovieCategory>();
    }

    public class OverseerrMovieCategory
    {
        public int Id { get; set; } = -1;
        public bool Is4K { get; set; } = false;
        public string Name { get; set; } = string.Empty;
        public int ServiceId { get; set; } = -1;
        public int ProfileId { get; set; } = -1;
        public string RootFolder { get; set; } = string.Empty;
        public int[] Tags { get; set; } = Array.Empty<int>();
    }

    public class OverseerrTvShowSettings
    {
        public string DefaultApiUserId { get; set; }
        public OverseerrTvShowCategory[] Categories { get; set; } = Array.Empty<OverseerrTvShowCategory>();
    }

    public class OverseerrTvShowCategory
    {
        public int Id { get; set; } = -1;
        public bool Is4K { get; set; } = false;
        public string Name { get; set; } = string.Empty;
        public int ServiceId { get; set; } = -1;
        public int ProfileId { get; set; } = -1;
        public int LanguageProfileId { get; set; } = -1;
        public string RootFolder { get; set; } = string.Empty;
        public int[] Tags { get; set; } = Array.Empty<int>();
    }
}