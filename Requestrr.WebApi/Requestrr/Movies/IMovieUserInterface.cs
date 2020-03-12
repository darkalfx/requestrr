using System.Collections.Generic;
using System.Threading.Tasks;

namespace Requestrr.WebApi.Requestrr.Movies
{
    public interface IMovieUserInterface
    {
        Task WarnNoMovieFoundAsync(string movieName);
        Task<MovieSelection> GetMovieSelectionAsync(IReadOnlyList<Movie> movies);
        Task WarnInvalidMovieSelectionAsync();
        Task<bool> GetMovieRequestAsync();
        Task DisplayMovieDetails(Movie movie);
        Task WarnMovieAlreadyAvailable();
        Task DisplayRequestSuccess(Movie movie);
        Task<bool> AskForNotificationRequestAsync();
        Task DisplayNotificationSuccessAsync(Movie movie);
        Task WarnMovieUnavailableAndAlreadyHasNotification();
    }
    
    public class MovieSelection
    {
        public Movie Movie { get; set; }
        public bool IsCancelled { get; set; }
    }
}