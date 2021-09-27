using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Requestrr.WebApi.RequestrrBot.Locale;

namespace Requestrr.WebApi.RequestrrBot.ChatClients.Discord
{
    public class DiscordPingWorkFlow
    {
        private readonly InteractionContext _context;

        public DiscordPingWorkFlow(InteractionContext context)
        {
            _context = context;
        }

        public async Task HandlePingAsync() 
        {
            await _context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral(true).WithContent(Language.Current.DiscordCommandPingResponse));
        }
    }
}