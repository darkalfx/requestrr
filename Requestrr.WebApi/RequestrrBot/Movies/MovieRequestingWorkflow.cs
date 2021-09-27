using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Requestrr.WebApi.RequestrrBot.Movies
{
    public class MovieRequestingWorkflow
    {
        private readonly MovieUserRequester _user;
        private readonly IMovieSearcher _searcher;
        private readonly IMovieRequester _requester;
        private readonly IMovieUserInterface _userInterface;
        private readonly IMovieNotificationWorkflow _notificationWorkflow;

        public MovieRequestingWorkflow(
            MovieUserRequester user,
            IMovieSearcher searcher,
            IMovieRequester requester,
            IMovieUserInterface userInterface,
            IMovieNotificationWorkflow movieNotificationWorkflow)
        {
            _user = user;
            _searcher = searcher;
            _requester = requester;
            _userInterface = userInterface;
            _notificationWorkflow = movieNotificationWorkflow;
        }

        public async Task SearchMovieAsync(string movieName)
        {
            var movies = await SearchMoviesAsync(movieName);

            if (movies.Any())
            {
                if (movies.Count > 1)
                {
                    await _userInterface.ShowMovieSelection(movies);
                }
                else if (movies.Count == 1)
                {
                    var movie = movies.Single();
                    await HandleMovieSelectionAsync(movie);
                }
            }
        }

        public async Task SearchMovieAsync(int theMovieDbId)
        {
            try
            {
                var movie = await _searcher.SearchMovieAsync(theMovieDbId);
                await HandleMovieSelectionAsync(movie);
            }
            catch
            {
                await _userInterface.WarnNoMovieFoundByTheMovieDbIdAsync(theMovieDbId.ToString());
            }
        }

        private async Task<IReadOnlyList<Movie>> SearchMoviesAsync(string movieName)
        {
            IReadOnlyList<Movie> movies = Array.Empty<Movie>();

            movieName = movieName.Replace(".", " ");
            movies = await _searcher.SearchMovieAsync(movieName);

            if (!movies.Any())
            {
                await _userInterface.WarnNoMovieFoundAsync(movieName);
            }

            return movies;
        }

        public async Task HandleMovieSelectionAsync(int theMovieDbId)
        {
            await HandleMovieSelectionAsync(await _searcher.SearchMovieAsync(theMovieDbId));
        }

        private async Task HandleMovieSelectionAsync(Movie movie)
        {
            if (CanBeRequested(movie))
            {
                await _userInterface.DisplayMovieDetailsAsync(movie);
            }
            else
            {
                if (movie.Available)
                {
                    await _userInterface.WarnMovieAlreadyAvailableAsync(movie);
                }
                else
                {
                    await _notificationWorkflow.NotifyForExistingRequestAsync(_user.UserId, movie);
                }
            }
        }

        public async Task RequestMovieAsync(int theMovieDbId)
        {
            var movie = await _searcher.SearchMovieAsync(theMovieDbId);
            var result = await _requester.RequestMovieAsync(_user, movie);

            if (result.WasDenied)
            {
                await _userInterface.DisplayRequestDeniedAsync(movie);
            }
            else
            {
                await _userInterface.DisplayRequestSuccessAsync(movie);
                await _notificationWorkflow.NotifyForNewRequestAsync(_user.UserId, movie);
            }
        }

        private static bool CanBeRequested(Movie movie)
        {
            return !movie.Available && !movie.Requested;
        }
    }
}