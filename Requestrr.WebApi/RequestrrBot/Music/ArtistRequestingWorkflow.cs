using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Requestrr.WebApi.RequestrrBot.Music
{
    public class ArtistRequestingWorkflow
    {
        private readonly MusicUserRequester _user;
        private readonly IArtistSearcher _searcher;
        private readonly IArtistRequester _requester;
        private readonly IArtistUserInterface _userInterface;
        private readonly IArtistNotificationWorkflow _notificationWorkflow;

        public ArtistRequestingWorkflow(
            MusicUserRequester user,
            IArtistSearcher searcher,
            IArtistRequester requester,
            IArtistUserInterface userInterface,
            IArtistNotificationWorkflow artistNotificationWorkflow)
        {
            _user = user;
            _searcher = searcher;
            _requester = requester;
            _userInterface = userInterface;
            _notificationWorkflow = artistNotificationWorkflow;
        }

        public async Task RequestArtistAsync(string artistName)
        {
            var artists = await SearchArtistsAsync(artistName);

            if (artists.Any())
            {
                if (artists.Count > 1)
                {
                    var artistSelection = await _userInterface.GetArtistSelectionAsync(artists);

                    if (!artistSelection.IsCancelled && artistSelection.Artist != null)
                    {
                        var artist = artistSelection.Artist;
                        await HandleArtistSelectionAsync(artist);
                    }
                    else if (!artistSelection.IsCancelled)
                    {
                        await _userInterface.WarnInvalidArtistSelectionAsync();
                    }
                }
                else if (artists.Count == 1)
                {
                    var artist = artists.Single();
                    await HandleArtistSelectionAsync(artist);
                }
            }
        }

        public async Task<IReadOnlyList<Artist>> SearchArtistsAsync(string artistName)
        {
            IReadOnlyList<Artist> artists = Array.Empty<Artist>();

            if (artistName.Trim().ToLower().StartsWith("mbid"))
            {
                var musicBrainzIdTextValue = artistName.ToLower().Split("mbid")[1]?.Trim();

                if (int.TryParse(musicBrainzIdTextValue, out var theMovieDbId))
                {
                    try
                    {
                        var artist = await _searcher.SearchArtistByIdAsync(musicBrainzIdTextValue);
                        artists = new List<Artist> { artist }.Where(x => x != null).ToArray();
                    }
                    catch
                    {
                        artists = new List<Artist>();
                    }

                    if (!artists.Any())
                    {
                        await _userInterface.WarnNoArtistFoundByMbIdAsync(musicBrainzIdTextValue);
                    }
                }
                else
                {
                    await _userInterface.WarnNoArtistFoundByMbIdAsync(musicBrainzIdTextValue);
                }
            }
            else
            {
                artistName = artistName.Replace(".", " ");
                artists = await _searcher.SearchArtistAsync(artistName);

                if (!artists.Any())
                {
                    await _userInterface.WarnNoArtistFoundAsync(artistName);
                }
            }

            return artists;
        }

        private async Task HandleArtistSelectionAsync(Artist artist)
        {
            await _userInterface.DisplayArtistDetailsAsync(artist);

            if (CanBeRequested(artist))
            {
                var wasRequested = await _userInterface.GetArtistRequestAsync(artist);

                if (wasRequested)
                {
                    var result = await _requester.RequestArtistAsync(_user, artist);

                    if (result.WasDenied)
                    {
                        await _userInterface.DisplayRequestDeniedAsync(artist);
                    }
                    else
                    {
                        await _userInterface.DisplayRequestSuccessAsync(artist);
                        await _notificationWorkflow.NotifyForNewRequestAsync(_user.UserId, artist);
                    }
                }
            }
            else
            {
                //TODO Rework for Available
                await _notificationWorkflow.NotifyForExistingRequestAsync(_user.UserId, artist);
            }
        }

        private static bool CanBeRequested(Artist artist)
        {
            return !artist.IsRequested;
        }
    }
}