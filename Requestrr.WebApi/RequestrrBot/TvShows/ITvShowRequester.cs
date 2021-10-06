using System.Threading.Tasks;

namespace Requestrr.WebApi.RequestrrBot.TvShows
{
    public interface ITvShowRequester
    {
        Task<TvShowRequestResult> RequestTvShowAsync(TvShowRequest request, TvShow tvShow, TvSeason seasons);
    }

    public class TvShowRequestResult
    {
        public TvShow TvShow { get; set; }
        public bool WasDenied { get; set; }
    }
}