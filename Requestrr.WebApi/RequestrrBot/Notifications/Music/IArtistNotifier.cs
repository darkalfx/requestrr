using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Requestrr.WebApi.RequestrrBot.Music;

namespace Requestrr.WebApi.RequestrrBot.Notifications.Music
{
    public interface IArtistNotifier
    {
        Task<HashSet<string>> NotifyAsync(IReadOnlyCollection<string> userIds, Artist artist, CancellationToken token);
    }
}