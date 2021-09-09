using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Requestrr.WebApi.RequestrrBot.Locale;

namespace Requestrr.WebApi.RequestrrBot.ChatClients.Discord
{
    public class DiscordHelpWorkFlow : RequestrrModuleBase<SocketCommandContext>
    {
        private readonly DiscordSettings _discordSettings;
        private readonly DiscordSocketClient _discord;

        public DiscordHelpWorkFlow(
            SocketCommandContext context,
            DiscordSocketClient discord,
            DiscordSettingsProvider discordSettingsProvider)
                : base(discord, context, discordSettingsProvider)
        {
            _discordSettings = discordSettingsProvider.Provide();
            _discord = discord;
        }

        public async Task HandleHelpAsync()
        {
            if (this.Context.Guild != null && _discordSettings.MonitoredChannels.Any() && _discordSettings.MonitoredChannels.All(c => !Context.Message.Channel.Name.Equals(c, StringComparison.InvariantCultureIgnoreCase)))
            {
                return;
            }

            var message = Language.Current.DiscordCommandHelpMessage.ReplaceTokens(new Dictionary<string, string>
            {
                { LanguageTokens.AuthorUsername, Context.Message.Author.Mention },
                { LanguageTokens.BotUsername, _discord.CurrentUser.Username },
                { LanguageTokens.CommandPrefix, _discordSettings.CommandPrefix },
                { LanguageTokens.TvShowCommand, _discordSettings.TvShowCommand},
                { LanguageTokens.MovieCommand, _discordSettings.MovieCommand },
            });

            if (this.Context.Guild == null)
            {
                var channel = await Context.User.GetOrCreateDMChannelAsync();
                await channel.SendMessageAsync(message);
            }
            else if (_discordSettings.DisplayHelpCommandInDMs)
            {
                try
                {
                    var channel = await Context.User.GetOrCreateDMChannelAsync();
                    await channel.SendMessageAsync(message);
                    await ReplyToUserAsync(Language.Current.DiscordCommandHelpDirectMessageSuccess);
                }
                catch
                {
                    await ReplyToUserAsync(Language.Current.DiscordCommandHelpDirectMessageFailure);
                }
            }
            else
            {
                await ReplyAsync(message);
            }
        }
    }
}