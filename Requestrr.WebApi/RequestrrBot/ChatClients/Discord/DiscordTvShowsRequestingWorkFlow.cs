using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Requestrr.WebApi.RequestrrBot.Locale;
using Requestrr.WebApi.RequestrrBot.Notifications;
using Requestrr.WebApi.RequestrrBot.Notifications.TvShows;
using Requestrr.WebApi.RequestrrBot.TvShows;

namespace Requestrr.WebApi.RequestrrBot.ChatClients.Discord
{
    public class DiscordTvShowsRequestingWorkFlow : RequestrrModuleBase<SocketCommandContext>, ITvShowUserInterface
    {
        private readonly DiscordSocketClient _discord;
        private ITvShowSearcher _tvShowSearcher;
        private readonly ITvShowRequester _tvShowRequester;
        private readonly DiscordSettingsProvider _discordSettingsProvider;
        private readonly TvShowsSettingsProvider _tvShowsSettingsProvider;
        private readonly TvShowNotificationsRepository _notificationsRepository;
        private IUserMessage _lastCommandMessage;
        private readonly DiscordSettings _discordSettings;

        public DiscordTvShowsRequestingWorkFlow(
            SocketCommandContext context,
            DiscordSocketClient discord,
            ITvShowSearcher tvShowSearcher,
            ITvShowRequester tvShowRequester,
            DiscordSettingsProvider discordSettingsProvider,
            TvShowsSettingsProvider tvShowsSettingsProvider,
            TvShowNotificationsRepository notificationsRepository)
                : base(discord, context, discordSettingsProvider)
        {
            _discord = discord;
            _tvShowSearcher = tvShowSearcher;
            _tvShowRequester = tvShowRequester;
            _discordSettingsProvider = discordSettingsProvider;
            _tvShowsSettingsProvider = tvShowsSettingsProvider;
            _notificationsRepository = notificationsRepository;
            _discordSettings = discordSettingsProvider.Provide();
        }

        public async Task HandleTvShowRequestAsync(object[] args)
        {
            var stringArgs = args?.Where(x => !string.IsNullOrWhiteSpace(x?.ToString())).Select(x => x.ToString().Trim()).ToArray() ?? Array.Empty<string>();

            if (!_discordSettings.EnableRequestsThroughDirectMessages && this.Context.Guild == null)
            {
                await ReplyToUserAsync(Language.Current.DiscordCommandNotAvailableInDM);
                return;
            }
            else if (this.Context.Guild != null && _discordSettings.MonitoredChannels.Any() && _discordSettings.MonitoredChannels.All(c => !Context.Message.Channel.Name.Equals(c, StringComparison.InvariantCultureIgnoreCase)))
            {
                return;
            }
            else if (this.Context.Guild != null && _discordSettings.TvShowRoles.Any() && Context.Message.Author is SocketGuildUser guildUser && _discordSettings.TvShowRoles.All(r => !guildUser.Roles.Any(ur => r.Equals(ur.Name, StringComparison.InvariantCultureIgnoreCase))))
            {
                await ReplyToUserAsync(Language.Current.DiscordCommandTvMissingRoles);
                return;
            }
            else if (!stringArgs.Any())
            {
                await ReplyToUserAsync(Language.Current.DiscordCommandTvInvalidArguments.ReplaceTokens(new Dictionary<string, string>
                {
                    { LanguageTokens.CommandPrefix, _discordSettings.CommandPrefix },
                    { LanguageTokens.TvShowCommand, _discordSettings.TvShowCommand }
                }));

                return;
            }

            var IsWebHook = this.Context.Message.Source == MessageSource.Webhook;
            var tvShowName = stringArgs[0].ToString();

            if (IsWebHook)
            {
                await ForceDeleteSafeAsync(this.Context.Message);
            }
            else
            {
                await DeleteSafeAsync(this.Context.Message);
            }

            ITvShowUserInterface userInterface = this;

            if (IsWebHook)
            {
                var webhookMessage = tvShowName.Split("//");
                userInterface = new WebHookTvShowUserInterface(this, webhookMessage[1]);
                tvShowName = webhookMessage[0];
            }

            ITvShowNotificationWorkflow tvShowNotificationWorkflow = new DisabledTvShowNotificationWorkflow(userInterface);

            if (_discordSettings.NotificationMode != NotificationMode.Disabled && !IsWebHook)
            {
                tvShowNotificationWorkflow = new TvShowNotificationWorkflow(_notificationsRepository, userInterface, _discordSettings.AutomaticallyNotifyRequesters);
            }

            var workFlow = new TvShowRequestingWorkflow(
                new TvShowUserRequester(this.Context.Message.Author.Id.ToString(), $"{this.Context.Message.Author.Username}#{this.Context.Message.Author.Discriminator}"),
                 _tvShowSearcher,
                 _tvShowRequester,
                userInterface,
                tvShowNotificationWorkflow,
                _tvShowsSettingsProvider.Provide());

            await workFlow.RequestTvShowAsync(tvShowName);
        }

        public Task WarnNoTvShowFoundAsync(string tvShowName)
        {
            return ReplyToUserAsync(Language.Current.DiscordCommandTvNotFound.ReplaceTokens(LanguageTokens.TvShowTitle, tvShowName));
        }

        public Task WarnNoTvShowFoundByTvDbIdAsync(string tvDbIdTextValue)
        {
            return ReplyToUserAsync(Language.Current.DiscordCommandTvNotFoundTVDBID.ReplaceTokens(LanguageTokens.TvShowTVDBID, tvDbIdTextValue));
        }

        public async Task<TvShowSelection> GetTvShowSelectionAsync(IReadOnlyList<SearchedTvShow> searchedTvShows)
        {
            var embedContent = new StringBuilder();

            for (int i = 0; i < searchedTvShows.Take(10).Count(); i++)
            {
                var tvRow = new StringBuilder();
                tvRow.Append($"{i + 1}) {searchedTvShows[i].Title} ");

                if (!string.IsNullOrWhiteSpace(searchedTvShows[i].FirstAired) && searchedTvShows[i].FirstAired.Length >= 4)
                {
                    var releaseYear = $"({searchedTvShows[i].FirstAired.Substring(0, 4)})";

                    if (!searchedTvShows[i].Title.Contains(releaseYear, StringComparison.InvariantCultureIgnoreCase))
                    {
                        tvRow.Append(releaseYear);
                    }
                }

                tvRow.Append($" [[TheTVDb](https://www.thetvdb.com/?id={searchedTvShows[i].TheTvDbId}&tab=series)]");
                tvRow.AppendLine();

                if (tvRow.Length + embedContent.Length < DiscordConstants.MaxEmbedLength)
                    embedContent.Append(tvRow.ToString());
            }

            var embedBuilder = new EmbedBuilder()
                .WithTitle(Language.Current.DiscordEmbedTvSearch)
                .AddField($"__{Language.Current.DiscordEmbedTvSearchResult}__", embedContent.ToString())
                .WithThumbnailUrl("https://thetvdb.com/images/logo.png");

            var reply = await ReplyAsync(string.Empty, false, embedBuilder.Build());
            var replyHelp = await ReplyToUserAsync(Language.Current.DiscordCommandTvRequestHelpSearch.ReplaceTokens(LanguageTokens.CommandPrefix, _discordSettings.CommandPrefix));

            var selectionMessage = await NextMessageAsync(Context);
            var selectionMessageContent = selectionMessage?.Content?.Trim() ?? Language.Current.DiscordCommandTvCancelCommand.ToLower();

            selectionMessageContent = !string.IsNullOrWhiteSpace(selectionMessageContent) && !string.IsNullOrWhiteSpace(_discordSettings.CommandPrefix) && selectionMessageContent.StartsWith(_discordSettings.CommandPrefix)
            ? selectionMessageContent.Remove(0, _discordSettings.CommandPrefix.Length)
            : selectionMessageContent;

            if (selectionMessageContent.ToLower().StartsWith(Language.Current.DiscordCommandTvCancelCommand.ToLower()))
            {
                await DeleteSafeAsync(selectionMessage);
                await DeleteSafeAsync(replyHelp);
                await DeleteSafeAsync(reply);
                await ReplyToUserAsync(Language.Current.DiscordCommandTvRequestCancelled);

                return new TvShowSelection
                {
                    IsCancelled = true
                };
            }
            else if (int.TryParse(selectionMessageContent, out var selectedTvShow) && selectedTvShow <= searchedTvShows.Count)
            {
                await DeleteSafeAsync(selectionMessage);
                await DeleteSafeAsync(replyHelp);
                await DeleteSafeAsync(reply);

                return new TvShowSelection
                {
                    SelectedTvShow = searchedTvShows[selectedTvShow - 1]
                };
            }

            return new TvShowSelection();
        }

        public Task WarnInvalidTvShowSelectionAsync()
        {
            return ReplyToUserAsync(Language.Current.DiscordCommandTvRequestInvalid);
        }

        public async Task DisplayTvShowDetailsAsync(TvShow tvShow)
        {
            var message = Context.Message;
            _lastCommandMessage = await ReplyAsync(string.Empty, false, GenerateTvShowDetailsAsync(tvShow, message.Author));
        }

        public static Embed GenerateTvShowDetailsAsync(TvShow tvShow, SocketUser user = null)
        {
            var title = tvShow.Title;

            if (!string.IsNullOrWhiteSpace(tvShow.FirstAired))
            {
                if (tvShow.FirstAired.Length >= 4 && !title.Contains(tvShow.FirstAired.Split("T")[0].Substring(0, 4), StringComparison.InvariantCultureIgnoreCase))
                {
                    title = $"{title} ({tvShow.FirstAired.Split("T")[0].Substring(0, 4)})";
                }
            }

            var embedBuilder = new EmbedBuilder()
                .WithTitle(title)
                .WithTimestamp(DateTime.Now)
                .WithUrl($"https://www.thetvdb.com/?id={tvShow.TheTvDbId}&tab=series")
                .WithThumbnailUrl("https://thetvdb.com/images/logo.png");

            if (user != null)
            {
                embedBuilder.WithFooter(user.Username, $"https://cdn.discordapp.com/avatars/{user.Id}/{user.AvatarId}.png");
            }

            if (!string.IsNullOrWhiteSpace(tvShow.Overview))
            {
                embedBuilder.WithDescription(tvShow.Overview.Substring(0, Math.Min(tvShow.Overview.Length, 255)) + "(...)");
            }

            if (!string.IsNullOrEmpty(tvShow.Banner) && tvShow.Banner.StartsWith("http", StringComparison.InvariantCultureIgnoreCase)) embedBuilder.WithImageUrl(tvShow.Banner);
            if (!string.IsNullOrWhiteSpace(tvShow.Network)) embedBuilder.AddField($"__{Language.Current.DiscordEmbedTvNetwork}__", tvShow.Network, true);
            if (!string.IsNullOrWhiteSpace(tvShow.Status)) embedBuilder.AddField($"__{Language.Current.DiscordEmbedTvStatus}__", tvShow.Status, true);
            if (!string.IsNullOrWhiteSpace(tvShow.Quality)) embedBuilder.AddField($"__{Language.Current.DiscordEmbedTvQuality}__", $"{tvShow.Quality}p", true);
            if (!string.IsNullOrWhiteSpace(tvShow.PlexUrl)) embedBuilder.AddField($"__Plex__", $"[{Language.Current.DiscordEmbedTvWatchNow}]({tvShow.PlexUrl})", true);
            if (!string.IsNullOrWhiteSpace(tvShow.EmbyUrl)) embedBuilder.AddField($"__Emby__", $"[{Language.Current.DiscordEmbedTvWatchNow}]({tvShow.EmbyUrl})", true);

            return embedBuilder.Build();
        }

        public async Task<TvSeasonsSelection> GetTvShowSeasonSelectionAsync(TvShow tvShow)
        {
            var fieldContent = string.Empty;
            var tvShowSeasons = tvShow.Seasons;

            foreach (var season in tvShowSeasons)
            {
                var seasonName = season is AllTvSeasons
                    ? $"{season.SeasonNumber}) - {Language.Current.DiscordEmbedTvAllSeasons}"
                    : season is FutureTvSeasons
                        ? $"{season.SeasonNumber}) - {Language.Current.DiscordEmbedTvFutureSeasons}"
                        : $"{season.SeasonNumber}) - {Language.Current.DiscordEmbedTvSeason.ReplaceTokens(LanguageTokens.SeasonNumber, season.SeasonNumber.ToString())}";

                fieldContent += seasonName;
                fieldContent += season.IsRequested == RequestedState.Full ? " ✅" : season.IsRequested == RequestedState.Partial ? " ☑" : string.Empty;
                fieldContent += System.Environment.NewLine;
            }

            var descriptionBuilder = new StringBuilder();
            descriptionBuilder.AppendLine($"✅ - {Language.Current.DiscordEmbedTvFullyRequested}");
            descriptionBuilder.AppendLine($"☑ - {Language.Current.DiscordEmbedTvPartiallyRequested}");
            descriptionBuilder.AppendLine();

            var embedBuilder = new EmbedBuilder()
                .WithTitle($"{tvShow.Title} {Language.Current.DiscordEmbedTvSeasons}")
                .AddField($"__{Language.Current.DiscordEmbedTvSeasons}__", fieldContent)
                .WithDescription(descriptionBuilder.ToString())
                .WithThumbnailUrl("https://thetvdb.com/images/logo.png");

            var reply = await ReplyAsync(string.Empty, false, embedBuilder.Build());
            var replyHelp = await ReplyToUserAsync(Language.Current.DiscordCommandTvRequestHelpSeasons.ReplaceTokens(LanguageTokens.CommandPrefix, _discordSettings.CommandPrefix));
            var selectionMessage = await NextMessageAsync(Context);
            var selectionMessageContent = selectionMessage?.Content?.Trim() ?? Language.Current.DiscordCommandTvCancelCommand.ToLower();

            selectionMessageContent = !string.IsNullOrWhiteSpace(selectionMessageContent) && !string.IsNullOrWhiteSpace(_discordSettings.CommandPrefix) && selectionMessageContent.StartsWith(_discordSettings.CommandPrefix)
            ? selectionMessageContent.Remove(0, _discordSettings.CommandPrefix.Length)
            : selectionMessageContent;

            if (selectionMessageContent.ToLower().StartsWith(Language.Current.DiscordCommandTvCancelCommand.ToLower()))
            {
                await DeleteSafeAsync(selectionMessage);
                await DeleteSafeAsync(replyHelp);
                await DeleteSafeAsync(reply);
                await ReplyToUserAsync(Language.Current.DiscordCommandTvRequestCancelled);

                return new TvSeasonsSelection
                {
                    IsCancelled = true
                };
            }
            else if (int.TryParse(selectionMessageContent, out var selectedSeasonNumber) && tvShowSeasons.Any(x => x.SeasonNumber == selectedSeasonNumber))
            {
                await DeleteSafeAsync(selectionMessage);
                await DeleteSafeAsync(replyHelp);
                await DeleteSafeAsync(reply);

                var selectedSeason = tvShowSeasons.Single(x => x.SeasonNumber == selectedSeasonNumber);

                return new TvSeasonsSelection
                {
                    SelectedSeason = selectedSeason
                };
            }

            return new TvSeasonsSelection();
        }

        public Task WarnInvalidSeasonSelectionAsync()
        {
            return ReplyToUserAsync(Language.Current.DiscordCommandTvRequestInvalid);
        }

        public Task DisplayRequestSuccessForSeasonAsync(TvSeason season)
        {
            var message = season is AllTvSeasons
                ? Language.Current.DiscordCommandTvRequestSuccessAllSeasons
                : season is FutureTvSeasons
                    ? Language.Current.DiscordCommandTvRequestSuccessFutureSeasons
                    : Language.Current.DiscordCommandTvRequestSuccessSeason.ReplaceTokens(LanguageTokens.SeasonNumber, season.SeasonNumber.ToString());

            return ReplyToUserAsync(message);
        }

        public async Task<bool> GetTvShowRequestConfirmationAsync(TvSeason season)
        {
            var message = season is AllTvSeasons
                ? Language.Current.DiscordCommandTvRequestConfirmAllSeasons
                : season is FutureTvSeasons
                    ? Language.Current.DiscordCommandTvRequestConfirmFutureSeasons
                    : Language.Current.DiscordCommandTvRequestConfirmSeason.ReplaceTokens(LanguageTokens.SeasonNumber, season.SeasonNumber.ToString());

            await _lastCommandMessage?.AddReactionAsync(new Emoji("⬇"));
            await ReplyToUserAsync(message);

            var reaction = await WaitForReactionAsync(Context, _lastCommandMessage, new Emoji("⬇"));

            return reaction != null;
        }

        public async Task<bool> AskForSeasonNotificationRequestAsync(TvShow tvShow, TvSeason requestedSeason)
        {
            var message = Language.Current.DiscordCommandTvNotificationRequestSeason.ReplaceTokens(tvShow, requestedSeason.SeasonNumber);

            if (requestedSeason is FutureTvSeasons)
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

            await _lastCommandMessage?.AddReactionAsync(new Emoji("🔔"));
            await ReplyToUserAsync(message);

            var reaction = await WaitForReactionAsync(Context, _lastCommandMessage, new Emoji("🔔"));

            return reaction != null;
        }

        public Task DisplayNotificationSuccessForSeasonAsync(TvSeason requestedSeason)
        {
            if (requestedSeason is FutureTvSeasons)
            {
                return ReplyToUserAsync(Language.Current.DiscordCommandTvNotificationSuccessFutureSeasons);
            }

            return ReplyToUserAsync(Language.Current.DiscordCommandTvNotificationSuccessSeason.ReplaceTokens(LanguageTokens.SeasonNumber, requestedSeason.SeasonNumber.ToString()));
        }

        public Task DisplayRequestDeniedForSeasonAsync(TvSeason selectedSeason)
        {
            return ReplyToUserAsync(Language.Current.DiscordCommandTvRequestDenied);
        }

        public Task WarnAlreadyNotifiedForSeasonsAsync(TvShow tvShow, TvSeason requestedSeason)
        {
            if (requestedSeason is FutureTvSeasons)
            {
                if (tvShow.AllSeasonsAvailable())
                {
                    return ReplyToUserAsync(Language.Current.DiscordCommandTvRequestAlreadyExistNotifiedFutureSeasonAvailable);
                }
                else if (tvShow.AllSeasonsFullyRequested())
                {
                    return ReplyToUserAsync(Language.Current.DiscordCommandTvRequestAlreadyExistNotifiedFutureSeasonRequested);
                }
                else
                {
                    return ReplyToUserAsync(Language.Current.DiscordCommandTvRequestAlreadyExistNotifiedFutureSeasonFound);
                }
            }

            return ReplyToUserAsync(Language.Current.DiscordCommandTvRequestAlreadyExistNotifiedSeason.ReplaceTokens(tvShow, requestedSeason.SeasonNumber));
        }

        public Task WarnAlreadySeasonAlreadyRequestedAsync(TvShow tvShow, TvSeason requestedSeason)
        {
            if (requestedSeason is FutureTvSeasons)
            {
                if (tvShow.AllSeasonsAvailable())
                {
                    return ReplyToUserAsync(Language.Current.DiscordCommandTvRequestAlreadyExistFutureSeasonAvailable);
                }
                else if (tvShow.AllSeasonsFullyRequested())
                {
                    return ReplyToUserAsync(Language.Current.DiscordCommandTvRequestAlreadyExistFutureSeasonRequested);
                }
                else
                {
                    return ReplyToUserAsync(Language.Current.DiscordCommandTvRequestAlreadyExistFutureSeasonFound);
                }
            }

            return ReplyToUserAsync(Language.Current.DiscordCommandTvRequestAlreadyExistSeason.ReplaceTokens(tvShow, requestedSeason.SeasonNumber));
        }

        public Task WarnShowHasEndedAsync(TvShow tvShow)
        {
            if (tvShow.AllSeasonsAvailable())
            {
                return ReplyToUserAsync(Language.Current.DiscordCommandTvRequestAlreadyAvailableSeries);
            }
            else
            {
                return ReplyToUserAsync(Language.Current.DiscordCommandTvRequestAlreadyExistSeries);
            }
        }

        public Task WarnSeasonAlreadyAvailableAsync(TvSeason requestedSeason)
        {
            return ReplyToUserAsync(Language.Current.DiscordCommandTvRequestAlreadyAvailableSeason.ReplaceTokens(LanguageTokens.SeasonNumber, requestedSeason.SeasonNumber.ToString()));
        }

        public Task WarnShowCannotBeRequestedAsync(TvShow tvShow)
        {
            return ReplyToUserAsync(Language.Current.DiscordCommandTvRequestUnsupported);
        }
    }
}