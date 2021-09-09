using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Requestrr.WebApi.RequestrrBot.ChatClients.Discord;
using Requestrr.WebApi.RequestrrBot.DownloadClients;
using Requestrr.WebApi.RequestrrBot.Locale;
using Requestrr.WebApi.RequestrrBot.TvShows;

namespace Requestrr.WebApi.RequestrrBot.Notifications.TvShows
{
    public class PrivateMessageTvShowNotifier : ITvShowNotifier
    {
        private readonly DiscordSocketClient _discordClient;
        private readonly DiscordSettingsProvider _discordSettingsProvider;
        private readonly ILogger<ChatBot> _logger;

        public PrivateMessageTvShowNotifier(
            DiscordSocketClient discordClient,
            DiscordSettingsProvider discordSettingsProvider,
            ILogger<ChatBot> logger)
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
                    var user = _discordClient.GetUser(ulong.Parse(userId));

                    if (user != null)
                    {
                        var channel = await user.GetOrCreateDMChannelAsync();

                        if(_discordSettingsProvider.Provide().TvShowDownloadClient == DownloadClient.Overseerr)
                            await channel.SendMessageAsync(Language.Current.DiscordNotificationTvDMSeason.ReplaceTokens(tvShow, seasonNumber), false, DiscordTvShowsRequestingWorkFlow.GenerateTvShowDetailsAsync(tvShow, user));
                        else
                            await channel.SendMessageAsync(Language.Current.DiscordNotificationTvDMFirstEpisode.ReplaceTokens(tvShow, seasonNumber), false, DiscordTvShowsRequestingWorkFlow.GenerateTvShowDetailsAsync(tvShow, user));

                        userNotified.Add(userId);
                    }
                    else if (!token.IsCancellationRequested && _discordClient.ConnectionState == ConnectionState.Connected)
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