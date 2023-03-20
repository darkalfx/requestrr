using Requestrr.WebApi.RequestrrBot.DownloadClients.Overseerr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Requestrr.WebApi.RequestrrBot.Movies
{
    public class MovieRequestingWorkflow
    {
        private readonly int _categoryId;
        private readonly MovieUserRequester _user;
        private readonly IMovieSearcher _searcher;
        private readonly IMovieRequester _requester;
        private readonly IMovieUserInterface _userInterface;
        private readonly IMovieNotificationWorkflow _notificationWorkflow;

        public MovieRequestingWorkflow(
            MovieUserRequester user,
            int categoryId,
            IMovieSearcher searcher,
            IMovieRequester requester,
            IMovieUserInterface userInterface,
            IMovieNotificationWorkflow movieNotificationWorkflow)
        {
            _categoryId = categoryId;
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
                    await _userInterface.ShowMovieSelection(new MovieRequest(_user, _categoryId), movies);
                }
                else if (movies.Count == 1)
                {
                    var movie = movies.Single();
                    await HandleMovieSelectionAsync(movie);
                }
            }
        }


        /// <summary>
        /// Searchs for movies in the library
        /// </summary>
        /// <param name="movieName"></param>
        /// <returns></returns>
        public async Task SearchMovieLibraryAsync(string movieName)
        {
            var movies = await SearchMoviesLibraryAsync(movieName);

            if (movies.Any())
            {
                if (movies.Count > 1)
                {
                    await _userInterface.ShowMovieSelection(new MovieRequest(_user, _categoryId), movies);
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
                var movie = await _searcher.SearchMovieAsync(new MovieRequest(_user, _categoryId), theMovieDbId);
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
            movies = await _searcher.SearchMovieAsync(new MovieRequest(_user, _categoryId), movieName);

            if (!movies.Any())
            {
                await _userInterface.WarnNoMovieFoundAsync(movieName);
            }

            return movies;
        }



        /// <summary>
        /// This handles the checking of movies in library against whats in library
        /// </summary>
        /// <param name="movieName">Name of the movie to search for</param>
        /// <returns></returns>
        private async Task<IReadOnlyList<Movie>> SearchMoviesLibraryAsync(string movieName)
        {
            IReadOnlyList<Movie> movies = Array.Empty<Movie>();
            if(_searcher is not OverseerrClient)
                return movies;

            movieName = movieName.Replace(".", " ");
            movies = await ((OverseerrClient)_searcher).SearchMovieLibraryAsync(new MovieRequest(_user, _categoryId), movieName);

            if (!movies.Any())
            {
                await _userInterface.WarnNoMovieFoundAsync(movieName);
            }

            return movies;
        }



        public async Task HandleMovieSelectionAsync(int theMovieDbId)
        {
            await HandleMovieSelectionAsync(await _searcher.SearchMovieAsync(new MovieRequest(_user, _categoryId), theMovieDbId));
        }

        private async Task HandleMovieSelectionAsync(Movie movie)
        {
            if (CanBeRequested(movie))
            {
                await _userInterface.DisplayMovieDetailsAsync(new MovieRequest(_user, _categoryId), movie);
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
            var movie = await _searcher.SearchMovieAsync(new MovieRequest(_user, _categoryId), theMovieDbId);
            var result = await _requester.RequestMovieAsync(new MovieRequest(_user, _categoryId), movie);

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