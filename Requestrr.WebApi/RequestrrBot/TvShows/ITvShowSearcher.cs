using System.Collections.Generic;
using System.Threading.Tasks;

namespace Requestrr.WebApi.RequestrrBot.TvShows
{
    public interface ITvShowSearcher
    {
        Task<IReadOnlyList<SearchedTvShow>> SearchTvShowAsync(TvShowRequest request, string tvShowName);
        Task<TvShow> GetTvShowDetailsAsync(TvShowRequest request, int tvDbId);
        Task<IReadOnlyList<TvShow>> GetTvShowDetailsAsync(HashSet<int> tvShows, System.Threading.CancellationToken token);
        Task<SearchedTvShow> SearchTvShowAsync(TvShowRequest request, int tvDbId);
    }
}