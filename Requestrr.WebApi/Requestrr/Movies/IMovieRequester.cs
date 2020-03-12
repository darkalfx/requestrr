using System.Threading.Tasks;

namespace Requestrr.WebApi.Requestrr.Movies
{
    public interface IMovieRequester
    {
        Task RequestMovieAsync(string userName, Movie movie);
    }
}