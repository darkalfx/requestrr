using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Requestrr.WebApi.Extensions;
using Requestrr.WebApi.RequestrrBot.ChatClients.Discord;
using Requestrr.WebApi.RequestrrBot.DownloadClients;
using Requestrr.WebApi.RequestrrBot.DownloadClients.Ombi;
using Requestrr.WebApi.RequestrrBot.DownloadClients.Overseerr;
using Requestrr.WebApi.RequestrrBot.DownloadClients.Radarr;
using Requestrr.WebApi.RequestrrBot.DownloadClients.Sonarr;
using Requestrr.WebApi.RequestrrBot.Movies;
using Requestrr.WebApi.RequestrrBot.Notifications;
using Requestrr.WebApi.RequestrrBot.Notifications.Movies;
using Requestrr.WebApi.RequestrrBot.Notifications.TvShows;
using Requestrr.WebApi.RequestrrBot.TvShows;

namespace Requestrr.WebApi.RequestrrBot
{
    public class ChatBot
    {
        private DiscordSocketClient _client;
        private MovieNotificationEngine _movieNotificationEngine;
        private TvShowNotificationEngine _tvShowNotificationEngine;
        private CommandService _commandService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ChatBot> _logger;
        private readonly DiscordSettingsProvider _discordSettingsProvider;
        private readonly ConcurrentBag<Func<Task>> _refreshQueue = new ConcurrentBag<Func<Task>>();
        private DiscordSettings _currentSettings = new DiscordSettings();
        private MovieNotificationsRepository _movieNotificationRequestRepository = new MovieNotificationsRepository();
        private TvShowNotificationsRepository _tvShowNotificationRequestRepository = new TvShowNotificationsRepository();

        private OverseerrClient _overseerrClient;
        private OmbiClient _ombiDownloadClient;
        private RadarrClient _radarrDownloadClient;
        private SonarrClient _sonarrDownloadClient;
        private ModuleInfo _moduleInfo = null;

        public ChatBot(IServiceProvider serviceProvider, ILogger<ChatBot> logger, DiscordSettingsProvider discordSettingsProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _discordSettingsProvider = discordSettingsProvider;
            _overseerrClient = new OverseerrClient(serviceProvider.Get<IHttpClientFactory>(), serviceProvider.Get<ILogger<OverseerrClient>>(), serviceProvider.Get<OverseerrSettingsProvider>());
            _ombiDownloadClient = new OmbiClient(serviceProvider.Get<IHttpClientFactory>(), serviceProvider.Get<ILogger<OmbiClient>>(), serviceProvider.Get<OmbiSettingsProvider>());
            _radarrDownloadClient = new RadarrClient(serviceProvider.Get<IHttpClientFactory>(), serviceProvider.Get<ILogger<RadarrClient>>(), serviceProvider.Get<RadarrSettingsProvider>());
            _sonarrDownloadClient = new SonarrClient(serviceProvider.Get<IHttpClientFactory>(), serviceProvider.Get<ILogger<SonarrClient>>(), serviceProvider.Get<SonarrSettingsProvider>());
        }

        public async void Start()
        {
            _commandService = new CommandService(new CommandServiceConfig
            {
                CaseSensitiveCommands = false,
            });

            _client = new DiscordSocketClient();
            _client.Log += LogAsync;
            _client.Connected += Connected;
            _commandService.CommandExecuted += CommandExecutedAsync;
            _client.MessageReceived += MessageReceivedAsync;

            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        var newSettings = _discordSettingsProvider.Provide();
                        var mustRestart = false;

                        try
                        {
                            if (_client.ConnectionState == ConnectionState.Connected)
                            {
                                mustRestart = string.IsNullOrEmpty(_client.CurrentUser.Username);
                            }
                        }
                        catch
                        {
                            mustRestart = true;
                        }

                        if (mustRestart)
                        {
                            _logger.LogWarning("Restarting bot due to incorrect automatic reconnection.");
                        }

                        if (!_currentSettings.Equals(newSettings) || mustRestart || _client.ConnectionState == ConnectionState.Disconnected)
                        {
                            _logger.LogWarning("Bot configuration changed/not connected to Discord: restarting bot");
                            _currentSettings = newSettings;
                            await RestartBot(newSettings);
                            _logger.LogWarning("Bot has been restarted.");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error while restarting the bot: " + ex.Message);
                    }

                    await Task.Delay(5000);
                }
            });
        }

        private async Task RestartBot(DiscordSettings discordSettings)
        {
            await _client.StopAsync();
            await _client.LogoutAsync();

            if (_moduleInfo != null)
            {
                await _commandService.RemoveModuleAsync(_moduleInfo);
            }

            if (!string.IsNullOrEmpty(discordSettings.BotToken))
            {
                await ApplyBotConfigurationAsync(discordSettings);

                await _client.LoginAsync(TokenType.Bot, discordSettings.BotToken);
                await _client.StartAsync();
            }
            else
            {
                _logger.LogWarning("No Bot Token for Discord has been configured.");
            }
        }

        private async Task ApplyBotConfigurationAsync(DiscordSettings discordSettings)
        {
            await _client.SetGameAsync(discordSettings.StatusMessage);

            _moduleInfo = await _commandService.CreateModuleAsync(string.Empty, x =>
            {
                x.AddCommand("ping", async (commandContext, noidea, serviceProvider, commandInfo) =>
                {
                    using (var command = new DiscordPingWorkFlow((SocketCommandContext)commandContext, _client, serviceProvider.Get<DiscordSettingsProvider>()))
                    {
                        await command.HandlePingAsync();
                    }
                }, c => c.WithName("ping").WithRunMode(RunMode.Async));

                x.AddCommand("help", async (commandContext, noidea, serviceProvider, commandInfo) =>
                {
                    using (var command = new DiscordHelpWorkFlow((SocketCommandContext)commandContext, _client, serviceProvider.Get<DiscordSettingsProvider>()))
                    {
                        await command.HandleHelpAsync();
                    }
                }, c => c.WithName("help").WithRunMode(RunMode.Async));

                if (discordSettings.MovieDownloadClient != DownloadClient.Disabled)
                {
                    x.AddCommand(discordSettings.MovieCommand, async (commandContext, message, serviceProvider, commandInfo) =>
                    {
                        using (var command = new DiscordMovieRequestingWorkFlow(
                        (SocketCommandContext)commandContext,
                        _client,
                        GetMovieClient<IMovieSearcher>(discordSettings),
                        GetMovieClient<IMovieRequester>(discordSettings),
                        serviceProvider.Get<DiscordSettingsProvider>(),
                        _movieNotificationRequestRepository))
                        {
                            await command.HandleMovieRequestAsync(message);
                        }
                    }, c => c.WithName("movie").WithRunMode(RunMode.Async).AddParameter<string>("movieName", p => p.WithIsRemainder(true).WithIsOptional(true)));
                }
                else
                {
                    x.AddCommand(discordSettings.MovieCommand, async (commandContext, message, serviceProvider, commandInfo) =>
                    {
                        using (var command = new DiscordDisableCommandWorkFlow((SocketCommandContext)commandContext, _client, serviceProvider.Get<DiscordSettingsProvider>()))
                        {
                            await command.HandleDisabledCommandAsync();
                        }
                    }, c => c.WithName("movie").WithRunMode(RunMode.Sync).AddParameter<string>("movieName", p => p.WithIsRemainder(true).WithIsOptional(true)));
                }

                if (discordSettings.TvShowDownloadClient != DownloadClient.Disabled)
                {
                    x.AddCommand(discordSettings.TvShowCommand, async (commandContext, message, serviceProvider, commandInfo) =>
                    {
                        using (var command = new DiscordTvShowsRequestingWorkFlow(
                           (SocketCommandContext)commandContext,
                           _client,
                            GetTvShowClient<ITvShowSearcher>(discordSettings),
                            GetTvShowClient<ITvShowRequester>(discordSettings),
                            serviceProvider.Get<DiscordSettingsProvider>(),
                            serviceProvider.Get<TvShowsSettingsProvider>(),
                           _tvShowNotificationRequestRepository))
                        {
                            await command.HandleTvShowRequestAsync(message);
                        }
                    }, c => c.WithName("tv").WithRunMode(RunMode.Async).AddParameter<string>("tvShowName", p => p.WithIsRemainder(true).WithIsOptional(true)));
                }
                else
                {
                    x.AddCommand(discordSettings.TvShowCommand, async (commandContext, message, serviceProvider, commandInfo) =>
                    {
                        using (var command = new DiscordDisableCommandWorkFlow((SocketCommandContext)commandContext, _client, serviceProvider.Get<DiscordSettingsProvider>()))
                        {
                            await command.HandleDisabledCommandAsync();
                        }
                    }, c => c.WithName("tv").WithRunMode(RunMode.Sync).AddParameter<string>("tvShowName", p => p.WithIsRemainder(true).WithIsOptional(true)));
                }
            });
        }

        private T GetMovieClient<T>(DiscordSettings settings) where T : class
        {
            if (settings.MovieDownloadClient == DownloadClient.Radarr)
            {
                return _radarrDownloadClient as T;
            }
            else if (settings.MovieDownloadClient == DownloadClient.Ombi)
            {
                return _ombiDownloadClient as T;
            }
            else if (settings.MovieDownloadClient == DownloadClient.Overseerr)
            {
                return _overseerrClient as T;
            }
            else
            {
                throw new Exception($"Invalid configured movie download client {settings.MovieDownloadClient}");
            }
        }

        private T GetTvShowClient<T>(DiscordSettings settings) where T : class
        {
            if (settings.TvShowDownloadClient == DownloadClient.Sonarr)
            {
                return _sonarrDownloadClient as T;
            }
            else if (settings.TvShowDownloadClient == DownloadClient.Ombi)
            {
                return _ombiDownloadClient as T;
            }
            else if (settings.TvShowDownloadClient == DownloadClient.Overseerr)
            {
                return _overseerrClient as T;
            }
            else
            {
                throw new Exception($"Invalid configured tv show download client {settings.TvShowDownloadClient}");
            }
        }

        private Task LogAsync(LogMessage log)
        {
            switch (log.Severity)
            {
                case LogSeverity.Critical:
                    _logger.LogCritical(log.Exception, $"[Discord] {log.Message}");

                    if (_client.ConnectionState == ConnectionState.Connected)
                    {
                        _logger.LogCritical($"[Discord] Disconnecting from Discord due to error");
                        _client.StopAsync().ContinueWith(x => _client.LogoutAsync());
                    }

                    break;
                case LogSeverity.Error:
                    _logger.LogError(log.Exception, $"[Discord] {log.Message}");
                    if (_client.ConnectionState == ConnectionState.Connected)
                    {
                        _logger.LogError($"[Discord] Disconnecting from Discord due to error");
                        _client.StopAsync().ContinueWith(x => _client.LogoutAsync());
                    }
                    break;
                case LogSeverity.Warning:
                    _logger.LogWarning(log.Exception, $"[Discord] {log.Message}");

                    if (log.Exception != null && _client.ConnectionState == ConnectionState.Connected)
                    {
                        _logger.LogWarning($"[Discord] Disconnecting from Discord due to error");
                        _client.StopAsync().ContinueWith(x => _client.LogoutAsync());
                    }
                    break;
                case LogSeverity.Info:
                    _logger.LogInformation(log.Exception, $"[Discord] {log.Message}");
                    break;
                case LogSeverity.Debug:
                    _logger.LogDebug(log.Exception, $"[Discord] {log.Message}");
                    break;
                case LogSeverity.Verbose:
                    _logger.LogTrace(log.Exception, $"[Discord] {log.Message}");
                    break;
            }

            return Task.CompletedTask;
        }

        private async Task Connected()
        {
            try
            {
                if (_movieNotificationEngine != null)
                {
                    await _movieNotificationEngine.StopAsync();
                }

                if (_currentSettings.MovieDownloadClient != DownloadClient.Disabled && _currentSettings.NotificationMode != NotificationMode.Disabled)
                {
                    IMovieNotifier movieNotifier = null;

                    if (_currentSettings.NotificationMode == NotificationMode.PrivateMessage)
                    {
                        movieNotifier = new PrivateMessageMovieNotifier(_client, _logger);
                    }
                    else if (_currentSettings.NotificationMode == NotificationMode.Channels)
                    {
                        movieNotifier = new ChannelMovieNotifier(_client, _currentSettings.NotificationChannels, _logger);
                    }
                    else
                    {
                        throw new Exception($"Could not create movie notifier of type \"{_currentSettings.NotificationMode}\"");
                    }

                    _movieNotificationEngine = new MovieNotificationEngine(GetMovieClient<IMovieSearcher>(_currentSettings), movieNotifier, _logger, _movieNotificationRequestRepository);
                    _movieNotificationEngine.Start();
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error while starting movie notification engine: " + ex.Message);
            }

            try
            {
                if (_tvShowNotificationEngine != null)
                {
                    await _tvShowNotificationEngine.StopAsync();
                }

                if (_currentSettings.TvShowDownloadClient != DownloadClient.Disabled && _currentSettings.NotificationMode != NotificationMode.Disabled)
                {
                    ITvShowNotifier tvShowNotifier = null;

                    if (_currentSettings.NotificationMode == NotificationMode.PrivateMessage)
                    {
                        tvShowNotifier = new PrivateMessageTvShowNotifier(_client, _discordSettingsProvider, _logger);
                    }
                    else if (_currentSettings.NotificationMode == NotificationMode.Channels)
                    {
                        tvShowNotifier = new ChannelTvShowNotifier(_client, _discordSettingsProvider, _currentSettings.NotificationChannels, _logger);
                    }
                    else
                    {
                        throw new Exception($"Could not create tv show notifier of type \"{_currentSettings.NotificationMode}\"");
                    }

                    _tvShowNotificationEngine = new TvShowNotificationEngine(GetTvShowClient<ITvShowSearcher>(_currentSettings), tvShowNotifier, _logger, _tvShowNotificationRequestRepository);
                    _tvShowNotificationEngine.Start();
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error while starting tv show notification engine: " + ex.Message);
            }
        }

        public async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            if (!(rawMessage is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User && message.Source != MessageSource.Webhook) return;

            var argPos = 0;

            if (!string.IsNullOrWhiteSpace(_currentSettings.CommandPrefix) && !message.HasStringPrefix(_currentSettings.CommandPrefix, ref argPos)) return;

            var context = new SocketCommandContext(_client, message);

            await _commandService.ExecuteAsync(context, argPos, _serviceProvider);
        }

        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (!command.IsSpecified)
                return;

            if (result.IsSuccess)
                return;

            if (result.Error == CommandError.BadArgCount)
            {
                await context.Channel.SendMessageAsync(command.Value.Summary);
                return;
            }

            if (result is ExecuteResult executeResult)
            {
                _logger.LogError(executeResult.Exception, $"An exception occurred while processing a command: {executeResult.Exception.Message}");
            }
            else
            {
                _logger.LogError($"An unknown occurred error while processing a command: {result.ErrorReason}");
            }

            await context.Channel.SendMessageAsync("An unexpected error occurred while trying to process your request.");
        }
    }
}