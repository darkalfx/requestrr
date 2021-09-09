using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Requestrr.WebApi.RequestrrBot.Locale;
using Requestrr.WebApi.RequestrrBot.Movies;
using Requestrr.WebApi.RequestrrBot.Notifications;
using Requestrr.WebApi.RequestrrBot.Notifications.Movies;

namespace Requestrr.WebApi.RequestrrBot.ChatClients.Discord
{
    public class DiscordMovieRequestingWorkFlow : RequestrrModuleBase<SocketCommandContext>, IMovieUserInterface
    {
        private IMovieSearcher _movieSearcher;
        private readonly IMovieRequester _movieRequester;
        private readonly MovieNotificationsRepository _notificationRequestRepository;
        private IUserMessage _lastCommandMessage;
        private readonly DiscordSettings _discordSettings;

        public DiscordMovieRequestingWorkFlow(
            SocketCommandContext context,
            DiscordSocketClient discord,
            IMovieSearcher movieSearcher,
            IMovieRequester movieRequester,
            DiscordSettingsProvider discordSettingsProvider,
            MovieNotificationsRepository notificationRequestRepository)
                : base(discord, context, discordSettingsProvider)
        {
            _movieSearcher = movieSearcher;
            _movieRequester = movieRequester;
            _notificationRequestRepository = notificationRequestRepository;
            _discordSettings = discordSettingsProvider.Provide();
        }

        public async Task HandleMovieRequestAsync(object[] args)
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
            else if (this.Context.Guild != null && _discordSettings.MovieRoles.Any() && Context.Message.Author is SocketGuildUser guildUser && _discordSettings.MovieRoles.All(r => !guildUser.Roles.Any(ur => r.Equals(ur.Name, StringComparison.InvariantCultureIgnoreCase))))
            {
                await ReplyToUserAsync(Language.Current.DiscordCommandMovieMissingRoles);
                return;
            }
            else if (!stringArgs.Any())
            {
                await ReplyToUserAsync(Language.Current.DiscordCommandMovieInvalidArguments.ReplaceTokens(new Dictionary<string, string>
                {
                    { LanguageTokens.CommandPrefix, _discordSettings.CommandPrefix },
                    { LanguageTokens.MovieCommand, _discordSettings.MovieCommand }
                }));

                return;
            }

            var IsWebHook = this.Context.Message.Source == MessageSource.Webhook;
            var movieName = stringArgs[0].ToString();

            if (IsWebHook)
            {
                await ForceDeleteSafeAsync(this.Context.Message);
            }
            else
            {
                await DeleteSafeAsync(this.Context.Message);
            }

            IMovieUserInterface userInterface = this;

            if (IsWebHook)
            {
                userInterface = new WebHookMovieUserInterface(this);
            }

            IMovieNotificationWorkflow movieNotificationWorkflow = new DisabledMovieNotificationWorkflow(userInterface);

            if (_discordSettings.NotificationMode != NotificationMode.Disabled && !IsWebHook)
            {
                movieNotificationWorkflow = new MovieNotificationWorkflow(_notificationRequestRepository, userInterface, _discordSettings.AutomaticallyNotifyRequesters);
            }

            var workFlow = new MovieRequestingWorkflow(
                new MovieUserRequester(this.Context.Message.Author.Id.ToString(), $"{this.Context.Message.Author.Username}#{this.Context.Message.Author.Discriminator}"),
                _movieSearcher,
                _movieRequester,
                userInterface,
                movieNotificationWorkflow);

            await workFlow.RequestMovieAsync(movieName);
        }

        public Task WarnNoMovieFoundAsync(string movieName)
        {
            return ReplyToUserAsync(Language.Current.DiscordCommandMovieNotFound.ReplaceTokens(LanguageTokens.MovieTitle, movieName));
        }

        public Task WarnNoMovieFoundByTheMovieDbIdAsync(string theMovieDbIdTextValue)
        {
            return ReplyToUserAsync(Language.Current.DiscordCommandMovieNotFoundTMDB.ReplaceTokens(LanguageTokens.MovieTMDB, theMovieDbIdTextValue));
        }

        public async Task<MovieSelection> GetMovieSelectionAsync(IReadOnlyList<Movie> movies)
        {
            var arrayMovies = movies.ToArray();
            var embedContent = new StringBuilder();

            for (int i = 0; i < arrayMovies.Take(10).Count(); i++)
            {
                var movieRow = new StringBuilder();
                movieRow.Append($"{i + 1}) {arrayMovies[i].Title} ");

                if (!string.IsNullOrWhiteSpace(arrayMovies[i].ReleaseDate) && arrayMovies[i].ReleaseDate.Length >= 4)
                    movieRow.Append($"({arrayMovies[i].ReleaseDate.Substring(0, 4)}) ");

                movieRow.Append($"[[TheMovieDb](https://www.themoviedb.org/movie/{arrayMovies[i].TheMovieDbId})]");
                movieRow.AppendLine();

                if (movieRow.Length + embedContent.Length < DiscordConstants.MaxEmbedLength)
                    embedContent.Append(movieRow.ToString());
            }

            var embedBuilder = new EmbedBuilder()
                .WithTitle(Language.Current.DiscordEmbedMovieSearch)
                .AddField($"__{Language.Current.DiscordEmbedMovieSearchResult}__", embedContent.ToString())
                .WithThumbnailUrl("https://i.imgur.com/44ueTES.png");

            var reply = await ReplyAsync(string.Empty, false, embedBuilder.Build());
            var replyHelp = await ReplyToUserAsync(Language.Current.DiscordCommandMovieRequestHelp.ReplaceTokens(LanguageTokens.CommandPrefix, _discordSettings.CommandPrefix));

            var selectionMessage = await NextMessageAsync(Context);
            var selectionMessageContent = selectionMessage?.Content?.Trim() ?? Language.Current.DiscordCommandMovieCancelCommand.ToLower();

            selectionMessageContent = !string.IsNullOrWhiteSpace(selectionMessageContent) && !string.IsNullOrWhiteSpace(_discordSettings.CommandPrefix) && selectionMessageContent.StartsWith(_discordSettings.CommandPrefix)
            ? selectionMessageContent.Remove(0, _discordSettings.CommandPrefix.Length)
            : selectionMessageContent;

            if (selectionMessageContent.ToLower().StartsWith(Language.Current.DiscordCommandMovieCancelCommand.ToLower()))
            {
                await DeleteSafeAsync(reply);
                await DeleteSafeAsync(replyHelp);
                await DeleteSafeAsync(selectionMessage);
                await ReplyToUserAsync(Language.Current.DiscordCommandMovieRequestCancelled);

                return new MovieSelection
                {
                    IsCancelled = true
                };
            }
            else if (int.TryParse(selectionMessageContent, out var selectedMovie) && selectedMovie <= movies.Count)
            {
                await DeleteSafeAsync(reply);
                await DeleteSafeAsync(replyHelp);
                await DeleteSafeAsync(selectionMessage);

                return new MovieSelection
                {
                    Movie = movies[selectedMovie - 1]
                };
            }

            return new MovieSelection();
        }

        public Task WarnInvalidMovieSelectionAsync()
        {
            return ReplyToUserAsync(Language.Current.DiscordCommandMovieRequestInvalid);
        }

        public async Task DisplayMovieDetailsAsync(Movie movie)
        {
            var message = Context.Message;
            _lastCommandMessage = await ReplyAsync(string.Empty, false, await GenerateMovieDetailsAsync(movie, message.Author, _movieSearcher));
        }

        public static async Task<Embed> GenerateMovieDetailsAsync(Movie movie, SocketUser user = null, IMovieSearcher movieSearcher = null)
        {
            var embedBuilder = new EmbedBuilder()
                .WithTitle($"{movie.Title} {(!string.IsNullOrWhiteSpace(movie.ReleaseDate) && movie.ReleaseDate.Length >= 4 ? $"({movie.ReleaseDate.Split("T")[0].Substring(0, 4)})" : string.Empty)}")
                .WithTimestamp(DateTime.Now)
                .WithUrl($"https://www.themoviedb.org/movie/{movie.TheMovieDbId}")
                .WithThumbnailUrl("https://i.imgur.com/44ueTES.png");

            if (user != null)
            {
                embedBuilder.WithFooter(user.Username, $"https://cdn.discordapp.com/avatars/{user.Id.ToString()}/{user.AvatarId}.png");
            }

            if (!string.IsNullOrWhiteSpace(movie.Overview))
            {
                embedBuilder.WithDescription(movie.Overview.Substring(0, Math.Min(movie.Overview.Length, 255)) + "(...)");
            }

            if (!string.IsNullOrEmpty(movie.PosterPath) && movie.PosterPath.StartsWith("http", StringComparison.InvariantCultureIgnoreCase)) embedBuilder.WithImageUrl(movie.PosterPath);
            if (!string.IsNullOrWhiteSpace(movie.Quality)) embedBuilder.AddField($"__{Language.Current.DiscordEmbedMovieQuality}__", $"{movie.Quality}p", true);

            if (movieSearcher != null)
            {
                try
                {
                    var details = await movieSearcher.GetMovieDetails(movie.TheMovieDbId);

                    if (!string.IsNullOrWhiteSpace(details.InTheatersDate))
                    {
                        embedBuilder.AddField($"__{Language.Current.DiscordEmbedMovieInTheaters}__", $"{details.InTheatersDate}", true);
                    }
                    else if (!string.IsNullOrWhiteSpace(movie.ReleaseDate))
                    {
                        embedBuilder.AddField($"__{Language.Current.DiscordEmbedMovieInTheaters}__", $"{DateTime.Parse(movie.ReleaseDate).ToString("MMMM dd yyyy", DateTimeFormatInfo.InvariantInfo)}", true);
                    }

                    if (!string.IsNullOrWhiteSpace(details.PhysicalReleaseName) && !string.IsNullOrWhiteSpace(details.PhysicalReleaseDate))
                    {
                        embedBuilder.AddField($"__{details.PhysicalReleaseName} {Language.Current.DiscordEmbedMovieRelease}__", $"{details.PhysicalReleaseDate}", true);
                    }
                }
                catch
                {
                    // Ignore
                }
            }

            if (!string.IsNullOrWhiteSpace(movie.PlexUrl)) embedBuilder.AddField($"__Plex__", $"[{Language.Current.DiscordEmbedMovieWatchNow}]({movie.PlexUrl})", true);
            if (!string.IsNullOrWhiteSpace(movie.EmbyUrl)) embedBuilder.AddField($"__Emby__", $"[{Language.Current.DiscordEmbedMovieWatchNow}]({movie.EmbyUrl})", true);

            return embedBuilder.Build();
        }

        public async Task<bool> GetMovieRequestAsync(Movie movie)
        {
            await _lastCommandMessage?.AddReactionAsync(new Emoji("⬇"));

            if (DateTime.TryParse(movie.ReleaseDate, out var releaseDate))
            {
                if (releaseDate > DateTime.UtcNow)
                {
                    await ReplyToUserAsync(Language.Current.DiscordCommandMovieNotReleased);
                }
                else
                {
                    await ReplyToUserAsync(Language.Current.DiscordCommandMovieRequestConfirm);
                }
            }
            else
            {
                await ReplyToUserAsync(Language.Current.DiscordCommandMovieRequestConfirm);
            }

            var reaction = await WaitForReactionAsync(Context, _lastCommandMessage, new Emoji("⬇"));

            return reaction != null;
        }

        public Task WarnMovieAlreadyAvailableAsync()
        {
            return ReplyToUserAsync(Language.Current.DiscordCommandMovieAlreadyAvailable);
        }

        public Task DisplayRequestSuccessAsync(Movie movie)
        {
            return ReplyToUserAsync(Language.Current.DiscordCommandMovieRequestSuccess.ReplaceTokens(movie));
        }

        public async Task<bool> AskForNotificationRequestAsync()
        {
            await _lastCommandMessage?.AddReactionAsync(new Emoji("🔔"));
            await ReplyToUserAsync(Language.Current.DiscordCommandMovieNotificationRequest);

            var reaction = await WaitForReactionAsync(Context, _lastCommandMessage, new Emoji("🔔"));

            return reaction != null;
        }

        public Task DisplayNotificationSuccessAsync(Movie movie)
        {
            return ReplyToUserAsync(Language.Current.DiscordCommandMovieNotificationSuccess.ReplaceTokens(movie));
        }

        public Task DisplayRequestDeniedAsync(Movie movie)
        {
            return ReplyToUserAsync(Language.Current.DiscordCommandMovieRequestDenied);
        }

        public Task WarnMovieUnavailableAndAlreadyHasNotificationAsync()
        {
            return ReplyToUserAsync(Language.Current.DiscordCommandMovieRequestAlreadyExistNotified);
        }

        public Task WarnMovieAlreadyRequestedAsync()
        {
            return ReplyToUserAsync(Language.Current.DiscordCommandMovieRequestAlreadyExist);
        }
    }
}