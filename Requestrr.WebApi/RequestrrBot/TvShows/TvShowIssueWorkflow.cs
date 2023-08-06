

using Requestrr.WebApi.RequestrrBot.Movies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Requestrr.WebApi.RequestrrBot.TvShows
{
    public class TvShowIssueWorkflow
    {
        private readonly TvShowUserRequester _user;
        private readonly int _categoryId;
        private readonly ITvShowSearcher _searcher;
        private readonly ITvShowRequester _requester;
        private readonly ITvShowUserInterface _userInterface;
        private readonly ITvShowNotificationWorkflow _tvShowNotificationWorkflow;
        private readonly TvShowsSettings _settings;

        public TvShowIssueWorkflow(
            TvShowUserRequester user,
            int categoryId,
            ITvShowSearcher searcher,
            ITvShowRequester requester,
            ITvShowUserInterface userInterface,
            ITvShowNotificationWorkflow tvShowNotificationWorkflow,
            TvShowsSettings settings)
        {
            _user = user;
            _categoryId = categoryId;
            _searcher = searcher;
            _requester = requester;
            _userInterface = userInterface;
            _tvShowNotificationWorkflow = tvShowNotificationWorkflow;
            _settings = settings;
        }
        
        
        /// <summary>
        /// Searchs for Tv Shows in library and responces
        /// </summary>
        /// <param name="tvShowName">Name of the TV Show</param>
        /// <returns></returns>
        public async Task SearchTvShowLibraryAsync(string tvShowName)
        {
            var searchedTvShows = await SearchTvShowLibraryByNameAsync(tvShowName);

            if (searchedTvShows.Any())
            {
                if (searchedTvShows.Count > 1)
                {
                    await _userInterface.ShowTvShowIssueSelection(new TvShowRequest(_user, _categoryId), searchedTvShows);
                }
                else if (searchedTvShows.Count == 1)
                {
                    var selection = searchedTvShows.Single();
                    await HandleTvShowSelectionAsync(selection.TheTvDbId);
                }
            }
        }



        /// <summary>
        /// Handles the fetching of a TV show by TVDB ID in your library
        /// </summary>
        /// <param name="tvDbId"></param>
        /// <returns></returns>
        public async Task SearchTvShowLibraryAsync(int tvDbId)
        {
            try
            {
                //Check if searcher supports issues
                if (_searcher is not ITvShowIssueSearcher)
                {
                    await _userInterface.WarnNoTvShowFoundByTvDbIdAsync(tvDbId);
                    return;
                }

                var searchedTvShow = await ((ITvShowIssueSearcher)_searcher).SearchTvShowLibraryAsync(new TvShowRequest(_user, _categoryId), tvDbId);
                if (searchedTvShow == null)
                {
                    await _userInterface.WarnNoTvShowFoundByTvDbIdAsync(tvDbId);
                }
                else
                {
                    await HandleTvShowSelectionAsync(searchedTvShow.TheTvDbId);
                }
            }
            catch
            {
                await _userInterface.WarnNoTvShowFoundByTvDbIdAsync(tvDbId);
            }
        }



        /// <summary>
        /// This handles the checking of the TV Show in the library
        /// </summary>
        /// <param name="tvShowName"></param>
        /// <returns></returns>
        private async Task<IReadOnlyList<SearchedTvShow>> SearchTvShowLibraryByNameAsync(string tvShowName)
        {
            IReadOnlyList<SearchedTvShow> searchedTvShows = Array.Empty<SearchedTvShow>();
            
            //Check if searcher supports issues
            if (_searcher is not ITvShowIssueSearcher)
            {
                await _userInterface.WarnNoTvShowFoundAsync(tvShowName);
                return searchedTvShows;
            }

            tvShowName = tvShowName.Replace(".", " ");
            searchedTvShows = await ((ITvShowIssueSearcher)_searcher).SearchTvShowLibraryAsync(new TvShowRequest(_user, _categoryId), tvShowName);

            if (!searchedTvShows.Any())
            {
                await _userInterface.WarnNoTvShowFoundAsync(tvShowName);
            }

            return searchedTvShows;
        }
        
        
        
        
        public async Task HandleTvShowSelectionAsync(int tvDbId)
        {
            var tvShow = await GetTvShowAsync(tvDbId);
            await _userInterface.DisplayTvShowIssueDetailsAsync(new TvShowRequest(_user, _categoryId), tvShow, string.Empty);
        }
        
        
        
        private async Task<TvShow> GetTvShowAsync(int tvDbId)
        {
            var tvShow = await _searcher.GetTvShowDetailsAsync(new TvShowRequest(_user, _categoryId), tvDbId);

            if (tvShow.IsMultiSeasons())
            {
                if (_settings.Restrictions == TvShowsRestrictions.AllSeasons || tvShow.Seasons.Length <= 24)
                {
                    tvShow.Seasons = tvShow.Seasons.Prepend(new AllTvSeasons
                    {
                        SeasonNumber = 0,
                        IsAvailable = false,
                        IsRequested = RequestedState.None,
                    }).ToArray();
                }
                else
                {
                    tvShow.Seasons = tvShow.Seasons.TakeLast(25).ToArray();
                }
            }

            return tvShow;
        }



        /// <summary>
        /// Handle the converting of a TMDB ID to the title name 
        /// </summary>
        /// <param name="theMovieDbId"></param>
        /// <returns></returns>
        public async Task HandleIssueTVSelectionAsync(int theTVDbId, string issue = "")
        {
            await HandleIssueTVSelectionAsync(await _searcher.GetTvShowDetailsAsync(new TvShowRequest(_user, _categoryId), theTVDbId), issue);
        }


        /// <summary>
        /// Handle responce to submit issues for a TV Show
        /// </summary>
        /// <param name="tvShow"></param>
        /// <param name="issue"></param>
        /// <returns></returns>
        private async Task HandleIssueTVSelectionAsync(TvShow tvShow, string issue = "")
        {
            await _userInterface.DisplayTvShowIssueDetailsAsync(new TvShowRequest(_user, _categoryId), tvShow, issue);
        }


        /// <summary>
        /// Handles responce to Discord responce service to handle creating Modal
        /// </summary>
        /// <param name="tvShow"></param>
        /// <param name="issue"></param>
        /// <returns></returns>
        public async Task HandleIssueTvShowSendModalAsync(int theTVDbId, string issue) //TvShow tvShow, string issue = "")
        {
            await _userInterface.DisplayTvShowIssueModalAsync(
                new TvShowRequest(_user, _categoryId),
                await _searcher.GetTvShowDetailsAsync(new TvShowRequest(_user, _categoryId), theTVDbId),
                issue
            );
        }



        /// <summary>
        /// Submit an issue
        /// </summary>
        /// <param name="textBox">Holds the details sent back form the user</param>
        /// <returns></returns>
        public async Task SubmitIssueTvShowModalReadAsync(KeyValuePair<string, string> textBox)
        {
            string[] values = textBox.Key.Split("/");

            int tvShowId = int.Parse(values[3]);
            string issue = values[4];

            bool result = false;

            //Check if searcher supports issues
            if (_searcher is ITvShowIssueRequester)
            {
                result = await ((ITvShowIssueRequester)_requester).SubmitTvShowIssueAsync(tvShowId, issue, textBox.Value);
            }

            await _userInterface.CompleteTvShowIssueModalRequestAsync(await _searcher.GetTvShowDetailsAsync(new TvShowRequest(_user, _categoryId), tvShowId), result);
        }
    }
}