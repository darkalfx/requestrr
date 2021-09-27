using System.Threading.Tasks;

namespace Requestrr.WebApi.RequestrrBot.Movies
{
    public interface IMovieNotificationWorkflow
    {
        Task NotifyForNewRequestAsync(string userId, Movie movie);
        Task NotifyForExistingRequestAsync(string userId, Movie movie);
        Task AddNotificationAsync(string userId, int theMovieDbId);
    }
}