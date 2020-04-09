using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Requestrr.WebApi.RequestrrBot.Movies
{
    public class WebHookMovieUserInterface : IMovieUserInterface
    {
        private readonly IMovieUserInterface _movieUserInterface;

        public WebHookMovieUserInterface(IMovieUserInterface movieUserInterface)
        {
            _movieUserInterface = movieUserInterface;
        }

        public Task<bool> AskForNotificationRequestAsync()
        {
            return _movieUserInterface.AskForNotificationRequestAsync();
        }

        public Task DisplayMovieDetailsAsync(Movie movie)
        {
            return _movieUserInterface.DisplayMovieDetailsAsync(movie);
        }

        public Task DisplayNotificationSuccessAsync(Movie movie)
        {
            return _movieUserInterface.DisplayNotificationSuccessAsync(movie);
        }

        public Task DisplayRequestDeniedAsync(Movie movie)
        {
            return _movieUserInterface.DisplayRequestDeniedAsync(movie);
        }

        public Task DisplayRequestSuccessAsync(Movie movie)
        {
            return _movieUserInterface.DisplayRequestSuccessAsync(movie);
        }

        public Task<bool> GetMovieRequestAsync(Movie movie)
        {
            return Task.FromResult(true);
        }

        public Task<MovieSelection> GetMovieSelectionAsync(IReadOnlyList<Movie> movies)
        {
            return Task.FromResult(new MovieSelection
            {
                Movie = movies.First(),
                IsCancelled = false,
            });
        }

        public Task WarnInvalidMovieSelectionAsync()
        {
            return _movieUserInterface.WarnInvalidMovieSelectionAsync();
        }

        public Task WarnMovieAlreadyAvailableAsync()
        {
            return _movieUserInterface.WarnMovieAlreadyAvailableAsync();
        }

        public Task WarnMovieAlreadyRequestedAsync()
        {
            return _movieUserInterface.WarnMovieAlreadyRequestedAsync();
        }

        public Task WarnMovieUnavailableAndAlreadyHasNotificationAsync()
        {
            return _movieUserInterface.WarnMovieUnavailableAndAlreadyHasNotificationAsync();
        }

        public Task WarnNoMovieFoundAsync(string movieName)
        {
            return _movieUserInterface.WarnNoMovieFoundAsync(movieName);
        }

        public Task WarnNoMovieFoundByTheMovieDbIdAsync(string theMovieDbIdTextValue)
        {
            return _movieUserInterface.WarnNoMovieFoundByTheMovieDbIdAsync(theMovieDbIdTextValue);
        }
    }
}