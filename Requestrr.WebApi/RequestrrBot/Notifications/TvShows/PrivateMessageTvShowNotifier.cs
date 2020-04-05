using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Requestrr.WebApi.RequestrrBot.ChatClients.Discord;
using Requestrr.WebApi.RequestrrBot.TvShows;

namespace Requestrr.WebApi.RequestrrBot.Notifications.TvShows
{
    public class PrivateMessageTvShowNotifier : ITvShowNotifier
    {
        private readonly DiscordSocketClient _discordClient;
        private readonly ILogger<ChatBot> _logger;

        public PrivateMessageTvShowNotifier(
            DiscordSocketClient discordClient,
            ILogger<ChatBot> logger)
        {
            _discordClient = discordClient;
            _logger = logger;
        }

        public async Task<HashSet<string>> NotifyAsync(IReadOnlyCollection<string> userIds, TvShow tvShow, int seasonNumber, CancellationToken token)
        {
            var userNotified = new HashSet<string>();

            foreach (var userId in userIds)
            {
                if (token.IsCancellationRequested)
                    return userNotified;

                try
                {
                    var user = _discordClient.GetUser(ulong.Parse(userId));

                    if (user != null)
                    {
                        var channel = await user.GetOrCreateDMChannelAsync();
                        await channel.SendMessageAsync($"The first episode of **season {seasonNumber}** of **{tvShow.Title}** that you requested has finished downloading!", false, DiscordTvShowsRequestingWorkFlow.GenerateTvShowDetailsAsync(tvShow, user));
                        userNotified.Add(userId);
                    }
                    else if (!token.IsCancellationRequested && _discordClient.ConnectionState == ConnectionState.Connected)
                    {
                        userNotified.Add(userId);
                    }
                }
                catch (System.Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while sending a tv show notification to a specific user: " + ex.Message);
                }
            }

            return userNotified;
        }
    }
}