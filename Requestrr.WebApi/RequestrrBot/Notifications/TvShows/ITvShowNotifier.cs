using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Requestrr.WebApi.RequestrrBot.TvShows;

namespace Requestrr.WebApi.RequestrrBot.Notifications.TvShows
{
    public interface ITvShowNotifier
    {
        Task<HashSet<string>> NotifyAsync(IReadOnlyCollection<string> userIds, TvShow tvShow, int seasonNumber, CancellationToken token);
    }
}