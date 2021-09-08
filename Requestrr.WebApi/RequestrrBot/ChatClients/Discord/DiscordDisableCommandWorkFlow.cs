using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace Requestrr.WebApi.RequestrrBot.ChatClients.Discord
{
    public class DiscordDisableCommandWorkFlow : RequestrrModuleBase<SocketCommandContext>
    {
        private readonly DiscordSettings _discordSettings;

        public DiscordDisableCommandWorkFlow(
            SocketCommandContext context,
            DiscordSocketClient discord,
            DiscordSettingsProvider discordSettingsProvider)
                : base(discord, context, discordSettingsProvider)
        {
            _discordSettings = discordSettingsProvider.Provide();
        }

        public Task HandleDisabledCommandAsync()
        {
            if (!_discordSettings.EnableRequestsThroughDirectMessages && this.Context.Guild == null)
            {
                return ReplyToUserAsync(Program.LocalizedStrings.OnlyAvailableWithinServer.ToString());
            }
            else if (this.Context.Guild != null && _discordSettings.MonitoredChannels.Any() && _discordSettings.MonitoredChannels.All(c => !Context.Message.Channel.Name.Equals(c, StringComparison.InvariantCultureIgnoreCase)))
            {
                return Task.CompletedTask;
            }

            return ReplyToUserAsync(Program.LocalizedStrings.DisabledCommand.ToString());
        }
    }
}