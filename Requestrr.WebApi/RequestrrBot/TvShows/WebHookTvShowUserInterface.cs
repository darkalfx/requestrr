using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Requestrr.WebApi.RequestrrBot.ChatClients.Discord;

namespace Requestrr.WebApi.RequestrrBot.TvShows
{
    public class WebHookTvShowUserInterface : RequestrrModuleBase<SocketCommandContext>, ITvShowUserInterface
    {
        private readonly ITvShowUserInterface _tvShowUserInterface;

        public WebHookTvShowUserInterface(
            DiscordSocketClient discord,
            SocketCommandContext commandContext,
            DiscordSettingsProvider discordSettingsProvider,
            ITvShowUserInterface tvShowUserInterface)
         : base(discord, commandContext, discordSettingsProvider)
        {
            _tvShowUserInterface = tvShowUserInterface;
        }

        public Task<bool> AskForSeasonNotificationRequestAsync(TvShow tvShow, TvSeason requestedSeason)
        {
            return _tvShowUserInterface.AskForSeasonNotificationRequestAsync(tvShow, requestedSeason);
        }

        public Task DisplayNotificationSuccessForSeasonAsync(TvSeason requestedSeason)
        {
            return _tvShowUserInterface.DisplayNotificationSuccessForSeasonAsync(requestedSeason);
        }

        public Task DisplayRequestDeniedForSeasonAsync(TvSeason selectedSeason)
        {
            return _tvShowUserInterface.DisplayRequestDeniedForSeasonAsync(selectedSeason);
        }

        public Task DisplayRequestSuccessForSeasonAsync(TvSeason selectedSeason)
        {
            return _tvShowUserInterface.DisplayRequestSuccessForSeasonAsync(selectedSeason);
        }

        public Task DisplayTvShowDetailsAsync(TvShow tvShow)
        {
            return _tvShowUserInterface.DisplayTvShowDetailsAsync(tvShow);
        }

        public Task<bool> GetTvShowRequestConfirmationAsync(TvSeason season)
        {
            return Task.FromResult(true);
        }

        public async Task<TvSeasonsSelection> GetTvShowSeasonSelectionAsync(TvShow tvShow)
        {
            var tvShowSeasons = tvShow.Seasons;

            var selectionMessage = await NextMessageAsync(Context);
            var selectionMessageContent = selectionMessage?.Content?.Trim();

            await DeleteSafeAsync(selectionMessage);

            if (selectionMessageContent.ToLower().StartsWith("all") && tvShowSeasons.OfType<AllTvSeasons>().Any())
            {
                return new TvSeasonsSelection
                {
                    SelectedSeason = tvShowSeasons.OfType<AllTvSeasons>().First(),
                };
            }
            else if (selectionMessageContent.ToLower().StartsWith("future") && tvShowSeasons.OfType<FutureTvSeasons>().Any())
            {
                return new TvSeasonsSelection
                {
                    SelectedSeason = tvShowSeasons.OfType<FutureTvSeasons>().First(),
                };
            }
            else if (int.TryParse(selectionMessageContent, out var selectedSeasonNumber) && tvShowSeasons.Any(x => x.SeasonNumber == selectedSeasonNumber))
            {
                var selectedSeason = tvShowSeasons.Single(x => x.SeasonNumber == selectedSeasonNumber);

                return new TvSeasonsSelection
                {
                    SelectedSeason = selectedSeason
                };
            }
            else
            {
                return new TvSeasonsSelection
                {
                    IsCancelled = true
                };
            }
        }

        public Task<TvShowSelection> GetTvShowSelectionAsync(IReadOnlyList<SearchedTvShow> tvShows)
        {
            return Task.FromResult(new TvShowSelection
            {
                IsCancelled = false,
                SelectedTvShow = tvShows.First()
            });
        }

        public Task WarnAlreadyNotifiedForSeasonsAsync(TvShow tvShow, TvSeason requestedSeason)
        {
            return _tvShowUserInterface.WarnAlreadyNotifiedForSeasonsAsync(tvShow, requestedSeason);
        }

        public Task WarnAlreadySeasonAlreadyRequestedAsync(TvShow tvShow, TvSeason selectedSeason)
        {
            return _tvShowUserInterface.WarnAlreadySeasonAlreadyRequestedAsync(tvShow, selectedSeason);
        }

        public Task WarnInvalidSeasonSelectionAsync()
        {
            return _tvShowUserInterface.WarnInvalidSeasonSelectionAsync();
        }

        public Task WarnInvalidTvShowSelectionAsync()
        {
            return _tvShowUserInterface.WarnInvalidTvShowSelectionAsync();
        }

        public Task WarnNoTvShowFoundAsync(string tvShowName)
        {
            return _tvShowUserInterface.WarnNoTvShowFoundAsync(tvShowName);
        }

        public Task WarnNoTvShowFoundByTvDbIdAsync(string tvDbIdTextValue)
        {
            return _tvShowUserInterface.WarnNoTvShowFoundByTvDbIdAsync(tvDbIdTextValue);
        }

        public Task WarnSeasonAlreadyAvailableAsync(TvSeason requestedSeason)
        {
            return _tvShowUserInterface.WarnSeasonAlreadyAvailableAsync(requestedSeason);
        }

        public Task WarnShowCannotBeRequestedAsync(TvShow tvShow)
        {
            return _tvShowUserInterface.WarnShowCannotBeRequestedAsync(tvShow);
        }

        public Task WarnShowHasEndedAsync(TvShow tvShow)
        {
            return _tvShowUserInterface.WarnShowHasEndedAsync(tvShow);
        }
    }
}