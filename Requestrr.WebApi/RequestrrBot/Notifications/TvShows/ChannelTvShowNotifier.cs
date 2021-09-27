using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Requestrr.WebApi.RequestrrBot.ChatClients.Discord;
using Requestrr.WebApi.RequestrrBot.DownloadClients;
using Requestrr.WebApi.RequestrrBot.Locale;
using Requestrr.WebApi.RequestrrBot.TvShows;

namespace Requestrr.WebApi.RequestrrBot.Notifications.TvShows
{
    public class ChannelTvShowNotifier : ITvShowNotifier
    {
        private readonly DiscordClient _discordClient;
        private readonly DiscordSettingsProvider _discordSettingsProvider;
        private readonly string[] _channelNames;
        private readonly ILogger _logger;

        public ChannelTvShowNotifier(
            DiscordClient discordClient,
            DiscordSettingsProvider discordSettingsProvider,
            string[] channelNames,
            ILogger logger)
        {
            _discordClient = discordClient;
            _discordSettingsProvider = discordSettingsProvider;
            _channelNames = channelNames;
            _logger = logger;
        }

        public async Task<HashSet<string>> NotifyAsync(IReadOnlyCollection<string> userIds, TvShow tvShow, int seasonNumber, CancellationToken token)
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
                    await NotifyUsersInChannel(tvShow, seasonNumber, discordUserIds, userNotified, channel);
                }
                catch (System.Exception ex)
                {
                    _logger.LogError(ex, $"An error occurred while sending a tv show notification to channel \"{channel?.Name}\" in server \"{channel?.Guild?.Name}\": " + ex.Message);
                }
            }

            return userNotified;
        }

        private DiscordChannel[] GetNotificationChannels()
        {
            return _discordClient.Guilds
                .SelectMany(x => x.Value.Channels.Values)
                .Where(x => _channelNames.Any(n => n.Equals(x.Name, StringComparison.InvariantCultureIgnoreCase)))
                .OfType<DiscordChannel>()
                .ToArray();
        }

        private void HandleRemovedUsers(HashSet<ulong> discordUserIds, HashSet<string> userNotified, CancellationToken token)
        {
            foreach (var userId in discordUserIds.Where(x => _discordClient.GetUserAsync(x) == null))
            {
                if (token.IsCancellationRequested)
                    return;

                userNotified.Add(userId.ToString());
                _logger.LogWarning($"An issue occurred while sending a tv show notification to a specific user, could not find user with ID {userId}. {System.Environment.NewLine} Make sure [Presence Intent] and [Server Members Intent] are enabled in the bots configuration.");
            }
        }

        private async Task NotifyUsersInChannel(TvShow tvShow, int seasonNumber, HashSet<ulong> discordUserIds, HashSet<string> userNotified, DiscordChannel channel)
        {
            var usersToMention = channel.Users
                .Where(x => discordUserIds.Contains(x.Id))
                .Where(x => !userNotified.Contains(x.Id.ToString()));

            if (usersToMention.Any())
            {
                var messageBuilder = new StringBuilder();

                if(_discordSettingsProvider.Provide().TvShowDownloadClient == DownloadClient.Overseerr)
                    messageBuilder.AppendLine(Language.Current.DiscordNotificationTvChannelSeason.ReplaceTokens(tvShow, seasonNumber));
                else
                    messageBuilder.AppendLine(Language.Current.DiscordNotificationTvChannelFirstEpisode.ReplaceTokens(tvShow, seasonNumber));

                foreach (var user in usersToMention)
                {
                    var userMentionText = $"{user.Mention} ";

                    if (messageBuilder.Length + userMentionText.Length < DiscordConstants.MaxMessageLength)
                        messageBuilder.Append(userMentionText);
                }

                await channel.SendMessageAsync(messageBuilder.ToString(), DiscordTvShowUserInterface.GenerateTvShowDetailsAsync(tvShow));

                foreach (var user in usersToMention)
                {
                    userNotified.Add(user.Id.ToString());
                }
            }
        }
    }
}