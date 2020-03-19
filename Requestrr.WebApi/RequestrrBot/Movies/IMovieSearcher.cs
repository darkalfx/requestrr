using System.Collections.Generic;
using System.Threading.Tasks;

namespace Requestrr.WebApi.RequestrrBot.Movies
{
    public interface IMovieSearcher
    {
        Task<IReadOnlyList<Movie>> SearchMovieAsync(string movieName);
        Task<MovieDetails> GetMovieDetails(string theMovieDbId);
        Task<Dictionary<int, Movie>> SearchAvailableMoviesAsync(HashSet<int> movies, System.Threading.CancellationToken token);
        Task<Movie> SearchMovieAsync(int theMovieDbId);
    }

    public class MovieDetails
    {
        public string InTheatersDate { get; set; }
        public string PhysicalReleaseName { get; set; }
        public string PhysicalReleaseDate { get; set; }
    }
}