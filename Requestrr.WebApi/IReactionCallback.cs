using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace Requestrr.WebApi
{
    public interface IReactionCallback
    {
        RunMode RunMode { get; }
        SocketCommandContext Context { get; }

        Task<bool> HandleCallbackAsync(SocketReaction reaction);
    }
}