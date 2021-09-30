using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Requestrr.WebApi.RequestrrBot.ChatClients.Discord;
using Requestrr.WebApi.RequestrrBot.Locale;
using Requestrr.WebApi.RequestrrBot.Movies;

namespace Requestrr.WebApi.RequestrrBot.Notifications.Movies
{
    public class ChannelMovieNotifier : IMovieNotifier
    {
        private readonly DiscordClient _discordClient;
        private readonly ulong[] _channelIds;
        private readonly ILogger _logger;

        public ChannelMovieNotifier(
            DiscordClient discordClient,
            ulong[] channelIds,
            ILogger logger)
        {
            _discordClient = discordClient;
            _channelIds = channelIds;
            _logger = logger;
        }

        public async Task<HashSet<string>> NotifyAsync(IReadOnlyCollection<string> userIds, Movie movie, CancellationToken token)
        {
            if (_discordClient.Guilds.Any())
            {
                var discordUserIds = new HashSet<ulong>(userIds.Select(x => ulong.Parse(x)));
                var userNotified = new HashSet<ulong>();

                var channels = GetNotificationChannels();
                HandleRemovedUsers(discordUserIds, userNotified, token);

                foreach (var channel in channels)
                {
                    if (token.IsCancellationRequested)
                        return new HashSet<string>(userNotified.Select(x => x.ToString()));

                    try
                    {
                        await NotifyUsersInChannel(movie, discordUserIds, userNotified, channel);
                    }
                    catch (System.Exception ex)
                    {
                        _logger.LogError(ex, $"An error occurred while sending a movie notification to channel \"{channel?.Name}\" in server \"{channel?.Guild?.Name}\": " + ex.Message);
                    }
                }

                return new HashSet<string>(userNotified.Select(x => x.ToString()));
            }

            return new HashSet<string>();
        }

        private DiscordChannel[] GetNotificationChannels()
        {
            return _discordClient.Guilds
                .SelectMany(x => x.Value.Channels.Values)
                .Where(x => _channelIds.Contains(x.Id))
                .OfType<DiscordChannel>()
                .ToArray();
        }

        private void HandleRemovedUsers(HashSet<ulong> discordUserIds, HashSet<ulong> userNotified, CancellationToken token)
        {
            var currentMembers = new HashSet<ulong>(_discordClient.Guilds.SelectMany(x => x.Value.Members).Select(x => x.Value.Id));

            foreach (var missingUserIds in discordUserIds.Except(currentMembers))
            {
                if (token.IsCancellationRequested)
                    return;

                userNotified.Add(missingUserIds);
                _logger.LogWarning($"Removing movie notification for user with ID {missingUserIds} as it could not be found in any of the guilds.");
            }
        }

        private static async Task NotifyUsersInChannel(Movie movie, HashSet<ulong> discordUserIds, HashSet<ulong> userNotified, DiscordChannel channel)
        {
            var usersToNotify = channel.Users
                .Where(x => discordUserIds.Contains(x.Id))
                .Where(x => !userNotified.Contains(x.Id));

            if (usersToNotify.Any())
            {
                var messageBuilder = new StringBuilder();
                messageBuilder.AppendLine(Language.Current.DiscordNotificationMovieChannel.ReplaceTokens(movie));

                foreach (var user in usersToNotify)
                {
                    var userMentionText = $"{user.Mention} ";

                    if (messageBuilder.Length + userMentionText.Length < DiscordConstants.MaxMessageLength)
                        messageBuilder.Append(userMentionText);
                }

                await channel.SendMessageAsync(messageBuilder.ToString(), await DiscordMovieUserInterface.GenerateMovieDetailsAsync(movie));

                foreach (var user in usersToNotify)
                {
                    userNotified.Add(user.Id);
                }
            }
        }
    }
}