using System;
using Requestrr.WebApi.RequestrrBot.DownloadClients;

namespace Requestrr.WebApi.RequestrrBot.Movies
{
    public class MoviesSettings
    {
        public string Client { get; set; }
        public Category[] Categories { get; set; } = Array.Empty<Category>();
    }
}