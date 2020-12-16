using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Requestrr.WebApi.RequestrrBot.Music
{
    public class WebHookMusicUserInterface : IArtistUserInterface
    {
        private readonly IArtistUserInterface _artistUserInterface;

        public WebHookMusicUserInterface(IArtistUserInterface artistUserInterface)
        {
            _artistUserInterface = artistUserInterface;
        }

        public Task<bool> AskForNotificationRequestAsync()
        {
            return Task.FromResult(false);
        }

        public Task DisplayArtistDetailsAsync(Artist artist)
        {
            return Task.CompletedTask;
        }

        public Task DisplayNotificationSuccessAsync(Artist artist)
        {
            return Task.CompletedTask;
        }

        public Task DisplayRequestDeniedAsync(Artist artist)
        {
            return Task.CompletedTask;
        }

        public async Task DisplayRequestSuccessAsync(Artist artist)
        {
            await _artistUserInterface.DisplayArtistDetailsAsync(artist);
            await _artistUserInterface.DisplayRequestSuccessAsync(artist);
        }

        public Task<bool> GetArtistRequestAsync(Artist artist)
        {
            return Task.FromResult(true);
        }

        public Task<ArtistSelection> GetArtistSelectionAsync(IReadOnlyList<Artist> artists)
        {
            return Task.FromResult(new ArtistSelection
            {
                Artist = artists.First(),
                IsCancelled = false,
            });
        }

        public Task WarnInvalidArtistSelectionAsync()
        {
            return Task.CompletedTask;
        }

        public Task WarnArtistAlreadyAvailableAsync()
        {
            return Task.CompletedTask;
        }

        public Task WarnArtistAlreadyRequestedAsync()
        {
            return Task.CompletedTask;
        }

        public Task WarnArtistUnavailableAndAlreadyHasNotificationAsync()
        {
            return Task.CompletedTask;
        }

        public Task WarnNoArtistFoundAsync(string artistName)
        {
            return Task.CompletedTask;
        }

        public Task WarnNoArtistFoundByMbIdAsync(string mbIdTextValue)
        {
            return Task.CompletedTask;
        }
    }
}