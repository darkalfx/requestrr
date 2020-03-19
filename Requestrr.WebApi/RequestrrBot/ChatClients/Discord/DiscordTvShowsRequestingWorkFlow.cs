using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Requestrr.WebApi.RequestrrBot.Notifications;
using Requestrr.WebApi.RequestrrBot.TvShows;

namespace Requestrr.WebApi.RequestrrBot.ChatClients.Discord
{
    public class DiscordTvShowsRequestingWorkFlow : RequestrrModuleBase<SocketCommandContext>, ITvShowUserInterface
    {
        private ITvShowSearcher _tvShowSearcher;
        private readonly ITvShowRequester _tvShowRequester;
        private readonly TvShowNotificationsRepository _notificationsRepository;
        private IUserMessage _lastCommandMessage;
        private readonly DiscordSettings _discordSettings;

        public DiscordTvShowsRequestingWorkFlow(
            SocketCommandContext context,
            DiscordSocketClient discord,
            ITvShowSearcher tvShowSearcher,
            ITvShowRequester tvShowRequester,
            DiscordSettingsProvider discordSettingsProvider,
            TvShowNotificationsRepository notificationsRepository)
                : base(discord, context)
        {
            _tvShowSearcher = tvShowSearcher;
            _tvShowRequester = tvShowRequester;
            _notificationsRepository = notificationsRepository;
            _discordSettings = discordSettingsProvider.Provide();
        }

        public async Task HandleTvShowRequestAsync(object[] args)
        {
            var stringArgs = args?.Where(x => !string.IsNullOrWhiteSpace(x?.ToString())).Select(x => x.ToString().Trim()).ToArray() ?? Array.Empty<string>();

            if (!_discordSettings.EnableDirectMessageSupport && this.Context.Guild == null)
            {
                await ReplyToUserAsync($"This command is only available within a server.");
                return;
            }
            else if (this.Context.Guild != null && _discordSettings.MonitoredChannels.Any() && _discordSettings.MonitoredChannels.All(c => !Context.Message.Channel.Name.Equals(c, StringComparison.InvariantCultureIgnoreCase)))
            {
                return;
            }
            else if (this.Context.Guild != null && _discordSettings.TvShowRoles.Any() && Context.Message.Author is SocketGuildUser guildUser && _discordSettings.TvShowRoles.All(r => !guildUser.Roles.Any(ur => r.Equals(ur.Name, StringComparison.InvariantCultureIgnoreCase))))
            {
                await ReplyToUserAsync($"You do not have the required role to request tv shows, please ask the server owner.");
                return;
            }
            else if (!stringArgs.Any())
            {
                await ReplyToUserAsync($"The correct usage of this command is: ```{_discordSettings.CommandPrefix}{_discordSettings.TvShowCommand} name of tv show```");
                return;
            }

            var tvShowName = stringArgs[0].ToString();

            await DeleteSafeAsync(this.Context.Message);

            var workFlow = new TvShowRequestingWorkflow(new TvShowUserRequester(this.Context.Message.Author.Id.ToString(), $"{this.Context.Message.Author.Username}#{this.Context.Message.Author.Discriminator}"), _tvShowSearcher, _tvShowRequester, this, _notificationsRepository);
            await workFlow.HandleTvShowRequestAsync(tvShowName);
        }

        public Task WarnNoTvShowFoundAsync(string tvShowName)
        {
            return ReplyToUserAsync($"I could not find any tv show with the name \"{tvShowName}\", please try something different.");
        }

        public Task WarnNoTvShowFoundByTvDbIdAsync(string tvDbIdTextValue)
        {
            return ReplyToUserAsync($"I could not find any tv show with the TvDbId of \"{tvDbIdTextValue}\", please try something different.");
        }

        public async Task<TvShowSelection> GetTvShowSelectionAsync(IReadOnlyList<SearchedTvShow> searchedTvShows)
        {
            var embedContent = new StringBuilder();

            for (int i = 0; i < searchedTvShows.Take(10).Count(); i++)
            {
                var tvRow = new StringBuilder();
                tvRow.Append($"{i + 1}) {searchedTvShows[i].Title} ");

                if (!string.IsNullOrWhiteSpace(searchedTvShows[i].FirstAired) && searchedTvShows[i].FirstAired.Length >= 4)
                    tvRow.Append($"({searchedTvShows[i].FirstAired.Substring(0, 4)}) ");

                tvRow.Append($"[[TheTVDb](https://www.thetvdb.com/?id={searchedTvShows[i].TheTvDbId}&tab=series)]");
                tvRow.AppendLine();

                if (tvRow.Length + embedContent.Length < 1000)
                    embedContent.Append(tvRow.ToString());
            }

            var embedBuilder = new EmbedBuilder()
                .WithTitle("Tv Show Search")
                .AddField("__Search Results__", embedContent.ToString())
                .WithThumbnailUrl("https://thetvdb.com/images/logo.png");

            var reply = await ReplyAsync(string.Empty, false, embedBuilder.Build());
            var replyHelp = await ReplyToUserAsync($"Please select one of the search results shown above by typing its corresponding numbers shown on the left. (ex: **{_discordSettings.CommandPrefix}2**) To abort type **{_discordSettings.CommandPrefix}cancel**");

            var selectionMessage = await NextMessageAsync(Context);
            var selectionMessageContent = selectionMessage?.Content?.Trim() ?? "cancel";

            selectionMessageContent = !string.IsNullOrWhiteSpace(selectionMessageContent) && !string.IsNullOrWhiteSpace(_discordSettings.CommandPrefix) && selectionMessageContent.StartsWith(_discordSettings.CommandPrefix)
            ? selectionMessageContent.Remove(0, _discordSettings.CommandPrefix.Length)
            : selectionMessageContent;

            if (selectionMessageContent.ToLower().StartsWith("cancel"))
            {
                await DeleteSafeAsync(selectionMessage);
                await DeleteSafeAsync(replyHelp);
                await DeleteSafeAsync(reply);
                await ReplyToUserAsync("Your request has been cancelled!!");

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
            return ReplyToUserAsync("I didn't understand your dramatic babbling, I'm afraid you'll have to make a new request.");
        }

        public async Task DisplayTvShowDetailsAsync(TvShow tvShow)
        {
            var message = Context.Message;
            _lastCommandMessage = await ReplyAsync(string.Empty, false, GenerateTvShowDetailsAsync(tvShow, message.Author));
        }

        public static Embed GenerateTvShowDetailsAsync(TvShow tvShow, SocketUser user)
        {
            var title = tvShow.Title;

            if (!string.IsNullOrWhiteSpace(tvShow.FirstAired))
            {
                if(tvShow.FirstAired.Length >= 4 && !title.Contains(tvShow.FirstAired.Split("T")[0].Substring(0, 4)))
                {
                    title = $"{title} ({tvShow.FirstAired.Split("T")[0].Substring(0, 4)})";
                }
            }

            var embedBuilder = new EmbedBuilder()
                .WithTitle(title)
                .WithFooter(user.Username, $"https://cdn.discordapp.com/avatars/{user.Id}/{user.AvatarId}.png")
                .WithTimestamp(DateTime.Now)
                .WithUrl($"https://www.thetvdb.com/?id={tvShow.TheTvDbId}&tab=series")
                .WithThumbnailUrl("https://thetvdb.com/images/logo.png");

            if(!string.IsNullOrWhiteSpace(tvShow.Overview))
            {
                embedBuilder.WithDescription(tvShow.Overview.Substring(0, Math.Min(tvShow.Overview.Length, 255)) + "(...)");
            }

            if (!string.IsNullOrEmpty(tvShow.Banner) && tvShow.Banner.StartsWith("http", StringComparison.InvariantCultureIgnoreCase)) embedBuilder.WithImageUrl(tvShow.Banner);
            if (!string.IsNullOrWhiteSpace(tvShow.Network)) embedBuilder.AddField("__Network__", tvShow.Network, true);
            if (!string.IsNullOrWhiteSpace(tvShow.Status)) embedBuilder.AddField("__Status__", tvShow.Status, true);
            if (!string.IsNullOrWhiteSpace(tvShow.Quality)) embedBuilder.AddField("__Quality__", $"{tvShow.Quality}p", true);
            if (!string.IsNullOrWhiteSpace(tvShow.PlexUrl)) embedBuilder.AddField("__Plex__", $"[Watch now]({tvShow.PlexUrl})", true);
            if (!string.IsNullOrWhiteSpace(tvShow.EmbyUrl)) embedBuilder.AddField("__Emby__", $"[Watch now]({tvShow.EmbyUrl})", true);

            return embedBuilder.Build();
        }

        public async Task<TvSeasonsSelection> GetTvShowSeasonSelectionAsync(TvShow tvShow)
        {
            var fieldContent = string.Empty;
            var tvShowSeasons = tvShow.Seasons;

            foreach (var season in tvShowSeasons)
            {
                var seasonName = season is AllTvSeasons
                    ? $"{season.SeasonNumber}) - All seasons"
                    : season is FutureTvSeasons
                        ? $"{season.SeasonNumber}) - Future seasons"
                        : $"{season.SeasonNumber}) - Season {season.SeasonNumber}";

                fieldContent += seasonName;
                fieldContent += season.IsRequested == RequestedState.Full ? " ✅" : season.IsRequested == RequestedState.Partial ? " ☑" : string.Empty;
                fieldContent += System.Environment.NewLine;
            }

            var descriptionBuilder = new StringBuilder();
            descriptionBuilder.AppendLine("✅ - Fully requested");
            descriptionBuilder.AppendLine("☑ - Partially requested");
            descriptionBuilder.AppendLine();

            var embedBuilder = new EmbedBuilder()
                .WithTitle($"{tvShow.Title} Seasons")
                .AddField("__Seasons__", fieldContent)
                .WithDescription(descriptionBuilder.ToString())
                .WithThumbnailUrl("https://thetvdb.com/images/logo.png");

            var reply = await ReplyAsync(string.Empty, false, embedBuilder.Build());
            var replyHelp = await ReplyToUserAsync($"Please select one of the available seasons shown above by typing its corresponding number shown on the left. (ex: **{_discordSettings.CommandPrefix}2**) To abort type **{_discordSettings.CommandPrefix}cancel**");
            var selectionMessage = await NextMessageAsync(Context);
            var selectionMessageContent = selectionMessage?.Content?.Trim() ?? "cancel";

            selectionMessageContent = !string.IsNullOrWhiteSpace(selectionMessageContent) && !string.IsNullOrWhiteSpace(_discordSettings.CommandPrefix) && selectionMessageContent.StartsWith(_discordSettings.CommandPrefix)
            ? selectionMessageContent.Remove(0, _discordSettings.CommandPrefix.Length)
            : selectionMessageContent;

            if (selectionMessageContent.ToLower().StartsWith("cancel"))
            {
                await DeleteSafeAsync(selectionMessage);
                await DeleteSafeAsync(replyHelp);
                await DeleteSafeAsync(reply);
                await ReplyToUserAsync("Your request has been cancelled!!");

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
            return ReplyToUserAsync("I didn't understand your dramatic babbling, I'm afraid you'll have to make a new request.");
        }

        public Task DisplayRequestSuccessForSeasonAsync(TvSeason season)
        {
            var message = season is AllTvSeasons
                ? $"Your request for **all seasons** was sent successfully, they should be available soon!"
                : season is FutureTvSeasons
                    ? $"Your request for **future seasons** was sent successfully, you will be notified when they become available."
                    : $"Your request for season **{season.SeasonNumber}** was sent successfully, it should be available soon!";

            return ReplyToUserAsync(message);
        }

        public async Task<bool> GetTvShowRequestConfirmation(TvSeason season)
        {
            var seasonName = season is AllTvSeasons
                ? "all seasons"
                : season is FutureTvSeasons
                    ? "future seasons"
                    : $"season {season.SeasonNumber}";

            await _lastCommandMessage?.AddReactionAsync(new Emoji("⬇"));
            await ReplyToUserAsync($"If you want to request **{seasonName}** of this tv show please click on the ⬇ reaction directly above this message.");

            var reaction = await WaitForReactionAsync(Context, _lastCommandMessage, new Emoji("⬇"));

            return reaction != null;
        }

        public async Task<bool> AskForSeasonNotificationRequestAsync(TvShow tvShow, TvSeason requestedSeason)
        {
            var message = $"Season {requestedSeason.SeasonNumber} has already been requested, you can click on the 🔔 reaction directly above this message to be notified when it becomes available.";

            if (requestedSeason is FutureTvSeasons)
            {
                if (tvShow.AllSeasonsAvailable())
                {
                    message = $"All seasons are already available, you can click on the 🔔 reaction directly above this message to be notified when future seasons becomes available.";
                }
                else if (tvShow.AllSeasonsFullyRequested())
                {
                    message = $"All seasons have been already requested, you can click on the 🔔 reaction directly above this message to be notified when future seasons becomes available.";
                }
                else
                {
                    message = $"Future seasons have already been requested, you can click on the 🔔 reaction directly above this message to be notified when future seasons becomes available.";
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
                return ReplyToUserAsync($"You will now receive a notification as soon as any **future seasons** becomes available to watch.");
            }

            return ReplyToUserAsync($"You will now receive a notification as soon as **season {requestedSeason.SeasonNumber}** becomes available to watch.");
        }

        public Task DisplayRequestDeniedForSeasonAsync(TvSeason selectedSeason)
        {
            return ReplyToUserAsync($"Your request was denied by the provider due to an insufficient quota limit.");
        }

        public Task WarnAlreadyNotifiedForSeasonsAsync(TvShow tvShow, TvSeason requestedSeason)
        {
            if (requestedSeason is FutureTvSeasons)
            {
                if (tvShow.AllSeasonsAvailable())
                {
                    return ReplyToUserAsync($"All seasons are available and you will be notified when new seasons become available.");
                }
                else if (tvShow.AllSeasonsFullyRequested())
                {
                    return ReplyToUserAsync($"All seasons have already been requested and you will be notified when new seasons become available.");
                }
                else
                {
                    return ReplyToUserAsync($"Future seasons have already been requested and you will be notified when they becomes available.");
                }
            }

            return ReplyToUserAsync($"Season {requestedSeason.SeasonNumber} has already been requested and you will be notified when it becomes available.");
        }

        public Task WarnShowHasEnded(TvShow tvShow)
        {
            return ReplyToUserAsync($"This show has ended, and all seasons {(tvShow.AllSeasonsAvailable() ? "are available" : "have been requested")}.");
        }

        public Task WarnSeasonAlreadyAvailable(TvSeason requestedSeason)
        {
            return ReplyToUserAsync($"**Season {requestedSeason.SeasonNumber}** is already available, enjoy!");
        }

        public Task WarnShowCannotBeRequested(TvShow tvShow)
        {
            return ReplyToUserAsync($"This show cannot be automatically requested, please ask the server owner to manually add it.");
        }
    }
}