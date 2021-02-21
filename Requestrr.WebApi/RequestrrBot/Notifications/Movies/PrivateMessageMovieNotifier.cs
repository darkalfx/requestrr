using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Requestrr.WebApi.RequestrrBot.ChatClients.Discord;
using Requestrr.WebApi.RequestrrBot.Movies;

namespace Requestrr.WebApi.RequestrrBot.Notifications.Movies
{
    public class PrivateMessageMovieNotifier : IMovieNotifier
    {
        private readonly DiscordSocketClient _discordClient;
        private readonly ILogger<ChatBot> _logger;

        public PrivateMessageMovieNotifier(
            DiscordSocketClient discordClient,
            ILogger<ChatBot> logger)
        {
            _discordClient = discordClient;
            _logger = logger;
        }

        public async Task<HashSet<string>> NotifyAsync(IReadOnlyCollection<string> userIds, Movie movie, CancellationToken token)
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
                        await channel.SendMessageAsync($"The movie **{movie.Title}** you requested has finished downloading!", false, await DiscordMovieRequestingWorkFlow.GenerateMovieDetailsAsync(movie, user));
                        userNotified.Add(userId);
                    }
                    else if (!token.IsCancellationRequested && _discordClient.ConnectionState == ConnectionState.Connected)
                    {
                        _logger.LogWarning($"An error occurred while sending a movie notification to a specific user, could not find user with ID {userId}. {System.Environment.NewLine} Make sure [Presence Intent] and [Server Members Intent] are enabled in the bots configuration.");
                    }
                }
                catch (System.Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while sending a movie notification to a specific user: " + ex.Message);
                }
            }

            return userNotified;
        }
    }
}