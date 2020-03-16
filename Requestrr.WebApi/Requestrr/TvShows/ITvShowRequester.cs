using System.Threading.Tasks;

namespace Requestrr.WebApi.Requestrr.TvShows
{
    public interface ITvShowRequester
    {
        Task<TvShowRequestResult> RequestTvShowAsync(TvShowUserRequester requester, TvShow tvShow, TvSeason seasons);
    }

    public class TvShowRequestResult
    {
        public bool WasDenied { get; set; }
    }
}