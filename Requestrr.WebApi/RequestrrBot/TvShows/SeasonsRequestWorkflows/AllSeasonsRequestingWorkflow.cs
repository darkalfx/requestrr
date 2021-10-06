using System.Linq;
using System.Threading.Tasks;

namespace Requestrr.WebApi.RequestrrBot.TvShows.SeasonsRequestWorkflows
{
    public class AllSeasonsRequestingWorkflow
    {
        private readonly ITvShowSearcher _searcher;
        private readonly ITvShowRequester _requester;
        private readonly ITvShowUserInterface _userInterface;
        private readonly ITvShowNotificationWorkflow _tvShowNotificationWorkflow;

        public AllSeasonsRequestingWorkflow(
            ITvShowSearcher searcher,
            ITvShowRequester requester,
            ITvShowUserInterface userInterface,
            ITvShowNotificationWorkflow tvShowNotificationWorkflow)
        {
            _searcher = searcher;
            _requester = requester;
            _userInterface = userInterface;
            _tvShowNotificationWorkflow = tvShowNotificationWorkflow;
        }

        public async Task HandleSelectionAsync(TvShowRequest request, TvShow tvShow, AllTvSeasons selectedSeason)
        {
            if(tvShow.AllSeasonsFullyRequested())
            {
                await _userInterface.WarnAlreadySeasonAlreadyRequestedAsync(tvShow, new FutureTvSeasons());
            }
            else
            {
                await _userInterface.DisplayTvShowDetailsForSeasonAsync(request, tvShow, selectedSeason);
            }
        }

        public async Task RequestAsync(TvShowRequest request, TvShow tvShow, AllTvSeasons selectedSeason)
        {
            var result = await _requester.RequestTvShowAsync(request, tvShow, selectedSeason);

            if (result.WasDenied)
            {
                await _userInterface.DisplayRequestDeniedForSeasonAsync(tvShow, selectedSeason);
            }
            else
            {
                await _userInterface.DisplayRequestSuccessForSeasonAsync(tvShow, selectedSeason);

                foreach (var season in tvShow.Seasons.OfType<NormalTvSeason>().Where(x => !x.IsAvailable))
                {
                    await _tvShowNotificationWorkflow.NotifyForNewRequestAsync(request.User.UserId, tvShow, season);
                }
            }
        }
    }
}