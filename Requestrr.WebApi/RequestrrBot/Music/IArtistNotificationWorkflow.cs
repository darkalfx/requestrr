using System.Threading.Tasks;

namespace Requestrr.WebApi.RequestrrBot.Music
{
    public interface IArtistNotificationWorkflow
    {
        Task NotifyForNewRequestAsync(string userId, Artist artist);
        Task NotifyForExistingRequestAsync(string userId, Artist artist);
    }
}