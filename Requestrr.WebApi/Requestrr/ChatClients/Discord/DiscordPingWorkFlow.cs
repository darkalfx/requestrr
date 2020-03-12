using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Requestrr.WebApi.Requestrr.ChatClients
{
    public class DiscordPingWorkFlow : RequestrrModuleBase<SocketCommandContext>
    {
        private readonly DiscordSettings _discordSettings;

        public DiscordPingWorkFlow(
            SocketCommandContext context,
            DiscordSocketClient discord,
            DiscordSettingsProvider discordSettingsProvider)
                : base(discord, context)
        {
            _discordSettings = discordSettingsProvider.Provide();
        }

        public Task HandlePingAsync() 
        {
            if (!_discordSettings.EnableDirectMessageSupport && this.Context.Guild == null)
            {
                return ReplyToUserAsync($"This command is only available within a server.");
            }
            else if (this.Context.Guild != null && _discordSettings.MonitoredChannels.Any() && _discordSettings.MonitoredChannels.All(c => !Context.Message.Channel.Name.Equals(c, StringComparison.InvariantCultureIgnoreCase)))
            {
                return Task.CompletedTask;
            }

            return ReplyToUserAsync($"pong");
        }
    }
}