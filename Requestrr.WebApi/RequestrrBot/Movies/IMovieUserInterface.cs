using System.Collections.Generic;
using System.Threading.Tasks;

namespace Requestrr.WebApi.RequestrrBot.Movies
{
    public interface IMovieUserInterface
    {
        Task WarnNoMovieFoundAsync(string movieName);
        Task<MovieSelection> GetMovieSelectionAsync(IReadOnlyList<Movie> movies);
        Task WarnInvalidMovieSelectionAsync();
        Task<bool> GetMovieRequestAsync(Movie movie);
        Task DisplayMovieDetails(Movie movie);
        Task WarnMovieAlreadyAvailable();
        Task DisplayRequestSuccess(Movie movie);
        Task<bool> AskForNotificationRequestAsync();
        Task DisplayNotificationSuccessAsync(Movie movie);
        Task WarnMovieUnavailableAndAlreadyHasNotification();
        Task DisplayRequestDenied(Movie movie);
    }
    
    public class MovieSelection
    {
        public Movie Movie { get; set; }
        public bool IsCancelled { get; set; }
    }
}