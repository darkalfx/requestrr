using System.Threading.Tasks;

namespace Requestrr.WebApi.RequestrrBot.Movies
{
    public class DisabledMovieNotificationWorkflow : IMovieNotificationWorkflow
    {
        private readonly IMovieUserInterface _userInterface;

        public DisabledMovieNotificationWorkflow(IMovieUserInterface userInterface)
        {
            _userInterface = userInterface;
        }

        public Task NotifyForNewRequestAsync(string userId, Movie movie)
        {
            return Task.CompletedTask;
        }

        public Task NotifyForExistingRequestAsync(string userId, Movie movie)
        {
            return _userInterface.WarnMovieAlreadyRequestedAsync(movie);
        }

        public Task AddNotificationAsync(string userId, int theMovieDbId)
        {
            return Task.CompletedTask;
        }
    }
}