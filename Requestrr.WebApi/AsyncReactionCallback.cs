using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace Requestrr.WebApi
{
    public class AsyncReactionCallback : IReactionCallback
    {
        private readonly SocketCommandContext _context;
        private readonly Func<SocketReaction, Task<bool>>  _callBack;

        public RunMode RunMode => RunMode.Async;

        public SocketCommandContext Context => _context;

        public AsyncReactionCallback(SocketCommandContext Context, Func<SocketReaction, Task<bool>> callBack)
        {
            _context = Context;
            _callBack = callBack;
        }
        
        public Task<bool> HandleCallbackAsync(SocketReaction reaction)
        {
            return _callBack(reaction);
        }
    }
}