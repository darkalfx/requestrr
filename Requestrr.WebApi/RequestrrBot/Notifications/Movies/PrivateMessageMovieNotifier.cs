using System.Collections.Generic;
using System.Linq;
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
    public class PrivateMessageMovieNotifier : IMovieNotifier
    {
        private readonly DiscordClient _discordClient;
        private readonly ILogger _logger;

        public PrivateMessageMovieNotifier(
            DiscordClient discordClient,
            ILogger logger)
        {
            _discordClient = discordClient;
            _logger = logger;
        }

        public async Task<HashSet<string>> NotifyAsync(IReadOnlyCollection<string> userIds, Movie movie, CancellationToken token)
        {
            var userNotified = new HashSet<string>();

            if (_discordClient.Guilds.Any())
            {
                foreach (var userId in userIds)
                {
                    if (token.IsCancellationRequested)
                        return userNotified;

                    try
                    {
                        DiscordMember user = null;

                        foreach (var guild in _discordClient.Guilds.Values)
                        {
                            try
                            {
                                user = await guild.GetMemberAsync(ulong.Parse(userId));
                                break;
                            }
                            catch { }
                        }

                        if (user != null)
                        {
                            var channel = await user.CreateDmChannelAsync();
                            await channel.SendMessageAsync(Language.Current.DiscordNotificationMovieDM.ReplaceTokens(movie), await DiscordMovieUserInterface.GenerateMovieDetailsAsync(movie));
                        }
                        else
                        {
                            _logger.LogWarning($"Removing movie notification for user with ID {userId} as it could not be found in any of the guilds.");
                        }

                        userNotified.Add(userId);
                    }
                    catch (DSharpPlus.Exceptions.UnauthorizedException ex)
                    {
                        userNotified.Add(userId);
                        _logger.LogWarning($"Removing movie notification [{movie.Title}] for user [{userId}] as we are missing permissions to do so [Unauthorized].");
                    }
                    catch (System.Exception ex)
                    {
                        _logger.LogError(ex, "An error occurred while sending a private message movie notification to a specific user: " + ex.Message);
                    }
                }
            }

            return userNotified;
        }
    }
}