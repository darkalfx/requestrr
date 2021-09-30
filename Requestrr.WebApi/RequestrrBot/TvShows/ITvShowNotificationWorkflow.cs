using System.Threading.Tasks;

namespace Requestrr.WebApi.RequestrrBot.TvShows
{
    public interface ITvShowNotificationWorkflow
    {
        Task NotifyForNewRequestAsync(string userId, TvShow tvShow, TvSeason selectedSeason);
        Task NotifyForExistingRequestAsync(string userId, TvShow tvShow, TvSeason selectedSeason);
        Task AddNotificationAsync(string userId, int theTvDbId, string seasonType, int seasonNumber);
    }
}