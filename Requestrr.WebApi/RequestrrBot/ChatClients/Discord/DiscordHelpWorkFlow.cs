using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

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
                : base(discord, context)
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

            var messageBuilder = new StringBuilder();
            messageBuilder.AppendLine($"Hello {Context.Message.Author.Mention}! I am {_discord.CurrentUser.Username}, your humble Media Bot!");
            messageBuilder.AppendLine();
            messageBuilder.AppendLine("I am very easy to use, please see below for the current available commands:");
            messageBuilder.AppendLine();
            messageBuilder.AppendLine($"The current available commands are:");

            if (!string.IsNullOrWhiteSpace(_discordSettings.TvShowCommand))
            {
                messageBuilder.AppendLine($"**{_discordSettings.CommandPrefix}{_discordSettings.TvShowCommand}**");
            }

            if (!string.IsNullOrWhiteSpace(_discordSettings.MovieCommand))
            {
                messageBuilder.AppendLine($"**{_discordSettings.CommandPrefix}{_discordSettings.MovieCommand}**");
            }

            messageBuilder.AppendLine($"**{_discordSettings.CommandPrefix}help**");
            messageBuilder.AppendLine();
            messageBuilder.AppendLine($"__**Here's how to use a command**__");
            messageBuilder.AppendLine($"``{_discordSettings.CommandPrefix}{_discordSettings.MovieCommand} Deadpool 2``");
            messageBuilder.AppendLine();
            messageBuilder.AppendLine("If you need any additional help with a specific command, simply type the command and I will be glad to help.");
            messageBuilder.AppendLine();
            messageBuilder.AppendLine("If you encounter broken media or are getting errors with me, please notify the server owner.");
            messageBuilder.AppendLine("Feedback is always appreciated, please let the server owner know if I am doing well or not.");

            try
            {
                var channel = await Context.User.GetOrCreateDMChannelAsync();
                await channel.SendMessageAsync(messageBuilder.ToString());

                if(this.Context.Guild != null)
                {
                    await ReplyToUserAsync("I sent you a DM with the help information!");
                }
            }
            catch
            {
                await ReplyToUserAsync("I was unable to send you a DM with the help information, make sure that you haven't blocked me and that you allow direct messages from server members");
            }
        }
    }
}