using System.Collections.Generic;
using System.Threading.Tasks;

namespace Requestrr.WebApi.Requestrr.Movies
{
    public interface IMovieSearcher
    {
        Task<IReadOnlyList<Movie>> SearchMovieAsync(string movieName);

        Task<Dictionary<int, Movie>> SearchAvailableMoviesAsync(HashSet<int> movies, System.Threading.CancellationToken token);
    }
}