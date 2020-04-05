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
        Task DisplayMovieDetailsAsync(Movie movie);
        Task WarnMovieAlreadyAvailableAsync();
        Task DisplayRequestSuccessAsync(Movie movie);
        Task<bool> AskForNotificationRequestAsync();
        Task DisplayNotificationSuccessAsync(Movie movie);
        Task WarnMovieUnavailableAndAlreadyHasNotificationAsync();
        Task DisplayRequestDeniedAsync(Movie movie);
        Task WarnNoMovieFoundByTheMovieDbIdAsync(string theMovieDbIdTextValue);
        Task WarnMovieAlreadyRequestedAsync();
    }
    
    public class MovieSelection
    {
        public Movie Movie { get; set; }
        public bool IsCancelled { get; set; }
    }
}