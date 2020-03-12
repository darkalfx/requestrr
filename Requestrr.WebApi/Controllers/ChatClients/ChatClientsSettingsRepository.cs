using Requestrr.WebApi.Config;

namespace Requestrr.WebApi.Controllers
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
                settings.ChatClients.Discord.EnableDirectMessageSupport = chatClientsSettings.Discord.EnableDirectMessageSupport;

                settings.BotClient.Client = botClientSettings.Client;
                settings.BotClient.MonitoredChannels = botClientSettings.MonitoredChannels;
                settings.BotClient.CommandPrefix = botClientSettings.CommandPrefix;
            });
        }
    }
}