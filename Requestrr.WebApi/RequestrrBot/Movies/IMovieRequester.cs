using System.Threading.Tasks;

namespace Requestrr.WebApi.RequestrrBot.Movies
{
    public interface IMovieRequester
    {
        Task<MovieRequestResult> RequestMovieAsync(MovieRequest request, Movie movie);
    }

    public class MovieRequestResult
    {
        public bool WasDenied { get; set; }
    }
}