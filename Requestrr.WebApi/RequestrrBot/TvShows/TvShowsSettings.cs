using System;
using Requestrr.WebApi.RequestrrBot.DownloadClients;

namespace Requestrr.WebApi.RequestrrBot.TvShows
{
    public class TvShowsSettings
    {
        public string Client { get; set; }
        public string Restrictions { get; set; }
        public Category[] Categories { get; set; } = Array.Empty<Category>();
    }
}