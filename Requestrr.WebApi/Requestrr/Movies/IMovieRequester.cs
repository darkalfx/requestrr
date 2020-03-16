using System.Threading.Tasks;

namespace Requestrr.WebApi.Requestrr.Movies
{
    public interface IMovieRequester
    {
        Task<MovieRequestResult> RequestMovieAsync(MovieUserRequester requester, Movie movie);
    }

    public class MovieRequestResult
    {
        public bool WasDenied { get; set; }
    }
}