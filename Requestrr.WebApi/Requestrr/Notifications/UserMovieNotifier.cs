using System.Threading.Tasks;
using Discord.WebSocket;
using Requestrr.WebApi.Requestrr.ChatClients;
using Requestrr.WebApi.Requestrr.Movies;

namespace Requestrr.WebApi.Requestrr.Notifications
{
    public class UserMovieNotifier
    {
        private readonly DiscordSocketClient _discordClient;
        public UserMovieNotifier(
            DiscordSocketClient discordClient)
        {
            _discordClient = discordClient;
        }

        public async Task NotifyAsync(string userId, Movie movie)
        {
            var user = _discordClient.GetUser(ulong.Parse(userId));
            var channel = await user.GetOrCreateDMChannelAsync();
            await channel.SendMessageAsync($"The movie **{movie.Title}** you requested has finished downloading and will be available in a few minutes!", false, await DiscordMovieRequestingWorkFlow.GenerateMovieDetailsAsync(movie, user));
        }
    } 
}