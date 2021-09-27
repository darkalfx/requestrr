using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;

namespace Requestrr.WebApi.RequestrrBot
{
    public class RequireChannelsAttribute : SlashCheckBaseAttribute
    {
        public string[] ChannelIds { get; set; }

        public RequireChannelsAttribute(ulong[] channelIds)
        {
            ChannelIds = channelIds.Select(x => x.ToString()).ToArray();
        }

        public override Task<bool> ExecuteChecksAsync(InteractionContext ctx)
        {
            try
            {
                return Task.FromResult(ctx.Guild == null || !ChannelIds.Any() || ChannelIds.Any(x => string.Equals(x, ctx.Channel.Id.ToString(), StringComparison.OrdinalIgnoreCase)));
            }
            catch (System.Exception ex)
            {
                ctx.Client.Logger.LogError(ex, "[Error while requiring channels for slash commands: ]" + ex.Message);
                return Task.FromResult(false);
            }
        }
    }
}