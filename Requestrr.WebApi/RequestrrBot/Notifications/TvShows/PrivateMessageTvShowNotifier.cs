using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using Microsoft.Extensions.Logging;
using Requestrr.WebApi.RequestrrBot.ChatClients.Discord;
using Requestrr.WebApi.RequestrrBot.DownloadClients;
using Requestrr.WebApi.RequestrrBot.Locale;
using Requestrr.WebApi.RequestrrBot.TvShows;

namespace Requestrr.WebApi.RequestrrBot.Notifications.TvShows
{
    public class PrivateMessageTvShowNotifier : ITvShowNotifier
    {
        private readonly DiscordClient _discordClient;
        private readonly DiscordSettingsProvider _discordSettingsProvider;
        private readonly ILogger _logger;

        public PrivateMessageTvShowNotifier(
            DiscordClient discordClient,
            DiscordSettingsProvider discordSettingsProvider,
            ILogger logger)
        {
            _discordClient = discordClient;
            _discordSettingsProvider = discordSettingsProvider;
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
                    var user = _discordClient.Guilds.Values.Where(x => x.Members.ContainsKey(ulong.Parse(userId))).FirstOrDefault().Members[ulong.Parse(userId)];

                    if (user != null)
                    {
                        var channel = await user.CreateDmChannelAsync();

                        if(_discordSettingsProvider.Provide().TvShowDownloadClient == DownloadClient.Overseerr)
                            await channel.SendMessageAsync(Language.Current.DiscordNotificationTvDMSeason.ReplaceTokens(tvShow, seasonNumber), DiscordTvShowUserInterface.GenerateTvShowDetailsAsync(tvShow));
                        else
                            await channel.SendMessageAsync(Language.Current.DiscordNotificationTvDMFirstEpisode.ReplaceTokens(tvShow, seasonNumber), DiscordTvShowUserInterface.GenerateTvShowDetailsAsync(tvShow));

                        userNotified.Add(userId);
                    }
                    else if (!token.IsCancellationRequested)
                    {
                        _logger.LogWarning($"An error occurred while sending a show notification to a specific user, could not find user with ID {userId}. {System.Environment.NewLine} Make sure [Presence Intent] and [Server Members Intent] are enabled in the bots configuration.");
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