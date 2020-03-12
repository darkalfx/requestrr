using System.Linq;
using System.Threading.Tasks;
using Requestrr.WebApi.Requestrr.Notifications;

namespace Requestrr.WebApi.Requestrr.TvShows
{
    public class AllSeasonsRequestingWorkflow
    {
        private readonly TvShowUserRequester _user;
        private readonly ITvShowSearcher _searcher;
        private readonly ITvShowRequester _requester;
        private readonly ITvShowUserInterface _userInterface;
        private readonly TvShowNotificationsRepository _notificationRequestRepository;

        public AllSeasonsRequestingWorkflow(
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

        public async Task HandleRequestAsync(TvShow tvShow, AllTvSeasons selectedSeason)
        {
            await _userInterface.DisplayTvShowDetailsAsync(tvShow);
            
            var wasRequested = await _userInterface.GetTvShowRequestConfirmation(selectedSeason);

            if (wasRequested)
            {
                await _requester.RequestTvShowAsync(_user.Username, tvShow, selectedSeason);
                
                foreach (var season in tvShow.Seasons.OfType<NormalTvSeason>().Where(x => !x.IsAvailable))
                {
                    _notificationRequestRepository.AddSeasonNotification(_user.UserId, tvShow.TheTvDbId, season);
                }

                await _userInterface.DisplayRequestSuccessForSeasonAsync(selectedSeason);
            }
        }
    }
}