using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Requestrr.WebApi.RequestrrBot.Locale;
using Requestrr.WebApi.RequestrrBot.Movies;

namespace Requestrr.WebApi.RequestrrBot.ChatClients.Discord
{
    public class DiscordMovieUserInterface : IMovieUserInterface
    {
        private readonly DiscordInteraction _interactionContext;
        private readonly IMovieSearcher _movieSearcher;

        public DiscordMovieUserInterface(
            DiscordInteraction interactionContext,
            IMovieSearcher movieSearcher)
        {
            _interactionContext = interactionContext;
            _movieSearcher = movieSearcher;
        }

        public async Task ShowMovieSelection(IReadOnlyList<Movie> movies)
        {
            var options = movies.Take(15).Select(x => new DiscordSelectComponentOption(GetFormatedMovieTitle(x), x.TheMovieDbId)).ToList();
            var select = new DiscordSelectComponent($"MRS/{_interactionContext.User.Id}", Language.Current.DiscordCommandMovieRequestHelpDropdown, options);

            await _interactionContext.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddComponents(select).WithContent(Language.Current.DiscordCommandMovieRequestHelp));
        }

        private string GetFormatedMovieTitle(Movie movie)
        {
            var releaseDate = !string.IsNullOrWhiteSpace(movie.ReleaseDate) && movie.ReleaseDate.Length >= 4 ? movie.ReleaseDate.Substring(0, 4) : null;

            if (releaseDate != null)
            {
                return $"{movie.Title} ({releaseDate})";
            }

            return movie.Title;
        }

        public async Task WarnNoMovieFoundAsync(string movieName)
        {
            await _interactionContext.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent(Language.Current.DiscordCommandMovieNotFound.ReplaceTokens(LanguageTokens.MovieTitle, movieName)));
        }

        public async Task WarnNoMovieFoundByTheMovieDbIdAsync(string theMovieDbIdTextValue)
        {
            await _interactionContext.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent(Language.Current.DiscordCommandMovieNotFoundTMDB.ReplaceTokens(LanguageTokens.MovieTMDB, theMovieDbIdTextValue)));
        }

        public async Task DisplayMovieDetailsAsync(Movie movie)
        {
            var message = Language.Current.DiscordCommandMovieRequestConfirm;

            if (DateTime.TryParse(movie.ReleaseDate, out var releaseDate))
            {
                if (releaseDate > DateTime.UtcNow)
                {
                    message = Language.Current.DiscordCommandMovieNotReleased;
                }
                else
                {
                    message = Language.Current.DiscordCommandMovieRequestConfirm;
                }
            }

            var requestButton = new DiscordButtonComponent(ButtonStyle.Primary, $"MRC/{_interactionContext.User.Id}/{movie.TheMovieDbId}", Language.Current.DiscordCommandRequestButton);

            var builder = (await AddPreviousDropdownsAsync(movie, new DiscordWebhookBuilder().AddEmbed(await GenerateMovieDetailsAsync(movie, _movieSearcher)))).AddComponents(requestButton).WithContent(message);
            await _interactionContext.EditOriginalResponseAsync(builder);
        }

        public static async Task<DiscordEmbed> GenerateMovieDetailsAsync(Movie movie, IMovieSearcher movieSearcher = null)
        {
            var embedBuilder = new DiscordEmbedBuilder()
                .WithTitle($"{movie.Title} {(!string.IsNullOrWhiteSpace(movie.ReleaseDate) && movie.ReleaseDate.Length >= 4 ? $"({movie.ReleaseDate.Split("T")[0].Substring(0, 4)})" : string.Empty)}")
                .WithTimestamp(DateTime.Now)
                .WithUrl($"https://www.themoviedb.org/movie/{movie.TheMovieDbId}")
                .WithThumbnail("https://i.imgur.com/44ueTES.png")
                .WithFooter("Powered by Requestrr");

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

        public async Task WarnMovieAlreadyAvailableAsync(Movie movie)
        {
            var requestButton = new DiscordButtonComponent(ButtonStyle.Primary, $"MMM/{_interactionContext.User.Id}/{movie.TheMovieDbId}", Language.Current.DiscordCommandAvailableButton, true);
            var builder = (await AddPreviousDropdownsAsync(movie, new DiscordWebhookBuilder().AddEmbed(await GenerateMovieDetailsAsync(movie, _movieSearcher)))).AddComponents(requestButton).WithContent(Language.Current.DiscordCommandMovieAlreadyAvailable);
            await _interactionContext.EditOriginalResponseAsync(builder);
        }

        public async Task WarnMovieAlreadyRequestedAsync(Movie movie)
        {
            var requestButton = new DiscordButtonComponent(ButtonStyle.Primary, $"MMM/{_interactionContext.User.Id}/{movie.TheMovieDbId}", Language.Current.DiscordCommandRequestedButton, true);
            var builder = (await AddPreviousDropdownsAsync(movie, new DiscordWebhookBuilder().AddEmbed(await GenerateMovieDetailsAsync(movie, _movieSearcher)))).AddComponents(requestButton).WithContent(Language.Current.DiscordCommandMovieRequestAlreadyExist);
            await _interactionContext.EditOriginalResponseAsync(builder);
        }

        public async Task DisplayRequestSuccessAsync(Movie movie)
        {
            var successButton = new DiscordButtonComponent(ButtonStyle.Success, $"0/1/0", Language.Current.DiscordCommandRequestButtonSuccess);

            var builder = (await AddPreviousDropdownsAsync(movie, new DiscordWebhookBuilder().AddEmbed(await GenerateMovieDetailsAsync(movie, _movieSearcher)))).AddComponents(successButton).WithContent(Language.Current.DiscordCommandMovieRequestSuccess.ReplaceTokens(movie));
            await _interactionContext.EditOriginalResponseAsync(builder);
        }

        public async Task AskForNotificationRequestAsync(Movie movie)
        {
            var notifyButton = new DiscordButtonComponent(ButtonStyle.Primary, $"MNR/{_interactionContext.User.Id}/{movie.TheMovieDbId}", Language.Current.DiscordCommandNotifyMe, false, new DiscordComponentEmoji(DiscordEmoji.FromUnicode("🔔")));

            var builder = (await AddPreviousDropdownsAsync(movie, new DiscordWebhookBuilder().AddEmbed(await GenerateMovieDetailsAsync(movie, _movieSearcher)))).AddComponents(notifyButton).WithContent(Language.Current.DiscordCommandMovieNotificationRequest);
            await _interactionContext.EditOriginalResponseAsync(builder);
        }

        public async Task DisplayNotificationSuccessAsync(Movie movie)
        {
            var successButton = new DiscordButtonComponent(ButtonStyle.Success, $"0/1/0", Language.Current.DiscordCommandNotifyMeSuccess);

            var builder = (await AddPreviousDropdownsAsync(movie, new DiscordWebhookBuilder().AddEmbed(await GenerateMovieDetailsAsync(movie, _movieSearcher)))).AddComponents(successButton).WithContent(Language.Current.DiscordCommandMovieNotificationSuccess.ReplaceTokens(movie));
            await _interactionContext.EditOriginalResponseAsync(builder);
        }

        public async Task DisplayRequestDeniedAsync(Movie movie)
        {
            var deniedButton = new DiscordButtonComponent(ButtonStyle.Danger, $"0/1/0", Language.Current.DiscordCommandRequestButtonDenied);

            var builder = (await AddPreviousDropdownsAsync(movie, new DiscordWebhookBuilder().AddEmbed(await GenerateMovieDetailsAsync(movie, _movieSearcher)))).AddComponents(deniedButton).WithContent(Language.Current.DiscordCommandMovieRequestDenied);
            await _interactionContext.EditOriginalResponseAsync(builder);
        }

        public async Task WarnMovieUnavailableAndAlreadyHasNotificationAsync(Movie movie)
        {
            var requestButton = new DiscordButtonComponent(ButtonStyle.Primary, $"MMM/{_interactionContext.User.Id}/{movie.TheMovieDbId}", Language.Current.DiscordCommandRequestedButton, true);

            var builder = (await AddPreviousDropdownsAsync(movie, new DiscordWebhookBuilder().AddEmbed(await GenerateMovieDetailsAsync(movie, _movieSearcher)))).AddComponents(requestButton).WithContent(Language.Current.DiscordCommandMovieRequestAlreadyExistNotified);
            await _interactionContext.EditOriginalResponseAsync(builder);
        }

        private async Task<DiscordWebhookBuilder> AddPreviousDropdownsAsync(Movie movie, DiscordWebhookBuilder builder)
        {
            var previousMovieSelector = (DiscordSelectComponent)(await _interactionContext.GetOriginalResponseAsync()).Components.FirstOrDefault(x => x.Components.OfType<DiscordSelectComponent>().Any())?.Components?.Single();

            if (previousMovieSelector != null)
            {
                var movieSelector = new DiscordSelectComponent(previousMovieSelector.CustomId, GetFormatedMovieTitle(movie), previousMovieSelector.Options);
                builder.AddComponents(movieSelector);
            }

            return builder;
        }
    }
}