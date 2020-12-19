using System.Threading.Tasks;
using Requestrr.WebApi.RequestrrBot.Notifications.Music;

namespace Requestrr.WebApi.RequestrrBot.Music
{
    public class ArtistNotificationWorkflow : IArtistNotificationWorkflow
    {
        private readonly ArtistNotificationsRepository _notificationsRepository;
        private readonly IArtistUserInterface _userInterface;
        private readonly bool _automaticNotificationForNewRequests;

        public ArtistNotificationWorkflow(
        ArtistNotificationsRepository artistNotificationsRepository,
        IArtistUserInterface userInterface,
        bool automaticNotificationForNewRequests)
        {
            _notificationsRepository = artistNotificationsRepository;
            _userInterface = userInterface;
            _automaticNotificationForNewRequests = automaticNotificationForNewRequests;
        }

        public Task NotifyForNewRequestAsync(string userId, Artist artist)
        {
            if (_automaticNotificationForNewRequests)
            {
                _notificationsRepository.AddNotification(userId, artist.MbId);
            }

            return Task.CompletedTask;
        }

        public async Task NotifyForExistingRequestAsync(string userId, Artist artist)
        {
            if (IsAlreadyNotified(userId, artist))
            {
                await _userInterface.WarnArtistUnavailableAndAlreadyHasNotificationAsync();
            }
            else
            {
                var isRequested = await _userInterface.AskForNotificationRequestAsync();

                if (isRequested)
                {
                    _notificationsRepository.AddNotification(userId, artist.MbId);
                    await _userInterface.DisplayNotificationSuccessAsync(artist);
                }
            }
        }

        private bool IsAlreadyNotified(string userId, Artist artist)
        {
            return _notificationsRepository.HasNotification(userId, artist.MbId);
        }
    }
}