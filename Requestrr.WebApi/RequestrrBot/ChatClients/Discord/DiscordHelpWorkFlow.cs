using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Requestrr.WebApi.RequestrrBot.Locale;

namespace Requestrr.WebApi.RequestrrBot.ChatClients.Discord
{
    public class DiscordHelpWorkFlow
    {
        private readonly DiscordSettings _discordSettings;
        private readonly DiscordClient _discordClient;
        private readonly InteractionContext _context;

        public DiscordHelpWorkFlow(
            DiscordClient discordClient,
            InteractionContext context,
            DiscordSettingsProvider discordSettingsProvider)
        {
            _discordSettings = discordSettingsProvider.Provide();
            _discordClient = discordClient;
            _context = context;
        }
        public async Task HandleHelpAsync()
        {
            var message = Language.Current.DiscordCommandHelpMessage.ReplaceTokens(new Dictionary<string, string>
            {
                { LanguageTokens.AuthorUsername, _context.User.Mention },
                { LanguageTokens.BotUsername, _discordClient.CurrentUser.Username },
                { LanguageTokens.CommandPrefix, "/" },
                { LanguageTokens.MovieCommandTitle, $"{Language.Current.DiscordCommandRequestGroupName.ToLower()} {Language.Current.DiscordCommandMovieRequestTitleName.ToLower()}" },
                { LanguageTokens.MovieCommandTmDb, $"{Language.Current.DiscordCommandRequestGroupName.ToLower()} {Language.Current.DiscordCommandMovieRequestTmbdName.ToLower()}" },
                { LanguageTokens.TvShowCommandTitle, $"{Language.Current.DiscordCommandRequestGroupName.ToLower()} {Language.Current.DiscordCommandTvRequestTitleName.ToLower()}" },
                { LanguageTokens.TvShowCommandTvDb, $"{Language.Current.DiscordCommandRequestGroupName.ToLower()} {Language.Current.DiscordCommandTvRequestTvdbName.ToLower()}" },
            });

            await _context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral(true).WithContent(message));
        }
    }
}