using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Requestrr.WebApi.RequestrrBot.Music;
using Requestrr.WebApi.RequestrrBot.Notifications;
using Requestrr.WebApi.RequestrrBot.Notifications.Music;

namespace Requestrr.WebApi.RequestrrBot.ChatClients.Discord
{
    public class DiscordMusicRequestingWorkFlow : RequestrrModuleBase<SocketCommandContext>, IArtistUserInterface
    {
        private IArtistSearcher _artistSearcher;
        private readonly IArtistRequester _artistRequester;
        private readonly ArtistNotificationsRepository _notificationRequestRepository;
        private IUserMessage _lastCommandMessage;
        private readonly DiscordSettings _discordSettings;

        public DiscordMusicRequestingWorkFlow(
            SocketCommandContext context,
            DiscordSocketClient discord,
            IArtistSearcher artistSearcher,
            IArtistRequester artistRequester,
            DiscordSettingsProvider discordSettingsProvider,
            ArtistNotificationsRepository notificationRequestRepository)
                : base(discord, context, discordSettingsProvider)
        {
            _artistSearcher = artistSearcher;
            _artistRequester = artistRequester;
            _notificationRequestRepository = notificationRequestRepository;
            _discordSettings = discordSettingsProvider.Provide();
        }

        public async Task HandleArtistRequestAsync(object[] args)
        {
            var stringArgs = args?.Where(x => !string.IsNullOrWhiteSpace(x?.ToString())).Select(x => x.ToString().Trim()).ToArray() ?? Array.Empty<string>();

            if (!_discordSettings.EnableRequestsThroughDirectMessages && this.Context.Guild == null)
            {
                await ReplyToUserAsync($"This command is only available within a server.");
                return;
            }
            else if (this.Context.Guild != null && _discordSettings.MonitoredChannels.Any() && _discordSettings.MonitoredChannels.All(c => !Context.Message.Channel.Name.Equals(c, StringComparison.InvariantCultureIgnoreCase)))
            {
                return;
            }
            else if (this.Context.Guild != null && _discordSettings.MusicRoles.Any() && Context.Message.Author is SocketGuildUser guildUser && _discordSettings.MusicRoles.All(r => !guildUser.Roles.Any(ur => r.Equals(ur.Name, StringComparison.InvariantCultureIgnoreCase))))
            {
                await ReplyToUserAsync($"You do not have the required role to request movies, please ask the server owner.");
                return;
            }
            else if (!stringArgs.Any())
            {
                await ReplyToUserAsync($"The correct usage of this command is: ```{_discordSettings.CommandPrefix}{_discordSettings.MusicCommand} name of artist```");
                return;
            }

            var IsWebHook = this.Context.Message.Source == MessageSource.Webhook;
            var artistName = stringArgs[0].ToString();

            if (IsWebHook)
            {
                await ForceDeleteSafeAsync(this.Context.Message);
            }
            else
            {
                await DeleteSafeAsync(this.Context.Message);
            }

            IArtistUserInterface userInterface = this;

            if (IsWebHook)
            {
                userInterface = new WebHookMusicUserInterface(this);
            }

            IArtistNotificationWorkflow movieNotificationWorkflow = new DisabledArtistNotificationWorkflow(userInterface);

            if (_discordSettings.NotificationMode != NotificationMode.Disabled && !IsWebHook)
            {
                movieNotificationWorkflow = new ArtistNotificationWorkflow(_notificationRequestRepository, userInterface, _discordSettings.AutomaticallyNotifyRequesters);
            }

            var workFlow = new ArtistRequestingWorkflow(
                new MusicUserRequester(this.Context.Message.Author.Id.ToString(), $"{this.Context.Message.Author.Username}#{this.Context.Message.Author.Discriminator}"),
                _artistSearcher,
                _artistRequester,
                userInterface,
                movieNotificationWorkflow);

            await workFlow.RequestArtistAsync(artistName);
        }

        public Task WarnNoArtistFoundAsync(string movieName)
        {
            return ReplyToUserAsync($"I could not find any artists with the name \"{movieName}\", please try something different.");
        }

        public Task WarnNoArtistFoundByMbIdAsync(string theMovieDbIdTextValue)
        {
            return ReplyToUserAsync($"I could not find any artists with MusicBrainz Id of \"{theMovieDbIdTextValue}\", please try something different.");
        }

        public async Task<ArtistSelection> GetArtistSelectionAsync(IReadOnlyList<Artist> artists)
        {
            var arrayArtists = artists.ToArray();
            var embedContent = new StringBuilder();

            for (int i = 0; i < arrayArtists.Take(10).Count(); i++)
            {
                var artistRow = new StringBuilder();
                artistRow.Append($"{i + 1}) {arrayArtists[i].Name} ");

                if (!string.IsNullOrWhiteSpace(arrayArtists[i].Disambiguation))
                {
                    var disambiguation = $"({arrayArtists[i].Disambiguation})";
                    artistRow.Append(disambiguation);
                }

                artistRow.Append($" [[MusicBrainz](https://musicbrainz.org/artist/{arrayArtists[i].MbId})]");
                artistRow.AppendLine();

                if (artistRow.Length + embedContent.Length < DiscordConstants.MaxEmbedLength)
                    embedContent.Append(artistRow.ToString());
            }

            var embedBuilder = new EmbedBuilder()
                .WithTitle("Artist Search")
                .AddField("__Search Results__", embedContent.ToString())
                .WithThumbnailUrl("https://i.imgur.com/0ZLmDR0.png");

            var reply = await ReplyAsync(string.Empty, false, embedBuilder.Build());
            var replyHelp = await ReplyToUserAsync($"Please select one of the search results shown above by typing its corresponding number shown on the left. (ex: **{_discordSettings.CommandPrefix}2**) To abort type **{_discordSettings.CommandPrefix}cancel**");

            var selectionMessage = await NextMessageAsync(Context);
            var selectionMessageContent = selectionMessage?.Content?.Trim() ?? "cancel";

            selectionMessageContent = !string.IsNullOrWhiteSpace(selectionMessageContent) && !string.IsNullOrWhiteSpace(_discordSettings.CommandPrefix) && selectionMessageContent.StartsWith(_discordSettings.CommandPrefix)
            ? selectionMessageContent.Remove(0, _discordSettings.CommandPrefix.Length)
            : selectionMessageContent;

            if (selectionMessageContent.ToLower().StartsWith("cancel"))
            {
                await DeleteSafeAsync(reply);
                await DeleteSafeAsync(replyHelp);
                await DeleteSafeAsync(selectionMessage);
                await ReplyToUserAsync("Your request has been cancelled!!");

                return new ArtistSelection
                {
                    IsCancelled = true
                };
            }
            else if (int.TryParse(selectionMessageContent, out var selectedMovie) && selectedMovie <= artists.Count)
            {
                await DeleteSafeAsync(reply);
                await DeleteSafeAsync(replyHelp);
                await DeleteSafeAsync(selectionMessage);

                return new ArtistSelection
                {
                    Artist = artists[selectedMovie - 1]
                };
            }

            return new ArtistSelection();
        }

        public Task WarnInvalidArtistSelectionAsync()
        {
            return ReplyToUserAsync("I didn't understand your dramatic babbling, I'm afraid you'll have to make a new request.");
        }

        public async Task DisplayArtistDetailsAsync(Artist artist)
        {
            var message = Context.Message;
            _lastCommandMessage = await ReplyAsync(string.Empty, false, await GenerateArtistDetailsAsync(artist, message.Author, _artistSearcher));
        }

        public static async Task<Embed> GenerateArtistDetailsAsync(Artist artist, SocketUser user = null, IArtistSearcher movieSearcher = null)
        {
            var embedBuilder = new EmbedBuilder()
                .WithTitle($"{artist.Name} {(!string.IsNullOrWhiteSpace(artist.Disambiguation) ? $"({artist.Disambiguation})" : string.Empty)}")
                .WithTimestamp(DateTime.Now)
                .WithUrl($"https://www.themoviedb.org/movie/{artist.MbId}")
                .WithThumbnailUrl("https://i.imgur.com/0ZLmDR0.png");

            if (user != null)
            {
                embedBuilder.WithFooter(user.Username, $"https://cdn.discordapp.com/avatars/{user.Id.ToString()}/{user.AvatarId}.png");
            }

            if (!string.IsNullOrWhiteSpace(artist.Overview))
            {
                embedBuilder.WithDescription(artist.Overview.Substring(0, Math.Min(artist.Overview.Length, 255)) + "(...)");
            }

            if (!string.IsNullOrEmpty(artist.Banner) && artist.Banner.StartsWith("http", StringComparison.InvariantCultureIgnoreCase)) embedBuilder.WithImageUrl(artist.Banner);
            if (!string.IsNullOrWhiteSpace(artist.Quality)) embedBuilder.AddField("__Quality__", $"{artist.Quality}p", true);

            if (!string.IsNullOrWhiteSpace(artist.PlexUrl)) embedBuilder.AddField("__Plex__", $"[Watch now]({artist.PlexUrl})", true);
            if (!string.IsNullOrWhiteSpace(artist.EmbyUrl)) embedBuilder.AddField("__Emby__", $"[Watch now]({artist.EmbyUrl})", true);

            return embedBuilder.Build();
        }

        public async Task<bool> GetArtistRequestAsync(Artist artist)
        {
            await _lastCommandMessage?.AddReactionAsync(new Emoji("⬇"));

            await ReplyToUserAsync("If you want to request this artist please click on the ⬇ reaction directly above this message.");

            var reaction = await WaitForReactionAsync(Context, _lastCommandMessage, new Emoji("⬇"));

            return reaction != null;
        }

        public Task WarnArtistAlreadyAvailableAsync()
        {
            return ReplyToUserAsync("This artist is already available, enjoy!");
        }

        public Task DisplayRequestSuccessAsync(Artist artist)
        {
            return ReplyToUserAsync($"Your request for **{artist.Name}** was sent successfully!");
        }

        public async Task<bool> AskForNotificationRequestAsync()
        {
            await _lastCommandMessage?.AddReactionAsync(new Emoji("🔔"));
            await ReplyToUserAsync("This artist has already been requested, you can click on the 🔔 reaction directly above this message to be notified when it becomes available.");

            var reaction = await WaitForReactionAsync(Context, _lastCommandMessage, new Emoji("🔔"));

            return reaction != null;
        }

        public Task DisplayNotificationSuccessAsync(Artist artist)
        {
            return ReplyToUserAsync($"You will now receive a notification as soon as **{artist.Name}** becomes available to watch.");
        }

        public Task DisplayRequestDeniedAsync(Artist artist)
        {
            return ReplyToUserAsync($"Your request was denied by the provider due to an insufficient quota limit.");
        }

        public Task WarnArtistUnavailableAndAlreadyHasNotificationAsync()
        {
            return ReplyToUserAsync("This artist has already been requested and you will be notified when it becomes available.");
        }

        public Task WarnArtistAlreadyRequestedAsync()
        {
            return ReplyToUserAsync("This artist has already been requested.");
        }
    }
}