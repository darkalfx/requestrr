using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Requestrr.WebApi.RequestrrBot.Movies;

namespace Requestrr.WebApi.RequestrrBot.Notifications.Movies
{
    public interface IMovieNotifier
    {
        Task<HashSet<string>> NotifyAsync(IReadOnlyCollection<string> userIds, Movie movie, CancellationToken token);
    }
}