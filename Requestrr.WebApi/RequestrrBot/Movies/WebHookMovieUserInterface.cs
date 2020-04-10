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
            return Task.FromResult(false);
        }

        public Task DisplayMovieDetailsAsync(Movie movie)
        {
            return Task.CompletedTask;
        }

        public Task DisplayNotificationSuccessAsync(Movie movie)
        {
            return Task.CompletedTask;
        }

        public Task DisplayRequestDeniedAsync(Movie movie)
        {
            return Task.CompletedTask;
        }

        public async Task DisplayRequestSuccessAsync(Movie movie)
        {
            await _movieUserInterface.DisplayMovieDetailsAsync(movie);
            await _movieUserInterface.DisplayRequestSuccessAsync(movie);
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
            return Task.CompletedTask;
        }

        public Task WarnMovieAlreadyAvailableAsync()
        {
            return Task.CompletedTask;
        }

        public Task WarnMovieAlreadyRequestedAsync()
        {
            return Task.CompletedTask;
        }

        public Task WarnMovieUnavailableAndAlreadyHasNotificationAsync()
        {
            return Task.CompletedTask;
        }

        public Task WarnNoMovieFoundAsync(string movieName)
        {
            return Task.CompletedTask;
        }

        public Task WarnNoMovieFoundByTheMovieDbIdAsync(string theMovieDbIdTextValue)
        {
            return Task.CompletedTask;
        }
    }
}