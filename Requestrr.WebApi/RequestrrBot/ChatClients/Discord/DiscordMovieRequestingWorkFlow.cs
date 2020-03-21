using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Requestrr.WebApi.RequestrrBot.Movies;
using Requestrr.WebApi.RequestrrBot.Notifications;

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
                : base(discord, context)
        {
            _movieSearcher = movieSearcher;
            _movieRequester = movieRequester;
            _notificationRequestRepository = notificationRequestRepository;
            _discordSettings = discordSettingsProvider.Provide();
        }

        public async Task HandleMovieRequestAsync(object[] args)
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
            else if (this.Context.Guild != null && _discordSettings.MovieRoles.Any() && Context.Message.Author is SocketGuildUser guildUser && _discordSettings.MovieRoles.All(r => !guildUser.Roles.Any(ur => r.Equals(ur.Name, StringComparison.InvariantCultureIgnoreCase))))
            {
                await ReplyToUserAsync($"You do not have the required role to request movies, please ask the server owner.");
                return;
            }
            else if (!stringArgs.Any())
            {
                await ReplyToUserAsync($"The correct usage of this command is: ```{_discordSettings.CommandPrefix}{_discordSettings.MovieCommand} name of movie```");
                return;
            }

            var movieName = stringArgs[0].ToString();

            await DeleteSafeAsync(this.Context.Message);

            var workFlow = new MovieRequestingWorkflow(new MovieUserRequester(this.Context.Message.Author.Id.ToString(), $"{this.Context.Message.Author.Username}#{this.Context.Message.Author.Discriminator}"), _movieSearcher, _movieRequester, this, _notificationRequestRepository);
            await workFlow.HandleMovieRequestAsync(movieName);
        }

        public Task WarnNoMovieFoundAsync(string movieName)
        {
            return ReplyToUserAsync($"I could not find any movie with the name \"{movieName}\", please try something different.");
        }

        public Task WarnNoMovieFoundByTheMovieDbIdAsync(string theMovieDbIdTextValue)
        {
            return ReplyToUserAsync($"I could not find any movie with TheMovieDbId of \"{theMovieDbIdTextValue}\", please try something different.");
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

                if (movieRow.Length + embedContent.Length < 1000)
                    embedContent.Append(movieRow.ToString());
            }

            var embedBuilder = new EmbedBuilder()
                .WithTitle("Movie Search")
                .AddField("__Search Results__", embedContent.ToString())
                .WithThumbnailUrl("https://i.imgur.com/44ueTES.png");

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
            return ReplyToUserAsync("I didn't understand your dramatic babbling, I'm afraid you'll have to make a new request.");
        }

        public async Task DisplayMovieDetails(Movie movie)
        {
            var message = Context.Message;
            _lastCommandMessage = await ReplyAsync(string.Empty, false, await GenerateMovieDetailsAsync(movie, message.Author, _movieSearcher));
        }

        public static async Task<Embed> GenerateMovieDetailsAsync(Movie movie, SocketUser user, IMovieSearcher movieSearcher = null)
        {
            var embedBuilder = new EmbedBuilder()
                .WithTitle($"{movie.Title} {(!string.IsNullOrWhiteSpace(movie.ReleaseDate) && movie.ReleaseDate.Length >= 4 ? $"({movie.ReleaseDate.Split("T")[0].Substring(0, 4)})" : string.Empty)}")
                .WithFooter(user.Username, $"https://cdn.discordapp.com/avatars/{user.Id.ToString()}/{user.AvatarId}.png")
                .WithTimestamp(DateTime.Now)
                .WithUrl($"https://www.themoviedb.org/movie/{movie.TheMovieDbId}")
                .WithThumbnailUrl("https://i.imgur.com/44ueTES.png");

            if (!string.IsNullOrWhiteSpace(movie.Overview))
            {
                embedBuilder.WithDescription(movie.Overview.Substring(0, Math.Min(movie.Overview.Length, 255)) + "(...)");
            }

            if (!string.IsNullOrEmpty(movie.PosterPath) && movie.PosterPath.StartsWith("http", StringComparison.InvariantCultureIgnoreCase)) embedBuilder.WithImageUrl(movie.PosterPath);
            if (!string.IsNullOrWhiteSpace(movie.Quality)) embedBuilder.AddField("__Quality__", $"{movie.Quality}p", true);

            if (movieSearcher != null)
            {
                try
                {
                    var details = await movieSearcher.GetMovieDetails(movie.TheMovieDbId);

                    if (!string.IsNullOrWhiteSpace(details.InTheatersDate))
                    {
                        embedBuilder.AddField("__In Theaters__", $"{details.InTheatersDate}", true);
                    }

                    if (!string.IsNullOrWhiteSpace(details.PhysicalReleaseName) && !string.IsNullOrWhiteSpace(details.PhysicalReleaseDate))
                    {
                        embedBuilder.AddField($"__{details.PhysicalReleaseName} Release__", $"{details.PhysicalReleaseDate}", true);
                    }
                }
                catch
                {
                    // Ignore
                }
            }

            if (!string.IsNullOrWhiteSpace(movie.PlexUrl)) embedBuilder.AddField("__Plex__", $"[Watch now]({movie.PlexUrl})", true);
            if (!string.IsNullOrWhiteSpace(movie.EmbyUrl)) embedBuilder.AddField("__Emby__", $"[Watch now]({movie.EmbyUrl})", true);

            return embedBuilder.Build();
        }

        public async Task<bool> GetMovieRequestAsync(Movie movie)
        {
            await _lastCommandMessage?.AddReactionAsync(new Emoji("⬇"));

            if (DateTime.TryParse(movie.ReleaseDate, out var releaseDate))
            {
                if (releaseDate > DateTime.UtcNow)
                {
                    await ReplyToUserAsync("This movie has not released yet, if you want to request this movie please click on the ⬇ reaction directly above this message.");
                }
                else
                {
                    await ReplyToUserAsync("If you want to request this movie please click on the ⬇ reaction directly above this message.");
                }
            }
            else
            {
                await ReplyToUserAsync("If you want to request this movie please click on the ⬇ reaction directly above this message.");
            }

            var reaction = await WaitForReactionAsync(Context, _lastCommandMessage, new Emoji("⬇"));

            return reaction != null;
        }

        public Task WarnMovieAlreadyAvailable()
        {
            return ReplyToUserAsync("This movie is already available, enjoy!");
        }

        public Task DisplayRequestSuccess(Movie movie)
        {
            return ReplyToUserAsync($"Your request for **{movie.Title}** was sent successfully, your movie should be available soon!");
        }

        public async Task<bool> AskForNotificationRequestAsync()
        {
            await _lastCommandMessage?.AddReactionAsync(new Emoji("🔔"));
            await ReplyToUserAsync("This movie has already been requested, you can click on the 🔔 reaction directly above this message to be notified when it becomes available.");

            var reaction = await WaitForReactionAsync(Context, _lastCommandMessage, new Emoji("🔔"));

            return reaction != null;
        }

        public Task DisplayNotificationSuccessAsync(Movie movie)
        {
            return ReplyToUserAsync($"You will now receive a notification as soon as **{movie.Title}** becomes available to watch.");
        }

        public Task DisplayRequestDenied(Movie movie)
        {
            return ReplyToUserAsync($"Your request was denied by the provider due to an insufficient quota limit.");
        }

        public Task WarnMovieUnavailableAndAlreadyHasNotification()
        {
            return ReplyToUserAsync("This movie has already been requested and you will be notified when it becomes available.");
        }
    }
}