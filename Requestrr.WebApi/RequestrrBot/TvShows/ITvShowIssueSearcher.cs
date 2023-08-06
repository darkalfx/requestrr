
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Requestrr.WebApi.RequestrrBot.TvShows
{
    public interface ITvShowIssueSearcher
    {
        /// <summary>
        /// Used to check the interal TV library
        /// </summary>
        /// <param name="request">TV Show Requested</param>
        /// <param name="tvShowName">Name of TV Show Requested</param>
        /// <returns>Returns a list of movies matching the name in the library</returns>
        Task<IReadOnlyList<SearchedTvShow>> SearchTvShowLibraryAsync(TvShowRequest request, string tvShowName);
        
        /// <summary>
        /// Used to check the interal TV library
        /// </summary>
        /// <param name="request">TV Show Requested</param>
        /// <param name="tvDbId">TVDB ID of the TV Show Requested</param>
        /// <returns>Returns a list of movies matching the name in the library</returns>
        Task<SearchedTvShow> SearchTvShowLibraryAsync(TvShowRequest request, int tvDbId);
        
        List<string> IssueTypes { get; }
    }
}