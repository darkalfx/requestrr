using DSharpPlus.SlashCommands;
using Requestrr.WebApi.RequestrrBot.ChatClients.Discord;
using Requestrr.WebApi.RequestrrBot.DownloadClients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Requestrr.WebApi.RequestrrBot.Movies
{
    public class MovieIssueWorkflow
    {
        private readonly int _categoryId;
        private readonly MovieUserRequester _user;
        private readonly IMovieSearcher _searcher;
        private readonly IMovieRequester _requester;
        private readonly IMovieUserInterface _userInterface;
        private readonly IMovieNotificationWorkflow _notificationWorkflow;

        public MovieIssueWorkflow(
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
                    await _userInterface.ShowMovieIssueSelection(new MovieRequest(_user, _categoryId), movies);
                }
                else if (movies.Count == 1)
                {
                    var movie = movies.Single();
                    await HandleIssueMovieSelectionAsync(movie);
                }
            }
        }




        /// <summary>
        /// This handles the checking of movies in library
        /// </summary>
        /// <param name="movieName">Name of the movie to search for</param>
        /// <returns></returns>
        private async Task<IReadOnlyList<Movie>> SearchMoviesLibraryAsync(string movieName)
        {
            IReadOnlyList<Movie> movies = Array.Empty<Movie>();

            //Check if searcher supports issues
            if (_searcher is not IMovieIssueSearcher)
            {
                await _userInterface.WarnNoMovieFoundAsync(movieName);
                return movies;
            }

            movieName = movieName.Replace(".", " ");
            movies = await ((IMovieIssueSearcher)_searcher).SearchMovieLibraryAsync(new MovieRequest(_user, _categoryId), movieName);

            if (!movies.Any())
            {
                await _userInterface.WarnNoMovieFoundAsync(movieName);
            }

            return movies;
        }


        /// <summary>
        /// This handles the checking of movies in library against whats in library
        /// </summary>
        /// <param name="theMovieDbId">Movie DB Id of the movie to find</param>
        /// <returns></returns>
        public async Task SearchMovieLibraryAsync(int theMovieDbId)
        {
            try
            {
                //Check if searcher supports issues
                if (_searcher is not IMovieIssueSearcher)
                {
                    await _userInterface.WarnNoMovieFoundByTheMovieDbIdAsync(theMovieDbId.ToString());
                    return;
                }

                var movie = await ((IMovieIssueSearcher)_searcher).SearchMovieLibraryAsync(new MovieRequest(_user, _categoryId), theMovieDbId);
                if (movie == null)
                {
                    await _userInterface.WarnNoMovieFoundByTheMovieDbIdAsync(theMovieDbId.ToString());
                }
                else
                {
                    await HandleIssueMovieSelectionAsync(theMovieDbId);
                }
            }
            catch
            {
                await _userInterface.WarnNoMovieFoundByTheMovieDbIdAsync(theMovieDbId.ToString());
            }
        }



        /// <summary>
        /// Handle the converting of a TMDB ID to the title name 
        /// </summary>
        /// <param name="theMovieDbId"></param>
        /// <returns></returns>
        public async Task HandleIssueMovieSelectionAsync(int theMovieDbId, string issue = "")
        {
            await HandleIssueMovieSelectionAsync(await _searcher.SearchMovieAsync(new MovieRequest(_user, _categoryId), theMovieDbId), issue);
        }


        /// <summary>
        /// Handle responce to to issues to a movie
        /// </summary>
        /// <param name="movie"></param>
        /// <returns></returns>
        private async Task HandleIssueMovieSelectionAsync(Movie movie, string issue = "")
        {
            await _userInterface.DisplayMovieIssueDetailsAsync(new MovieRequest(_user, _categoryId), movie, issue);
        }



        /// <summary>
        /// Handle the responce to a issue submittion to send a Modal
        /// </summary>
        /// <param name="theMovieDbId"></param>
        /// <param name="issue"></param>
        /// <returns></returns>
        public async Task HandleIssueMovieSendModalAsync(int theMovieDbId, string issue)
        {
            await _userInterface.DisplayMovieIssueModalAsync(
                new MovieRequest(_user, _categoryId),
                await _searcher.SearchMovieAsync(new MovieRequest(_user, _categoryId), theMovieDbId),
                issue
            );
        }


        /// <summary>
        /// This handles the submittion of the issue from a modal to the issue requester
        /// </summary>
        /// <param name="textBox"></param>
        /// <returns></returns>
        public async Task SubmitIssueMovieModalReadAsync(KeyValuePair<string, string> textBox)
        {
            string[] values = textBox.Key.Split("/");

            int movieId = int.Parse(values[3]);
            string issue = values[4];

            bool result = false;

            //Check if searcher supports issues
            if (_searcher is IMovieIssueRequester)
            {
                result = await ((IMovieIssueRequester)_requester).SubmitMovieIssueAsync(movieId, issue, textBox.Value);
            }

            await _userInterface.CompleteMovieIssueModalRequestAsync(await _searcher.SearchMovieAsync(new MovieRequest(_user, _categoryId), movieId), result);
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
    }
}
