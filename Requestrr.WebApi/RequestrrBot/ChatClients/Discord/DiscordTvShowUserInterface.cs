using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Requestrr.WebApi.RequestrrBot.Locale;
using Requestrr.WebApi.RequestrrBot.TvShows;

namespace Requestrr.WebApi.RequestrrBot.ChatClients.Discord
{
    public class DiscordTvShowUserInterface : ITvShowUserInterface
    {
        private readonly DiscordInteraction _interactionContext;

        public DiscordTvShowUserInterface(DiscordInteraction interactionContext)
        {
            _interactionContext = interactionContext;
        }

        public static DiscordEmbed GenerateTvShowDetailsAsync(TvShow tvShow)
        {
            var title = tvShow.Title;

            if (!string.IsNullOrWhiteSpace(tvShow.FirstAired))
            {
                if (tvShow.FirstAired.Length >= 4 && !title.Contains(tvShow.FirstAired.Split("T")[0].Substring(0, 4), StringComparison.InvariantCultureIgnoreCase))
                {
                    title = $"{title} ({tvShow.FirstAired.Split("T")[0].Substring(0, 4)})";
                }
            }

            var embedBuilder = new DiscordEmbedBuilder()
                .WithTitle(title)
                .WithTimestamp(DateTime.Now)
                .WithThumbnail("https://thetvdb.com/images/logo.png")
                .WithFooter("Powered by Requestrr");

            if (!string.IsNullOrWhiteSpace(tvShow.Overview))
            {
                embedBuilder.WithDescription(tvShow.Overview.Substring(0, Math.Min(tvShow.Overview.Length, 255)) + "(...)");
            }

            if (!string.IsNullOrEmpty(tvShow.Banner) && tvShow.Banner.StartsWith("http", StringComparison.InvariantCultureIgnoreCase)) embedBuilder.WithImageUrl(tvShow.Banner);
            if (!string.IsNullOrWhiteSpace(tvShow.WebsiteUrl)) embedBuilder.WithUrl(tvShow.WebsiteUrl);
            if (!string.IsNullOrWhiteSpace(tvShow.Network)) embedBuilder.AddField($"__{Language.Current.DiscordEmbedTvNetwork}__", tvShow.Network, true);
            if (!string.IsNullOrWhiteSpace(tvShow.Status)) embedBuilder.AddField($"__{Language.Current.DiscordEmbedTvStatus}__", tvShow.Status, true);
            if (!string.IsNullOrWhiteSpace(tvShow.Quality)) embedBuilder.AddField($"__{Language.Current.DiscordEmbedTvQuality}__", $"{tvShow.Quality}p", true);
            if (!string.IsNullOrWhiteSpace(tvShow.PlexUrl)) embedBuilder.AddField($"__Plex__", $"[{Language.Current.DiscordEmbedTvWatchNow}]({tvShow.PlexUrl})", true);
            if (!string.IsNullOrWhiteSpace(tvShow.EmbyUrl)) embedBuilder.AddField($"__Emby__", $"[{Language.Current.DiscordEmbedTvWatchNow}]({tvShow.EmbyUrl})", true);

            return embedBuilder.Build();
        }

        public async Task DisplayMultiSeasonSelectionAsync(TvShow tvShow, TvSeason[] tvSeasons)
        {
            var options = tvSeasons.Select(x => new DiscordSelectComponentOption(GetFormattedSeasonName(x), $"{tvShow.TheTvDbId.ToString()}/{x.SeasonNumber.ToString()}")).ToList();
            var select = new DiscordSelectComponent($"TSS/{_interactionContext.User.Id}", Language.Current.DiscordCommandTvRequestHelpSeasonsDropdown, options);

            await _interactionContext.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddComponents(select).WithContent(Language.Current.DiscordCommandTvRequestHelpSeasons));
        }

        public async Task DisplayNotificationSuccessForSeasonAsync(TvShow tvShow, TvSeason requestedSeason)
        {
            if (requestedSeason is FutureTvSeasons)
            {
                await _interactionContext.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent(Language.Current.DiscordCommandTvNotificationSuccessFutureSeasons.ReplaceTokens(tvShow)));
            }

            await _interactionContext.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent(Language.Current.DiscordCommandTvNotificationSuccessSeason.ReplaceTokens(tvShow, requestedSeason.SeasonNumber)));
        }

        public async Task AskForSeasonNotificationRequestAsync(TvShow tvShow, TvSeason selectedSeason)
        {
            var message = Language.Current.DiscordCommandTvNotificationRequestSeason.ReplaceTokens(tvShow, selectedSeason.SeasonNumber);

            if (selectedSeason is FutureTvSeasons)
            {
                if (tvShow.AllSeasonsAvailable())
                {
                    message = Language.Current.DiscordCommandTvNotificationRequestFutureSeasonAvailable;
                }
                else if (tvShow.AllSeasonsFullyRequested())
                {
                    message = Language.Current.DiscordCommandTvNotificationRequestFutureSeasonRequested;
                }
                else
                {
                    message = Language.Current.DiscordCommandTvNotificationRequestFutureSeasonMissing;
                }
            }

            var requestButton = new DiscordButtonComponent(ButtonStyle.Primary, $"TNR/{_interactionContext.User.Id}/{tvShow.TheTvDbId}/{selectedSeason.GetType().Name.First()}|{selectedSeason.SeasonNumber}", Language.Current.DiscordCommandNotifyMe, false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🔔")));

            var embed = GenerateTvShowDetailsAsync(tvShow);
            await _interactionContext.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed).AddComponents(requestButton).WithContent(message));
        }

        public async Task DisplayRequestDeniedForSeasonAsync(TvSeason selectedSeason)
        {
            await _interactionContext.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent(Language.Current.DiscordCommandTvRequestDenied));
        }

        public async Task DisplayRequestSuccessForSeasonAsync(TvShow tvShow, TvSeason season)
        {
            var message = season is AllTvSeasons
                ? Language.Current.DiscordCommandTvRequestSuccessAllSeasons.ReplaceTokens(tvShow, season.SeasonNumber)
                : season is FutureTvSeasons
                    ? Language.Current.DiscordCommandTvRequestSuccessFutureSeasons.ReplaceTokens(tvShow, season.SeasonNumber)
                    : Language.Current.DiscordCommandTvRequestSuccessSeason.ReplaceTokens(tvShow, season.SeasonNumber);

            await _interactionContext.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent(message));
        }

        public async Task DisplayTvShowDetailsForSeasonAsync(TvShow tvShow, TvSeason season)
        {
            var message = season is AllTvSeasons
                ? Language.Current.DiscordCommandTvRequestConfirmAllSeasons
                : season is FutureTvSeasons
                    ? Language.Current.DiscordCommandTvRequestConfirmFutureSeasons
                    : Language.Current.DiscordCommandTvRequestConfirmSeason.ReplaceTokens(LanguageTokens.SeasonNumber, season.SeasonNumber.ToString());

            var requestButton = new DiscordButtonComponent(ButtonStyle.Primary, $"TRC/{_interactionContext.User.Id}/{tvShow.TheTvDbId}/{season.SeasonNumber}", Language.Current.DiscordCommandRequestButton);

            var embed = GenerateTvShowDetailsAsync(tvShow);
            await _interactionContext.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddComponents(requestButton).AddEmbed(embed).WithContent(message));
        }

        public async Task ShowTvShowSelection(IReadOnlyList<SearchedTvShow> searchedTvShows)
        {
            var options = searchedTvShows.Take(15).Select(x => new DiscordSelectComponentOption(GetFormatedTvShowTitle(x), x.TheTvDbId.ToString())).ToList();
            var select = new DiscordSelectComponent($"TRS/{_interactionContext.User.Id}", Language.Current.DiscordCommandTvRequestHelpSearchDropdown, options);

            await _interactionContext.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddComponents(select).WithContent(Language.Current.DiscordCommandTvRequestHelpSearch));
        }

        public async Task WarnAlreadyNotifiedForSeasonsAsync(TvShow tvShow, TvSeason requestedSeason)
        {
            var embed = GenerateTvShowDetailsAsync(tvShow);

            if (requestedSeason is FutureTvSeasons)
            {
                if (tvShow.AllSeasonsAvailable())
                {
                    await _interactionContext.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed).WithContent(Language.Current.DiscordCommandTvRequestAlreadyExistNotifiedFutureSeasonAvailable));
                }
                else if (tvShow.AllSeasonsFullyRequested())
                {
                    await _interactionContext.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed).WithContent(Language.Current.DiscordCommandTvRequestAlreadyExistNotifiedFutureSeasonRequested));
                }
                else
                {
                    await _interactionContext.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed).WithContent(Language.Current.DiscordCommandTvRequestAlreadyExistNotifiedFutureSeasonFound));
                }
            }
            else
            {
                await _interactionContext.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed).WithContent(Language.Current.DiscordCommandTvRequestAlreadyExistNotifiedSeason.ReplaceTokens(tvShow, requestedSeason.SeasonNumber)));
            }
        }

        public async Task WarnAlreadySeasonAlreadyRequestedAsync(TvShow tvShow, TvSeason requestedSeason)
        {
            var embed = GenerateTvShowDetailsAsync(tvShow);

            if (requestedSeason is FutureTvSeasons)
            {
                if (tvShow.AllSeasonsAvailable())
                {
                    await _interactionContext.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed).WithContent(Language.Current.DiscordCommandTvRequestAlreadyExistFutureSeasonAvailable));
                }
                else if (tvShow.AllSeasonsFullyRequested())
                {
                    await _interactionContext.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed).WithContent(Language.Current.DiscordCommandTvRequestAlreadyExistFutureSeasonRequested));
                }
                else
                {
                    await _interactionContext.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed).WithContent(Language.Current.DiscordCommandTvRequestAlreadyExistFutureSeasonFound));
                }
            }
            else
            {
                await _interactionContext.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed).WithContent(Language.Current.DiscordCommandTvRequestAlreadyExistSeason.ReplaceTokens(tvShow, requestedSeason.SeasonNumber)));
            }
        }

        public async Task WarnNoTvShowFoundAsync(string tvShowName)
        {
            await _interactionContext.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent(Language.Current.DiscordCommandTvNotFound.ReplaceTokens(LanguageTokens.TvShowTitle, tvShowName)));
        }

        public async Task WarnNoTvShowFoundByTvDbIdAsync(int tvDbId)
        {
            await _interactionContext.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent(Language.Current.DiscordCommandTvNotFound.ReplaceTokens(LanguageTokens.TvShowTVDBID, tvDbId.ToString())));
        }

        public async Task WarnSeasonAlreadyAvailableAsync(TvShow tvShow, TvSeason selectedSeason)
        {
            var embed = GenerateTvShowDetailsAsync(tvShow);
            await _interactionContext.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed).WithContent(Language.Current.DiscordCommandTvRequestAlreadyAvailableSeason.ReplaceTokens(LanguageTokens.SeasonNumber, selectedSeason.SeasonNumber.ToString())));
        }

        public async Task WarnShowCannotBeRequestedAsync(TvShow tvShow)
        {
            var embed = GenerateTvShowDetailsAsync(tvShow);
            await _interactionContext.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed).WithContent(Language.Current.DiscordCommandTvRequestUnsupported));
        }

        public async Task WarnShowHasEndedAsync(TvShow tvShow)
        {
            var embed = GenerateTvShowDetailsAsync(tvShow);

            if (tvShow.AllSeasonsAvailable())
            {
                await _interactionContext.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed).WithContent(Language.Current.DiscordCommandTvRequestAlreadyAvailableSeries));
            }
            else
            {
                await _interactionContext.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed).WithContent(Language.Current.DiscordCommandTvRequestAlreadyExistSeries));
            }
        }

        private string GetFormatedTvShowTitle(SearchedTvShow tvShow)
        {
            var releaseYear = !string.IsNullOrWhiteSpace(tvShow.FirstAired) && tvShow.FirstAired.Length >= 4 ? tvShow.FirstAired.Substring(0, 4) : null;

            if (releaseYear != null)
            {
                if (!tvShow.Title.Contains(releaseYear, StringComparison.InvariantCultureIgnoreCase))
                {
                    return $"{tvShow.Title} ({releaseYear})";
                }
            }

            return tvShow.Title;
        }

        private string GetFormattedSeasonName(TvSeason season)
        {
            var seasonName = season is AllTvSeasons
                ? $"{Language.Current.DiscordEmbedTvAllSeasons}"
                : season is FutureTvSeasons
                    ? $"{Language.Current.DiscordEmbedTvFutureSeasons}"
                    : $"{Language.Current.DiscordEmbedTvSeason.ReplaceTokens(LanguageTokens.SeasonNumber, season.SeasonNumber.ToString())}";

            seasonName += season.IsRequested == RequestedState.Full ? $" ({Language.Current.DiscordEmbedTvFullyRequested})" : season.IsRequested == RequestedState.Partial ? $" ({Language.Current.DiscordEmbedTvPartiallyRequested})" : string.Empty;

            return seasonName;
        }
    }
}