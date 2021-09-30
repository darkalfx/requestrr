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
            var embed = GenerateTvShowDetailsAsync(tvShow);
            var options = tvSeasons.Select(x => new DiscordSelectComponentOption(GetFormattedSeasonName(tvShow, x), $"{tvShow.TheTvDbId.ToString()}/{x.SeasonNumber.ToString()}")).ToList();
            var seasonSelector = new DiscordSelectComponent($"TSS/{_interactionContext.User.Id}", Language.Current.DiscordCommandTvRequestHelpSeasonsDropdown, options);

            var previousTvSelector = (DiscordSelectComponent)(await _interactionContext.GetOriginalResponseAsync()).Components.First(x => x.Components.OfType<DiscordSelectComponent>().Any()).Components.Single();
            var tvSelector = new DiscordSelectComponent(previousTvSelector.CustomId, tvShow.Title, previousTvSelector.Options);

            var builder = new DiscordWebhookBuilder().AddEmbed(embed).AddComponents(tvSelector).AddComponents(seasonSelector).WithContent(Language.Current.DiscordCommandTvRequestHelpSeasons);

            await _interactionContext.EditOriginalResponseAsync(builder);
        }

        public async Task DisplayNotificationSuccessForSeasonAsync(TvShow tvShow, TvSeason requestedSeason)
        {
            var embed = GenerateTvShowDetailsAsync(tvShow);
            var successButton = new DiscordButtonComponent(ButtonStyle.Success, $"0/1/0", Language.Current.DiscordCommandNotifyMeSuccess);

            if (requestedSeason is FutureTvSeasons)
            {
                var builder = (await AddPreviousDropdownsAsync(tvShow, new DiscordWebhookBuilder().AddEmbed(embed))).AddComponents(successButton).WithContent(Language.Current.DiscordCommandTvNotificationSuccessFutureSeasons.ReplaceTokens(tvShow));
                await _interactionContext.EditOriginalResponseAsync(builder);
            }
            else
            {
                var builder = (await AddPreviousDropdownsAsync(tvShow, new DiscordWebhookBuilder().AddEmbed(embed))).AddComponents(successButton).WithContent(Language.Current.DiscordCommandTvNotificationSuccessSeason.ReplaceTokens(tvShow, requestedSeason.SeasonNumber));
                await _interactionContext.EditOriginalResponseAsync(builder);
            }
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

            var requestButton = new DiscordButtonComponent(ButtonStyle.Primary, $"TNR/{_interactionContext.User.Id}/{tvShow.TheTvDbId}/{selectedSeason.GetType().Name.First()}/{selectedSeason.SeasonNumber}", Language.Current.DiscordCommandNotifyMe, false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🔔")));

            var embed = GenerateTvShowDetailsAsync(tvShow);
            var builder = (await AddPreviousDropdownsAsync(tvShow, new DiscordWebhookBuilder().AddEmbed(embed))).AddComponents(requestButton).WithContent(message);

            await _interactionContext.EditOriginalResponseAsync(builder);
        }

        public async Task DisplayRequestDeniedForSeasonAsync(TvShow tvShow, TvSeason selectedSeason)
        {
            var embed = GenerateTvShowDetailsAsync(tvShow);
            var deniedButton = new DiscordButtonComponent(ButtonStyle.Danger, $"0/1/0", Language.Current.DiscordCommandRequestButtonDenied);
            var builder = (await AddPreviousDropdownsAsync(tvShow, new DiscordWebhookBuilder().AddEmbed(embed))).AddComponents(deniedButton).WithContent(Language.Current.DiscordCommandTvRequestDenied);

            await _interactionContext.EditOriginalResponseAsync(builder);
        }

        public async Task DisplayRequestSuccessForSeasonAsync(TvShow tvShow, TvSeason requestedSeason)
        {
            var embed = GenerateTvShowDetailsAsync(tvShow);

            var message = requestedSeason is AllTvSeasons
                ? Language.Current.DiscordCommandTvRequestSuccessAllSeasons.ReplaceTokens(tvShow, requestedSeason.SeasonNumber)
                : requestedSeason is FutureTvSeasons
                    ? Language.Current.DiscordCommandTvRequestSuccessFutureSeasons.ReplaceTokens(tvShow, requestedSeason.SeasonNumber)
                    : Language.Current.DiscordCommandTvRequestSuccessSeason.ReplaceTokens(tvShow, requestedSeason.SeasonNumber);

            var successButton = new DiscordButtonComponent(ButtonStyle.Success, $"0/1/0", Language.Current.DiscordCommandRequestButtonSuccess);
            var builder = (await AddPreviousDropdownsAsync(tvShow, new DiscordWebhookBuilder().AddEmbed(embed))).AddComponents(successButton).WithContent(message);

            await _interactionContext.EditOriginalResponseAsync(builder);
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
            var builder = (await AddPreviousDropdownsAsync(tvShow, new DiscordWebhookBuilder().AddEmbed(embed))).AddComponents(requestButton).WithContent(message);
            await _interactionContext.EditOriginalResponseAsync(builder);
        }

        public async Task ShowTvShowSelection(IReadOnlyList<SearchedTvShow> searchedTvShows)
        {
            var options = searchedTvShows.Take(15).Select(x => new DiscordSelectComponentOption(GetFormatedTvShowTitle(x), x.TheTvDbId.ToString())).ToList();
            var select = new DiscordSelectComponent($"TRS/{_interactionContext.User.Id}", Language.Current.DiscordCommandTvRequestHelpSearchDropdown, options);

            await _interactionContext.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddComponents(select).WithContent(Language.Current.DiscordCommandTvRequestHelpSearch));
        }

        public async Task WarnAlreadyNotifiedForSeasonsAsync(TvShow tvShow, TvSeason requestedSeason)
        {
            var messageContent = Language.Current.DiscordCommandTvRequestAlreadyExistNotifiedSeason.ReplaceTokens(tvShow, requestedSeason.SeasonNumber);
            var buttonMessage = Language.Current.DiscordCommandRequestedButton;

            if (requestedSeason is FutureTvSeasons)
            {
                if (tvShow.AllSeasonsAvailable())
                {
                    messageContent = Language.Current.DiscordCommandTvRequestAlreadyExistNotifiedFutureSeasonAvailable;
                    buttonMessage = Language.Current.DiscordCommandAvailableButton;
                }
                else if (tvShow.AllSeasonsFullyRequested())
                {
                    messageContent = Language.Current.DiscordCommandTvRequestAlreadyExistNotifiedFutureSeasonRequested;
                }
                else
                {
                    messageContent = Language.Current.DiscordCommandTvRequestAlreadyExistNotifiedFutureSeasonFound;
                }
            }

            var embed = GenerateTvShowDetailsAsync(tvShow);
            var requestButton = new DiscordButtonComponent(ButtonStyle.Primary, $"TTT/{_interactionContext.User.Id}/{tvShow.TheTvDbId}/999", buttonMessage, true);
            var builder = (await AddPreviousDropdownsAsync(tvShow, new DiscordWebhookBuilder().AddEmbed(embed))).WithContent(messageContent).AddComponents(requestButton);

            await _interactionContext.EditOriginalResponseAsync(builder);
        }

        public async Task WarnAlreadySeasonAlreadyRequestedAsync(TvShow tvShow, TvSeason requestedSeason)
        {
            var messageContent = Language.Current.DiscordCommandTvRequestAlreadyExistSeason.ReplaceTokens(tvShow, requestedSeason.SeasonNumber);
            var buttonMessage = Language.Current.DiscordCommandRequestedButton;

            if (requestedSeason is FutureTvSeasons)
            {
                if (tvShow.AllSeasonsAvailable())
                {
                    messageContent = Language.Current.DiscordCommandTvRequestAlreadyExistFutureSeasonAvailable;
                    buttonMessage = Language.Current.DiscordCommandAvailableButton;
                }
                else if (tvShow.AllSeasonsFullyRequested())
                {
                    messageContent = Language.Current.DiscordCommandTvRequestAlreadyExistFutureSeasonRequested;
                }
                else
                {
                    messageContent = Language.Current.DiscordCommandTvRequestAlreadyExistFutureSeasonFound;
                }
            }

            var embed = GenerateTvShowDetailsAsync(tvShow);
            var requestButton = new DiscordButtonComponent(ButtonStyle.Primary, $"TTT/{_interactionContext.User.Id}/{tvShow.TheTvDbId}/999", buttonMessage, true);
            var builder = (await AddPreviousDropdownsAsync(tvShow, new DiscordWebhookBuilder().AddEmbed(embed))).WithContent(messageContent).AddComponents(requestButton);

            await _interactionContext.EditOriginalResponseAsync(builder);
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
            var requestButton = new DiscordButtonComponent(ButtonStyle.Primary, $"TTT/{_interactionContext.User.Id}/{tvShow.TheTvDbId}/999", Language.Current.DiscordCommandAvailableButton, true);
            var builder = (await AddPreviousDropdownsAsync(tvShow, new DiscordWebhookBuilder().AddEmbed(embed))).WithContent(Language.Current.DiscordCommandTvRequestAlreadyAvailableSeason.ReplaceTokens(LanguageTokens.SeasonNumber, selectedSeason.SeasonNumber.ToString())).AddComponents(requestButton);

            await _interactionContext.EditOriginalResponseAsync(builder);
        }

        public async Task WarnShowCannotBeRequestedAsync(TvShow tvShow)
        {
            var embed = GenerateTvShowDetailsAsync(tvShow);
            var requestButton = new DiscordButtonComponent(ButtonStyle.Danger, $"TTT/{_interactionContext.User.Id}/{tvShow.TheTvDbId}/999", Language.Current.DiscordCommandRequestButton, true);
            var builder = (await AddPreviousDropdownsAsync(tvShow, new DiscordWebhookBuilder().AddEmbed(embed))).WithContent(Language.Current.DiscordCommandTvRequestUnsupported).AddComponents(requestButton);

            await _interactionContext.EditOriginalResponseAsync(builder);
        }

        public async Task WarnShowHasEndedAsync(TvShow tvShow)
        {
            var embed = GenerateTvShowDetailsAsync(tvShow);

            if (tvShow.AllSeasonsAvailable())
            {
                var requestButton = new DiscordButtonComponent(ButtonStyle.Primary, $"TTT/{_interactionContext.User.Id}/{tvShow.TheTvDbId}/999", Language.Current.DiscordCommandAvailableButton, true);
                var builder = (await AddPreviousDropdownsAsync(tvShow, new DiscordWebhookBuilder().AddEmbed(embed))).WithContent(Language.Current.DiscordCommandTvRequestAlreadyAvailableSeries).AddComponents(requestButton);
                await _interactionContext.EditOriginalResponseAsync(builder);
            }
            else
            {
                var requestButton = new DiscordButtonComponent(ButtonStyle.Primary, $"TTT/{_interactionContext.User.Id}/{tvShow.TheTvDbId}/999", Language.Current.DiscordCommandRequestedButton, true);
                var builder = (await AddPreviousDropdownsAsync(tvShow, new DiscordWebhookBuilder().AddEmbed(embed))).AddComponents(requestButton).WithContent(Language.Current.DiscordCommandTvRequestAlreadyExistSeries);
                await _interactionContext.EditOriginalResponseAsync(builder);
            }
        }

        private async Task<DiscordWebhookBuilder> AddPreviousDropdownsAsync(TvShow tvShow, DiscordWebhookBuilder builder)
        {
            var previousTvSelector = (DiscordSelectComponent)(await _interactionContext.GetOriginalResponseAsync()).Components.First(x => x.Components.OfType<DiscordSelectComponent>().Any()).Components.Single();
            var tvSelector = new DiscordSelectComponent(previousTvSelector.CustomId, tvShow.Title, previousTvSelector.Options);

            builder.AddComponents(tvSelector);

            var previousSeasonSelector = (DiscordSelectComponent)(await _interactionContext.GetOriginalResponseAsync()).Components.Skip(1).FirstOrDefault(x => x.Components.OfType<DiscordSelectComponent>().Any())?.Components?.Single();

            if (!tvShow.AllSeasonsAvailable() && previousSeasonSelector != null && previousSeasonSelector.Options.Any(x => x.Value.Contains(tvShow.TheTvDbId.ToString(), StringComparison.OrdinalIgnoreCase)))
            {
                if (!_interactionContext.Data.CustomId.StartsWith("trs", StringComparison.OrdinalIgnoreCase))
                {
                    var lastSeasonSelected = _interactionContext.Data.Values.Any()
                        ? _interactionContext.Data.Values.Last()
                        : $"{tvShow.TheTvDbId}/{_interactionContext.Data.CustomId.Split("/").Last()}";

                    var newOptions = tvShow.Seasons.Select(x => new DiscordSelectComponentOption(GetFormattedSeasonName(tvShow, x), $"{tvShow.TheTvDbId.ToString()}/{x.SeasonNumber.ToString()}")).ToDictionary(x => x.Value, x => x);
                    var oldOptions = previousSeasonSelector.Options;

                    var currentOptions = oldOptions.Select(x => new DiscordSelectComponentOption(newOptions.ContainsKey(x.Value) ? newOptions[x.Value].Label : x.Label, x.Value)).ToList();

                    var seasonSelector = new DiscordSelectComponent(previousSeasonSelector.CustomId, currentOptions.Single(x => x.Value == lastSeasonSelected).Label, currentOptions);
                    builder.AddComponents(seasonSelector);
                }
            }

            return builder;
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

        private string GetFormattedSeasonName(TvShow tvShow, TvSeason season)
        {
            var seasonName = season is AllTvSeasons
                ? $"{Language.Current.DiscordEmbedTvAllSeasons}"
                : season is FutureTvSeasons
                    ? $"{Language.Current.DiscordEmbedTvFutureSeasons}"
                    : $"{Language.Current.DiscordEmbedTvSeason.ReplaceTokens(LanguageTokens.SeasonNumber, season.SeasonNumber.ToString())}";

            if (season is AllTvSeasons)
            {
                seasonName += tvShow.AllSeasonsFullyRequested() ? $" ({Language.Current.DiscordEmbedTvFullyRequested})" : season.IsRequested == RequestedState.Partial ? string.Empty : string.Empty;
            }
            else
            {
                seasonName += season.IsRequested == RequestedState.Full ? $" ({Language.Current.DiscordEmbedTvFullyRequested})" : season.IsRequested == RequestedState.Partial ? $" ({Language.Current.DiscordEmbedTvPartiallyRequested})" : string.Empty;
            }

            return seasonName;
        }
    }
}