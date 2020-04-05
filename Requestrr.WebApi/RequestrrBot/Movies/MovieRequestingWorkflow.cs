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

        public async Task RequestMovieAsync(string movieName)
        {
            var movies = await SearchMoviesAsync(movieName);

            if (movies.Any())
            {
                if (movies.Count > 1)
                {
                    var movieSelection = await _userInterface.GetMovieSelectionAsync(movies);

                    if (!movieSelection.IsCancelled && movieSelection.Movie != null)
                    {
                        var movie = movieSelection.Movie;
                        await HandleMovieSelectionAsync(movie);
                    }
                    else if (!movieSelection.IsCancelled)
                    {
                        await _userInterface.WarnInvalidMovieSelectionAsync();
                    }
                }
                else if (movies.Count == 1)
                {
                    var movie = movies.Single();
                    await HandleMovieSelectionAsync(movie);
                }
            }
        }

        public async Task<IReadOnlyList<Movie>> SearchMoviesAsync(string movieName)
        {
            IReadOnlyList<Movie> movies = Array.Empty<Movie>();

            if (movieName.Trim().ToLower().StartsWith("tmdb"))
            {
                var theMovieDbIdTextValue = movieName.ToLower().Split("tmdb")[1]?.Trim();

                if (int.TryParse(theMovieDbIdTextValue, out var theMovieDbId))
                {
                    try
                    {
                        var movie = await _searcher.SearchMovieAsync(theMovieDbId);
                        movies = new List<Movie> { movie }.Where(x => x != null).ToArray();
                    }
                    catch
                    {
                        movies = new List<Movie>();
                    }

                    if (!movies.Any())
                    {
                        await _userInterface.WarnNoMovieFoundByTheMovieDbIdAsync(theMovieDbIdTextValue);
                    }
                }
                else
                {
                    await _userInterface.WarnNoMovieFoundByTheMovieDbIdAsync(theMovieDbIdTextValue);
                }
            }
            else
            {
                movieName = movieName.Replace(".", " ");
                movies = await _searcher.SearchMovieAsync(movieName);

                if (!movies.Any())
                {
                    await _userInterface.WarnNoMovieFoundAsync(movieName);
                }
            }

            return movies;
        }

        private async Task HandleMovieSelectionAsync(Movie movie)
        {
            await _userInterface.DisplayMovieDetailsAsync(movie);

            if (CanBeRequested(movie))
            {
                var wasRequested = await _userInterface.GetMovieRequestAsync(movie);

                if (wasRequested)
                {
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
            }
            else
            {
                if (movie.Available)
                {
                    await _userInterface.WarnMovieAlreadyAvailableAsync();
                }
                else
                {
                    await _notificationWorkflow.NotifyForExistingRequestAsync(_user.UserId, movie);
                }
            }
        }

        private static bool CanBeRequested(Movie movie)
        {
            return !movie.Available && !movie.Requested;
        }
    }
}