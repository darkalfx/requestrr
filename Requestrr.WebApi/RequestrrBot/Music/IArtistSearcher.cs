using System.Collections.Generic;
using System.Threading.Tasks;

namespace Requestrr.WebApi.RequestrrBot.Music
{
    public interface IArtistSearcher
    {
        Task<IReadOnlyList<Artist>> SearchArtistAsync(string artistName);

        Task<Dictionary<string, Artist>> SearchAvailableArtistsAsync(HashSet<string> artists, System.Threading.CancellationToken token);
        Task<Artist> SearchArtistByIdAsync(string mbId);
    }
}