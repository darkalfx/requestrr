using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;

namespace Requestrr.WebApi.RequestrrBot
{
    public class RequireRolesAttribute : SlashCheckBaseAttribute
    {
        public string[] RolesIds { get; set; }

        public RequireRolesAttribute(ulong[] roleIds)
        {
            RolesIds = roleIds.Select(x => x.ToString()).ToArray();
        }

        public override Task<bool> ExecuteChecksAsync(InteractionContext ctx)
        {
            try
            {
                return Task.FromResult(ctx.Guild == null || !RolesIds.Any() || RolesIds.Any(r => ctx.Member.Roles.Any(x => string.Equals(r, x.Id.ToString(), StringComparison.OrdinalIgnoreCase))));
            }
            catch (System.Exception ex)
            {
                ctx.Client.Logger.LogError(ex, "Error while requiring roles for slash commands: " + ex.Message);
                return Task.FromResult(false);
            }
        }
    }
}