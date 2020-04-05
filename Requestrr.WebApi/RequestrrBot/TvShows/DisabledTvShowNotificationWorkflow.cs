using System.Threading.Tasks;

namespace Requestrr.WebApi.RequestrrBot.TvShows
{
    public class DisabledTvShowNotificationWorkflow : ITvShowNotificationWorkflow
    {
        private readonly ITvShowUserInterface _userInterface;

        public DisabledTvShowNotificationWorkflow(ITvShowUserInterface userInterface)
        {
            _userInterface = userInterface;
        }

        public Task NotifyForNewRequestAsync(string userId, TvShow tvShow, TvSeason selectedSeason)
        {
            return Task.CompletedTask;
        }

        public Task NotifyForExistingRequestAsync(string userId, TvShow tvShow, TvSeason selectedSeason)
        {
            return _userInterface.WarnAlreadySeasonAlreadyRequestedAsync(tvShow, selectedSeason);
        }
    }
}