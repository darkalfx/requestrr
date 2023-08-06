using System.Collections.Generic;
using System.Threading.Tasks;

namespace Requestrr.WebApi.RequestrrBot.Movies
{
    public interface IMovieIssueSearcher
    {
        /// <summary>
        /// Used to check interal library
        /// </summary>
        /// <param name="request">Movie request</param>
        /// <param name="movieName">Name of the movie to look for</param>
        /// <returns>Returns the list of movies matching the name in library</returns>
        Task<IReadOnlyList<Movie>> SearchMovieLibraryAsync(MovieRequest request, string movieName);

        /// <summary>
        /// Used to check interal library with Movie DB ID
        /// </summary>
        /// <param name="request"></param>
        /// <param name="theMovieDbId"></param>
        /// <returns></returns>
        Task<Movie> SearchMovieLibraryAsync(MovieRequest request, int theMovieDbId);

        List<string> IssueTypes { get; }
    }
}
