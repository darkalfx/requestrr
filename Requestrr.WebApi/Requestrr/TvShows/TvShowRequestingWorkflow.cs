using System;
using System.Linq;
using System.Threading.Tasks;
using Requestrr.WebApi.Requestrr.Notifications;

namespace Requestrr.WebApi.Requestrr.TvShows
{
    public class TvShowRequestingWorkflow
    {
        private readonly TvShowUserRequester _user;
        private readonly ITvShowSearcher _searcher;
        private readonly ITvShowRequester _requester;
        private readonly ITvShowUserInterface _userInterface;
        private readonly TvShowNotificationsRepository _notificationRequestRepository;

        public TvShowRequestingWorkflow(
            TvShowUserRequester user,
            ITvShowSearcher searcher,
            ITvShowRequester requester,
            ITvShowUserInterface userInterface,
            TvShowNotificationsRepository tvShowNotificationsRepository)
        {
            _user = user;
            _searcher = searcher;
            _requester = requester;
            _userInterface = userInterface;
            _notificationRequestRepository = tvShowNotificationsRepository;
        }

        public async Task HandleTvShowRequestAsync(string tvShowName)
        {
            var searchedTvShows = await _searcher.SearchTvShowAsync(tvShowName);

            if (!searchedTvShows.Any())
            {
                await _userInterface.WarnNoTvShowFoundAsync(tvShowName);
            }
            else if (searchedTvShows.Count > 1)
            {
                var tvShowSelection = await _userInterface.GetTvShowSelectionAsync(searchedTvShows);

                if (!tvShowSelection.IsCancelled && tvShowSelection.SelectedTvShow.IsSpecified)
                {
                    var selection = tvShowSelection.SelectedTvShow.Value;
                    await HandleTvShowSelection(selection);
                }
                else if (!tvShowSelection.IsCancelled)
                {
                    await _userInterface.WarnInvalidTvShowSelectionAsync();
                }
            }
            else if (searchedTvShows.Count == 1)
            {
                var selection = searchedTvShows.Single();
                await HandleTvShowSelection(selection);
            }
        }

        private async Task HandleTvShowSelection(SearchedTvShow searchedTvShow)
        {
            var tvShow = await GetTvShow(searchedTvShow);

            if (!tvShow.Seasons.Any() && tvShow.HasEnded)
            {
                await _userInterface.DisplayTvShowDetailsAsync(tvShow);
                await _userInterface.WarnShowCannotBeRequested(tvShow);
            }
            else if (tvShow.AllSeasonsFullyRequested())
            {
                if (tvShow.Seasons.OfType<FutureTvSeasons>().Any())
                {
                    await new FutureSeasonsRequestingWorkflow(_user, _searcher, _requester, _userInterface, _notificationRequestRepository)
                        .HandleRequestAsync(tvShow, tvShow.Seasons.OfType<FutureTvSeasons>().FirstOrDefault());
                }
                else
                {
                    await _userInterface.DisplayTvShowDetailsAsync(tvShow);
                    await _userInterface.WarnShowHasEnded(tvShow);
                }
            }
            else if (!tvShow.IsMultiSeasons() && tvShow.Seasons.OfType<NormalTvSeason>().Any())
            {
                await new NormalTvSeasonRequestingWorkflow(_user, _searcher, _requester, _userInterface, _notificationRequestRepository)
                    .HandleRequestAsync(tvShow, tvShow.Seasons.OfType<NormalTvSeason>().Single());
            }
            else
            {
                await RequestTvShowSeason(tvShow);
            }
        }

        private async Task RequestTvShowSeason(TvShow tvShow)
        {
            var seasonSelection = await _userInterface.GetTvShowSeasonSelectionAsync(tvShow);

            if (!seasonSelection.IsCancelled && seasonSelection.SelectedSeason.IsSpecified)
            {
                var selectedSeason = seasonSelection.SelectedSeason.Value;

                switch (selectedSeason)
                {
                    case FutureTvSeasons futureTvSeasons:
                        await new FutureSeasonsRequestingWorkflow(_user, _searcher, _requester, _userInterface, _notificationRequestRepository)
                            .HandleRequestAsync(tvShow, futureTvSeasons);
                        break;
                    case AllTvSeasons allTvSeasons:
                        await new AllSeasonsRequestingWorkflow(_user, _searcher, _requester, _userInterface, _notificationRequestRepository)
                            .HandleRequestAsync(tvShow, allTvSeasons);
                        break;
                    case NormalTvSeason normalTvSeason:
                        await new NormalTvSeasonRequestingWorkflow(_user, _searcher, _requester, _userInterface, _notificationRequestRepository)
                            .HandleRequestAsync(tvShow, normalTvSeason);
                        break;
                    default:
                        throw new Exception($"Could not handle season of type \"{selectedSeason.GetType().Name}\"");
                }
            }
            else if (!seasonSelection.IsCancelled)
            {
                await _userInterface.WarnInvalidSeasonSelectionAsync();
            }
        }

        private async Task<TvShow> GetTvShow(SearchedTvShow searchedTvShow)
        {
            var tvShow = await _searcher.GetTvShowDetailsAsync(searchedTvShow);

            if (!tvShow.HasEnded)
            {
                tvShow.Seasons = tvShow.Seasons.Append(new FutureTvSeasons
                {
                    SeasonNumber = tvShow.Seasons.Max(x => x.SeasonNumber) + 1,
                    IsAvailable = false,
                    IsRequested = tvShow.IsRequested ? RequestedState.Full : RequestedState.None,
                }).ToArray();
            }

            if (tvShow.IsMultiSeasons())
            {
                tvShow.Seasons = tvShow.Seasons.Prepend(new AllTvSeasons
                {
                    SeasonNumber = 0,
                    IsAvailable = false,
                    IsRequested = RequestedState.None,
                }).ToArray();
            }

            return tvShow;
        }
    }
}