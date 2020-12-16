using System.Threading.Tasks;

namespace Requestrr.WebApi.RequestrrBot.Music
{
    public class DisabledArtistNotificationWorkflow : IArtistNotificationWorkflow
    {
        private readonly IArtistUserInterface _userInterface;

        public DisabledArtistNotificationWorkflow(IArtistUserInterface userInterface)
        {
            _userInterface = userInterface;
        }

        public Task NotifyForNewRequestAsync(string userId, Artist artist)
        {
            return Task.CompletedTask;
        }

        public Task NotifyForExistingRequestAsync(string userId, Artist artist)
        {
            return _userInterface.WarnArtistAlreadyRequestedAsync();
        }
    }
}