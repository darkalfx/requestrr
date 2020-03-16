using Newtonsoft.Json.Linq;
using Requestrr.WebApi.config;
using Requestrr.WebApi.RequestrrBot;

namespace Requestrr.WebApi.Controllers.ChatClients
{
    public static class ChatClientsSettingsRepository
    {
        public static void Update(BotClientSettings botClientSettings, ChatClientsSettings chatClientsSettings)
        {
            SettingsFile.Write(settings =>
            {
                settings.ChatClients.Discord.BotToken = chatClientsSettings.Discord.BotToken;
                settings.ChatClients.Discord.ClientId = chatClientsSettings.Discord.ClientId;
                settings.ChatClients.Discord.StatusMessage = chatClientsSettings.Discord.StatusMessage;
                settings.ChatClients.Discord.MonitoredChannels = JToken.FromObject(chatClientsSettings.Discord.MonitoredChannels);
                settings.ChatClients.Discord.TvShowRoles = JToken.FromObject(chatClientsSettings.Discord.TvShowRoles);
                settings.ChatClients.Discord.MovieRoles = JToken.FromObject(chatClientsSettings.Discord.MovieRoles);
                settings.ChatClients.Discord.EnableDirectMessageSupport = chatClientsSettings.Discord.EnableDirectMessageSupport;
                settings.BotClient.Client = botClientSettings.Client;
                settings.BotClient.CommandPrefix = botClientSettings.CommandPrefix;
            });
        }
    }
}