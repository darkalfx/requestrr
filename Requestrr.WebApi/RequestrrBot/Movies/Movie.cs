namespace Requestrr.WebApi.RequestrrBot.Movies
{
    public class Movie
    {
        public string DownloadClientId { get; set; }
        public string TheMovieDbId { get; set; }
        public string Title { get; set; }
        public bool Available { get; set; }
        public string Quality { get; set; }
        public bool Requested { get; set; }
        public bool Approved { get; set; }
        public string PlexUrl { get; set; }
        public string EmbyUrl { get; set; }
        public string Overview { get; set; }
        public string PosterPath { get; set; }
        public string ReleaseDate { get; set; }
    }
}