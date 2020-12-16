using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Requestrr.WebApi.RequestrrBot.ChatClients.Discord;
using Requestrr.WebApi.RequestrrBot.Music;

namespace Requestrr.WebApi.RequestrrBot.Notifications.Music
{
    public class ChannelArtistNotifier : IArtistNotifier
    {
        private readonly DiscordSocketClient _discordClient;
        private readonly string[] _channelNames;
        private readonly ILogger<ChatBot> _logger;

        public ChannelArtistNotifier(
            DiscordSocketClient discordClient,
            string[] channelNames,
            ILogger<ChatBot> logger)
        {
            _discordClient = discordClient;
            _channelNames = channelNames;
            _logger = logger;
        }

        public async Task<HashSet<string>> NotifyAsync(IReadOnlyCollection<string> userIds, Artist tvShow, CancellationToken token)
        {
            var discordUserIds = new HashSet<ulong>(userIds.Select(x => ulong.Parse(x)));
            var userNotified = new HashSet<string>();

            var channels = GetNotificationChannels();
            HandleRemovedUsers(discordUserIds, userNotified, token);

            foreach (var channel in channels)
            {
                if (token.IsCancellationRequested)
                    return userNotified;

                try
                {
                    await NotifyUsersInChannel(tvShow, discordUserIds, userNotified, channel);
                }
                catch (System.Exception ex)
                {
                    _logger.LogError(ex, $"An error occurred while sending a artist notification to channel \"{channel?.Name}\" in server \"{channel?.Guild?.Name}\": " + ex.Message);
                }
            }

            return userNotified;
        }

        private SocketTextChannel[] GetNotificationChannels()
        {
            return _discordClient.Guilds
                .SelectMany(x => x.Channels)
                .Where(x => _channelNames.Any(n => n.Equals(x.Name, StringComparison.InvariantCultureIgnoreCase)))
                .OfType<SocketTextChannel>()
                .ToArray();
        }

        private void HandleRemovedUsers(HashSet<ulong> discordUserIds, HashSet<string> userNotified, CancellationToken token)
        {
            foreach (var userId in discordUserIds.Where(x => _discordClient.GetUser(x) == null))
            {
                if (token.IsCancellationRequested)
                    return;

                if (_discordClient.ConnectionState == ConnectionState.Connected)
                {
                    userNotified.Add(userId.ToString());
                }
            }
        }

        private static async Task NotifyUsersInChannel(Artist artist, HashSet<ulong> discordUserIds, HashSet<string> userNotified, SocketTextChannel channel)
        {
            var usersToMention = channel.Users
                .Where(x => discordUserIds.Contains(x.Id))
                .Where(x => !userNotified.Contains(x.Id.ToString()));

            if (usersToMention.Any())
            {
                var messageBuilder = new StringBuilder();
                messageBuilder.AppendLine($"The first album of **{artist.Name}** has finished downloading!");

                foreach (var user in usersToMention)
                {
                    var userMentionText = $"{user.Mention} ";

                    if (messageBuilder.Length + userMentionText.Length < DiscordConstants.MaxMessageLength)
                        messageBuilder.Append(userMentionText);
                }

                await channel.SendMessageAsync(messageBuilder.ToString(), false, DiscordMusicRequestingWorkFlow.GenerateArtistDetailsAsync(artist).Result);

                foreach (var user in usersToMention)
                {
                    userNotified.Add(user.Id.ToString());
                }
            }
        }
    }
}