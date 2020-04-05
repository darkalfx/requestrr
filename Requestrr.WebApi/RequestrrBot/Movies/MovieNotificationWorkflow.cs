using System.Threading.Tasks;
using Requestrr.WebApi.RequestrrBot.Notifications.Movies;

namespace Requestrr.WebApi.RequestrrBot.Movies
{
    public class MovieNotificationWorkflow : IMovieNotificationWorkflow
    {
        private readonly MovieNotificationsRepository _notificationsRepository;
        private readonly IMovieUserInterface _userInterface;
        private readonly bool _automaticNotificationForNewRequests;

        public MovieNotificationWorkflow(
        MovieNotificationsRepository movieNotificationsRepository,
        IMovieUserInterface userInterface,
        bool automaticNotificationForNewRequests)
        {
            _notificationsRepository = movieNotificationsRepository;
            _userInterface = userInterface;
            _automaticNotificationForNewRequests = automaticNotificationForNewRequests;
        }

        public Task NotifyForNewRequestAsync(string userId, Movie movie)
        {
            if (_automaticNotificationForNewRequests)
            {
                _notificationsRepository.AddNotification(userId, int.Parse(movie.TheMovieDbId));
            }

            return Task.CompletedTask;
        }

        public async Task NotifyForExistingRequestAsync(string userId, Movie movie)
        {
            if (IsAlreadyNotified(userId, movie))
            {
                await _userInterface.WarnMovieUnavailableAndAlreadyHasNotificationAsync();
            }
            else
            {
                var isRequested = await _userInterface.AskForNotificationRequestAsync();

                if (isRequested)
                {
                    _notificationsRepository.AddNotification(userId, int.Parse(movie.TheMovieDbId));
                    await _userInterface.DisplayNotificationSuccessAsync(movie);
                }
            }
        }

        private bool IsAlreadyNotified(string userId, Movie movie)
        {
            return _notificationsRepository.HasNotification(userId, int.Parse(movie.TheMovieDbId));
        }
    }
}