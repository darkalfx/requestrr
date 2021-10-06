using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Requestrr.WebApi.config;

namespace Requestrr.WebApi.Controllers.ChatClients
{
    [ApiController]
    [Authorize]
    [Route("/api/chatclients")]
    public class ChatClientsController : ControllerBase
    {
        private readonly ChatClientsSettings _chatClientsSettings;
        private readonly BotClientSettings _botClientsSettings;

        public ChatClientsController(
            ChatClientsSettingsProvider chatClientsSettingsProvider,
            BotClientSettingsProvider botClientSettingsProvider)
        {
            _chatClientsSettings = chatClientsSettingsProvider.Provide();
            _botClientsSettings = botClientSettingsProvider.Provide();
        }

        [HttpGet()]
        public async Task<IActionResult> GetAsync()
        {
            return Ok(new ChatClientsSettingsModel
            {
                Client = "Discord",
                StatusMessage = _chatClientsSettings.Discord.StatusMessage,
                BotToken = _chatClientsSettings.Discord.BotToken,
                ClientId = _chatClientsSettings.Discord.ClientId,
                EnableRequestsThroughDirectMessages = _chatClientsSettings.Discord.EnableRequestsThroughDirectMessages,
                TvShowRoles = _chatClientsSettings.Discord.TvShowRoles ?? Array.Empty<string>(),
                MovieRoles = _chatClientsSettings.Discord.MovieRoles ?? Array.Empty<string>(),
                MonitoredChannels = _chatClientsSettings.Discord.MonitoredChannels ?? Array.Empty<string>(),
                AutomaticallyNotifyRequesters = _chatClientsSettings.Discord.AutomaticallyNotifyRequesters,
                NotificationMode = _chatClientsSettings.Discord.NotificationMode,
                NotificationChannels = _chatClientsSettings.Discord.NotificationChannels ?? Array.Empty<string>(),
                AutomaticallyPurgeCommandMessages = _chatClientsSettings.Discord.AutomaticallyPurgeCommandMessages,
                Language = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(_chatClientsSettings.Language.ToLower()),
                AvailableLanguages = GetLanguages()
            });
        }

        [HttpPost("discord/test")]
        public async Task<IActionResult> TestDiscordSettings([FromBody] ChatClientTestSettingsModel model)
        {
            try
            {
                var discord = new DiscordClient(new DiscordConfiguration
                {
                    Token = model.BotToken,
                    TokenType = TokenType.Bot,
                    Intents = DiscordIntents.All,
                    AutoReconnect = false
                });

                await discord.ConnectAsync();
                await discord.DisconnectAsync();
                discord.Dispose();

                return Ok(new { ok = true });
            }
            catch (System.Exception)
            {
                return BadRequest($"Invalid bot token or missing bot presence intents.");
            }
        }

        [HttpPost()]
        public async Task<IActionResult> SaveAsync([FromBody] ChatClientsSettingsModel model)
        {
            if (!model.Client.Equals("discord", System.StringComparison.InvariantCultureIgnoreCase))
            {
                return BadRequest("Invalid client was selected");
            }

            if (model.TvShowRoles.Any(x => !ulong.TryParse(x, System.Globalization.NumberStyles.Integer, null, out _)))
            {
                return BadRequest("Invalid tv show roles, please make sure to enter the discord role ids.");
            }

            if (model.MovieRoles.Any(x => !ulong.TryParse(x, System.Globalization.NumberStyles.Integer, null, out _)))
            {
                return BadRequest("Invalid movie roles, please make sure to enter the discord role ids.");
            }

            if (model.NotificationChannels.Any(x => !ulong.TryParse(x, System.Globalization.NumberStyles.Integer, null, out _)))
            {
                return BadRequest("Invalid notification channels, please make sure to enter the discord channel ids.");
            }

            if (model.MonitoredChannels.Any(x => !ulong.TryParse(x, System.Globalization.NumberStyles.Integer, null, out _)))
            {
                return BadRequest("Invalid monitored channels channels, please make sure to enter the monitored channel ids.");
            }

            _chatClientsSettings.Discord.BotToken = model.BotToken.Trim();
            _chatClientsSettings.Discord.ClientId = model.ClientId;
            _chatClientsSettings.Discord.StatusMessage = model.StatusMessage.Trim();
            _chatClientsSettings.Discord.TvShowRoles = (model.TvShowRoles ?? Array.Empty<string>()).Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToArray();
            _chatClientsSettings.Discord.MovieRoles = (model.MovieRoles ?? Array.Empty<string>()).Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToArray();
            _chatClientsSettings.Discord.EnableRequestsThroughDirectMessages = model.EnableRequestsThroughDirectMessages;
            _chatClientsSettings.Discord.MonitoredChannels = (model.MonitoredChannels ?? Array.Empty<string>()).Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToArray();

            _chatClientsSettings.Discord.AutomaticallyNotifyRequesters = model.AutomaticallyNotifyRequesters;
            _chatClientsSettings.Discord.NotificationMode = model.NotificationMode;
            _chatClientsSettings.Discord.NotificationChannels = (model.NotificationChannels ?? Array.Empty<string>()).Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToArray(); ;
            _chatClientsSettings.Discord.AutomaticallyPurgeCommandMessages = model.AutomaticallyPurgeCommandMessages;
            _chatClientsSettings.Language = model.Language;

            _botClientsSettings.Client = model.Client;

            ChatClientsSettingsRepository.Update(_botClientsSettings, _chatClientsSettings);

            return Ok(new { ok = true });
        }

        private string[] GetLanguages()
        {
            return Directory.GetFiles("locales", "*.json", SearchOption.TopDirectoryOnly)
                 .Select(x => System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Path.GetFileNameWithoutExtension(x).ToLower()))
                 .ToArray();
        }
    }
}
