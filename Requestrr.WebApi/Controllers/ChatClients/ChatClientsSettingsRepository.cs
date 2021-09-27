using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Requestrr.WebApi.config;
using Requestrr.WebApi.RequestrrBot;
using Requestrr.WebApi.RequestrrBot.Locale;

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
                settings.ChatClients.Discord.EnableRequestsThroughDirectMessages = chatClientsSettings.Discord.EnableRequestsThroughDirectMessages;
                settings.ChatClients.Discord.AutomaticallyNotifyRequesters = chatClientsSettings.Discord.AutomaticallyNotifyRequesters;
                settings.ChatClients.Discord.NotificationMode = chatClientsSettings.Discord.NotificationMode;
                settings.ChatClients.Discord.NotificationChannels = JToken.FromObject(chatClientsSettings.Discord.NotificationChannels);
                settings.ChatClients.Discord.AutomaticallyPurgeCommandMessages = chatClientsSettings.Discord.AutomaticallyPurgeCommandMessages;

                settings.BotClient.Client = botClientSettings.Client;

                settings.ChatClients.Language = chatClientsSettings.Language.ToLower();
                Language.Current = JsonConvert.DeserializeObject<Language>(File.ReadAllText($"locales/{settings.ChatClients.Language}.json"));
            });
        }
    }
}