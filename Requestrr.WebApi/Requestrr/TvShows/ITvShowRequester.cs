using System.Threading.Tasks;

namespace Requestrr.WebApi.Requestrr.TvShows
{
    public interface ITvShowRequester
    {
        Task RequestTvShowAsync(string userName, TvShow tvShow, TvSeason seasons);
    }
}