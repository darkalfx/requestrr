using System.Threading.Tasks;

namespace Requestrr.WebApi.RequestrrBot.Music
{
    public interface IArtistRequester
    {
        Task<ArtistRequestResult> RequestArtistAsync(MusicUserRequester requester, Artist artist);
    }

    public class ArtistRequestResult
    {
        public bool WasDenied { get; set; }
    }
}