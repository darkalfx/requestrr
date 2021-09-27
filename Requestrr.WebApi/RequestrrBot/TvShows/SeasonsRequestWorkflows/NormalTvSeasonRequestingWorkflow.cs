using System.Threading.Tasks;

namespace Requestrr.WebApi.RequestrrBot.TvShows.SeasonsRequestWorkflows
{
    public class NormalTvSeasonRequestingWorkflow
    {
        private readonly TvShowUserRequester _user;
        private readonly ITvShowSearcher _searcher;
        private readonly ITvShowRequester _requester;
        private readonly ITvShowUserInterface _userInterface;
        private readonly ITvShowNotificationWorkflow _tvShowNotificationWorkflow;

        public NormalTvSeasonRequestingWorkflow(
            TvShowUserRequester user,
            ITvShowSearcher searcher,
            ITvShowRequester requester,
            ITvShowUserInterface userInterface,
            ITvShowNotificationWorkflow tvShowNotificationWorkflow)
        {
            _user = user;
            _searcher = searcher;
            _requester = requester;
            _userInterface = userInterface;
            _tvShowNotificationWorkflow = tvShowNotificationWorkflow;
        }

        public async Task HandleSelectionAsync(TvShow tvShow, NormalTvSeason selectedSeason)
        {
            if (selectedSeason.IsRequested == RequestedState.Full)
            {
                await RequestNotificationsForSeasonAsync(tvShow, selectedSeason);
            }
            else
            {
                await _userInterface.DisplayTvShowDetailsForSeasonAsync(tvShow, selectedSeason);
            }
        }

        public async Task RequestAsync(TvShow tvShow, NormalTvSeason selectedSeason)
        {
            var result = await _requester.RequestTvShowAsync(_user, tvShow, selectedSeason);

            if (result.WasDenied)
            {
                await _userInterface.DisplayRequestDeniedForSeasonAsync(selectedSeason);
            }
            else
            {
                await _userInterface.DisplayRequestSuccessForSeasonAsync(tvShow, selectedSeason);
                await _tvShowNotificationWorkflow.NotifyForNewRequestAsync(_user.UserId, tvShow, selectedSeason);
            }
        }

        private async Task RequestNotificationsForSeasonAsync(TvShow tvShow, TvSeason selectedSeason)
        {
            if (selectedSeason.IsAvailable)
            {
                await _userInterface.WarnSeasonAlreadyAvailableAsync(tvShow, selectedSeason);
            }
            else
            {
                await _tvShowNotificationWorkflow.NotifyForExistingRequestAsync(_user.UserId, tvShow, selectedSeason);
            }
        }
    }
}