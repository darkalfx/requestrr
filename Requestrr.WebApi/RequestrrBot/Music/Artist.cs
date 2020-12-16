using System.Linq;

namespace Requestrr.WebApi.RequestrrBot.Music
{
    public class Artist
    {
        public string MbId { get; set; }
        public string DownloadClientId { get; set; }
        public string Name { get; set; }
        public string Disambiguation { get; set; }
        public string Quality { get; set; }
        public bool HasEnded { get; set; }
        public string PlexUrl { get; set; }
        public string EmbyUrl { get; set; }
        public string Overview { get; set; }
        public string Banner { get; set; }
        public string Status { get; set; }
        public bool IsRequested { get; set; }
    }
}