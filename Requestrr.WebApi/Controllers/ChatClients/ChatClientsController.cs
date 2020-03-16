using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Requestrr.WebApi.config;

namespace Requestrr.WebApi.Controllers.ChatClients
{
    [ApiController]
    [Authorize]
    [Route("/api/chatclients")]
    public class ChatClientsController : ControllerBase
    {
        public class TestDiscordSettingsModel
        {
            [Required]
            public string BotToken { get; set; }
        }

        private readonly ChatClientsSettings _chatClientsSettings;
        private readonly BotClientSettings _botClientsSettings;

        public ChatClientsController(
            IOptionsSnapshot<ChatClientsSettings> chatClientsSettingsAccessor,
            IOptionsSnapshot<BotClientSettings> botClientsSettingsAccessor)
        {
            _chatClientsSettings = chatClientsSettingsAccessor.Value;
            _botClientsSettings = botClientsSettingsAccessor.Value;
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
                EnableDirectMessageSupport = _chatClientsSettings.Discord.EnableDirectMessageSupport,
                CommandPrefix = _botClientsSettings.CommandPrefix,
                TvShowRoles = _chatClientsSettings.Discord.TvShowRoles ?? Array.Empty<string>(),
                MovieRoles = _chatClientsSettings.Discord.MovieRoles ?? Array.Empty<string>(),
                MonitoredChannels = _chatClientsSettings.Discord.MonitoredChannels ?? Array.Empty<string>(),
            });
        }

        [HttpPost("discord/test")]
        public async Task<IActionResult> TestDiscordSettings([FromBody]TestDiscordSettingsModel model)
        {
            try
            {
                using (var discord = new DiscordSocketClient())
                {
                    await discord.LoginAsync(TokenType.Bot, model.BotToken.Trim());
                    await discord.LogoutAsync();
                }

                return Ok(new { ok = true });
            }
            catch (System.Exception)
            {
                return BadRequest($"The specified settings are invalid");
            }
        }

        [HttpPost()]
        public async Task<IActionResult> SaveAsync([FromBody]ChatClientsSettingsModel model)
        {
            if (!model.Client.Equals("discord", System.StringComparison.InvariantCultureIgnoreCase))
            {
                return BadRequest("Invalid client was selected");
            }

            _chatClientsSettings.Discord.BotToken = model.BotToken.Trim();
            _chatClientsSettings.Discord.ClientId = model.ClientId;
            _chatClientsSettings.Discord.StatusMessage = model.StatusMessage.Trim();
            _chatClientsSettings.Discord.TvShowRoles = model.TvShowRoles ?? Array.Empty<string>();
            _chatClientsSettings.Discord.MovieRoles = model.MovieRoles ?? Array.Empty<string>();
            _chatClientsSettings.Discord.EnableDirectMessageSupport = model.EnableDirectMessageSupport;
            _chatClientsSettings.Discord.MonitoredChannels = model.MonitoredChannels ?? Array.Empty<string>();

            _botClientsSettings.Client = model.Client;
            _botClientsSettings.CommandPrefix = model.CommandPrefix.Trim();

            ChatClientsSettingsRepository.Update(_botClientsSettings, _chatClientsSettings);

            return Ok(new { ok = true });
        }
    }
}
