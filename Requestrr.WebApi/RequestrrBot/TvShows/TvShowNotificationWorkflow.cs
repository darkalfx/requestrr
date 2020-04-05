using System.Threading.Tasks;
using Requestrr.WebApi.RequestrrBot.Notifications.TvShows;

namespace Requestrr.WebApi.RequestrrBot.TvShows
{
    public class TvShowNotificationWorkflow : ITvShowNotificationWorkflow
    {
        private readonly TvShowNotificationsRepository _notificationsRepository;
        private readonly ITvShowUserInterface _userInterface;
        private readonly bool _automaticNotificationForNewRequests;

        public TvShowNotificationWorkflow(
        TvShowNotificationsRepository movieNotificationsRepository,
        ITvShowUserInterface userInterface,
        bool automaticNotificationForNewRequests)
        {
            _notificationsRepository = movieNotificationsRepository;
            _userInterface = userInterface;
            _automaticNotificationForNewRequests = automaticNotificationForNewRequests;
        }

        public Task NotifyForNewRequestAsync(string userId, TvShow tvShow, TvSeason selectedSeason)
        {
            if (_automaticNotificationForNewRequests)
            {
                _notificationsRepository.AddSeasonNotification(userId, tvShow.TheTvDbId, selectedSeason);
            }

            return Task.CompletedTask;
        }

        public async Task NotifyForExistingRequestAsync(string userId, TvShow tvShow, TvSeason selectedSeason)
        {
            if (_notificationsRepository.HasSeasonNotification(userId, tvShow.TheTvDbId, selectedSeason))
            {
                await _userInterface.WarnAlreadyNotifiedForSeasonsAsync(tvShow, selectedSeason);
            }
            else
            {
                var isRequested = await _userInterface.AskForSeasonNotificationRequestAsync(tvShow, selectedSeason);

                if (isRequested)
                {
                    _notificationsRepository.AddSeasonNotification(userId, tvShow.TheTvDbId, selectedSeason);
                    await _userInterface.DisplayNotificationSuccessForSeasonAsync(selectedSeason);
                }
            }
        }
    }
}