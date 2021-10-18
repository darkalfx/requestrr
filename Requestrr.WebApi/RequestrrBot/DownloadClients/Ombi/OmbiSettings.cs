using Requestrr.WebApi.config;

namespace Requestrr.WebApi.RequestrrBot.DownloadClients.Ombi
{
    public class OmbiDownloadClientSettings : DownloadClientSettings
    {
        public string Hostname { get; set; }
        public int Port { get; set; }
        public bool UseSSL { get; set; }
        public string ApiKey { get; set; }
        public string BaseUrl { get; set; }
        public string Version { get; set; }
    }

    public class OmbiMovieCategory : Category
    {
        public bool UseOmbiSettings { get; set; }
        public string ApiUsername { get; set; }
    }

    public class OmbiTvShowCategory : Category
    {
        public bool UseOmbiSettings { get; set; }
        public string ApiUsername { get; set; }
    }
}