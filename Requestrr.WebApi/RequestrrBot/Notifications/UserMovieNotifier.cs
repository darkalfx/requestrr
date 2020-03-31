using System.Threading.Tasks;
using Discord.WebSocket;
using Requestrr.WebApi.RequestrrBot.ChatClients.Discord;
using Requestrr.WebApi.RequestrrBot.Movies;

namespace Requestrr.WebApi.RequestrrBot.Notifications
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
            await channel.SendMessageAsync($"The movie **{movie.Title}** you requested has finished downloading!", false, await DiscordMovieRequestingWorkFlow.GenerateMovieDetailsAsync(movie, user));
        }
    } 
}