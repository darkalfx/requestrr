using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;

namespace Requestrr.WebApi.RequestrrBot.Music
{
    public interface IArtistUserInterface
    {
        Task WarnNoArtistFoundAsync(string artistName);
        Task<ArtistSelection> GetArtistSelectionAsync(IReadOnlyList<Artist> artists);
        Task WarnInvalidArtistSelectionAsync();
        Task<bool> GetArtistRequestAsync(Artist artist);
        Task DisplayArtistDetailsAsync(Artist artist);
        Task WarnArtistAlreadyAvailableAsync();
        Task DisplayRequestSuccessAsync(Artist artist);
        Task<bool> AskForNotificationRequestAsync();
        Task DisplayNotificationSuccessAsync(Artist artist);
        Task WarnArtistUnavailableAndAlreadyHasNotificationAsync();
        Task DisplayRequestDeniedAsync(Artist artist);
        Task WarnNoArtistFoundByMbIdAsync(string mbIdTextValue);
        Task WarnArtistAlreadyRequestedAsync();
    }

    public class ArtistSelection
    {
        public Artist Artist { get; set; }
        public bool IsCancelled { get; set; }
    }
}