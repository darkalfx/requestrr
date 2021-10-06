using Requestrr.WebApi.config;
using Requestrr.WebApi.RequestrrBot;

namespace Requestrr.WebApi.Controllers.ChatClients
{
    public class BotClientSettingsProvider
    {
        public BotClientSettings Provide()
        {
            dynamic settings = SettingsFile.Read();

            return new BotClientSettings
            {
                Client = (string)settings.BotClient.Client,
            };
        }
    }
}