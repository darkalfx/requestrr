using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Requestrr.WebApi
{
    public class RequestrrModuleBase<T> : IDisposable
    {
        private readonly DiscordSocketClient _discord;
        private Dictionary<ulong, IReactionCallback> _callbacks = new Dictionary<ulong, IReactionCallback>();

        protected SocketCommandContext Context { get; }

        public RequestrrModuleBase(DiscordSocketClient discord, SocketCommandContext commandContext)
        {
            _discord = discord;
            Context = commandContext;
            _discord.ReactionAdded += HandleReactionAsync;
        }

        public void Dispose()
        {
            _discord.ReactionAdded -= HandleReactionAsync;
        }

        protected async Task DeleteSafeAsync(IMessage message, RequestOptions options = null)
        {
            try
            {
                if (message != null)
                {
                    if (!(message.Channel is IPrivateChannel))
                    {
                        await message.DeleteAsync();
                    }
                }
            }
            catch
            {
                //We did our best
            }
        }

        protected Task<IUserMessage> ReplyToUserAsync(string message)
        {
            return ReplyAsync($"{Context.Message.Author.Mention} " + message);
        }

        protected async Task<IUserMessage> ReplyAsync(string message = null, bool isTTS = false, Embed embed = null, RequestOptions options = null)
        {
            return await Context.Channel.SendMessageAsync(message, isTTS, embed, options).ConfigureAwait(false);
        }

        public async Task<SocketReaction> WaitForReactionAsync(SocketCommandContext context, IMessage message, IEmote expectedEmote)
        {
            var token = new CancellationToken();
            var timeout = TimeSpan.FromSeconds(120);

            var eventTrigger = new TaskCompletionSource<SocketReaction>();
            var cancelTrigger = new TaskCompletionSource<bool>();

            token.Register(() => cancelTrigger.SetResult(true));

            var sourceContext = context;

            AddReactionCallback(message, new AsyncReactionCallback(sourceContext, reaction =>
            {
                if (reaction.Emote.Name.Equals(expectedEmote.Name, StringComparison.Ordinal))
                {
                    eventTrigger.SetResult(reaction);
                    return Task.FromResult(true);
                }

                return Task.FromResult(false);
            }));

            try
            {
                var trigger = eventTrigger.Task;
                var cancel = cancelTrigger.Task;
                var delay = Task.Delay(timeout);
                var task = await Task.WhenAny(trigger, delay, cancel).ConfigureAwait(false);

                if (task == trigger)
                    return await trigger.ConfigureAwait(false);
                else
                    return null;
            }
            catch
            {
                return null;
            }
            finally
            {
                RemoveReactionCallback(message);
            }
        }

        public void AddReactionCallback(IMessage message, IReactionCallback callback)
            => _callbacks[message.Id] = callback;
        public void RemoveReactionCallback(IMessage message)
            => RemoveReactionCallback(message.Id);
        public void RemoveReactionCallback(ulong id)
            => _callbacks.Remove(id);
        public void ClearReactionCallbacks()
            => _callbacks.Clear();

        private async Task HandleReactionAsync(Cacheable<IUserMessage, ulong> message,
            ISocketMessageChannel channel,
            SocketReaction reaction)
        {
            if (reaction.UserId == _discord.CurrentUser.Id) return;
            if (!(_callbacks.TryGetValue(message.Id, out var callback))) return;
            if (!(callback.Context.Message.Author.Id == reaction.UserId && callback.Context.Channel.Id == reaction.Channel.Id))
                return;

            switch (callback.RunMode)
            {
                case RunMode.Async:
                    _ = Task.Run(async () =>
                    {
                        if (await callback.HandleCallbackAsync(reaction).ConfigureAwait(false))
                            RemoveReactionCallback(message.Id);
                    });
                    break;
                default:
                    if (await callback.HandleCallbackAsync(reaction).ConfigureAwait(false))
                        RemoveReactionCallback(message.Id);
                    break;
            }
        }

        protected async Task<SocketMessage> NextMessageAsync(
            SocketCommandContext context,
            TimeSpan? timeout = null,
            CancellationToken token = default)
        {
            timeout = timeout ?? TimeSpan.FromSeconds(120);

            var eventTrigger = new TaskCompletionSource<SocketMessage>();
            var cancelTrigger = new TaskCompletionSource<bool>();

            token.Register(() => cancelTrigger.SetResult(true));

            var sourceContext = context;

            async Task Handler(SocketMessage message)
            {
                if (sourceContext.Message.Author.Id == message.Author.Id && sourceContext.Channel.Id == message.Channel.Id)
                    eventTrigger.SetResult(message);
            }

            try
            {
                context.Client.MessageReceived += Handler;

                var trigger = eventTrigger.Task;
                var cancel = cancelTrigger.Task;
                var delay = Task.Delay(timeout.Value);
                var task = await Task.WhenAny(trigger, delay, cancel).ConfigureAwait(false);

                context.Client.MessageReceived -= Handler;

                if (task == trigger)
                    return await trigger.ConfigureAwait(false);
                else
                    return null;
            }
            catch
            {
                return null;
            }
        }
    }
}