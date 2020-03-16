using System.Threading.Tasks;
using Discord.WebSocket;
using Requestrr.WebApi.RequestrrBot.ChatClients.Discord;
using Requestrr.WebApi.RequestrrBot.TvShows;

namespace Requestrr.WebApi.RequestrrBot.Notifications
{
    public class UserTvShowNotifier
    {
        private readonly DiscordSocketClient _discordClient;
        public UserTvShowNotifier(
            DiscordSocketClient discordClient)
        {
            _discordClient = discordClient;
        }

        public async Task NotifyAsync(string userId, TvShow tvShow, int seasonNumber)
        {
            var user = _discordClient.GetUser(ulong.Parse(userId));
            var channel = await user.GetOrCreateDMChannelAsync();
            await channel.SendMessageAsync($"The first episode of **season {seasonNumber}** of **{tvShow.Title}** that you requested has finished downloading and will be available in a few minutes!", false, DiscordTvShowsRequestingWorkFlow.GenerateTvShowDetailsAsync(tvShow, user));
        }
    }
}