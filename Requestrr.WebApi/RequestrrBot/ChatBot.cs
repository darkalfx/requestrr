using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Requestrr.WebApi.Extensions;
using Requestrr.WebApi.RequestrrBot.ChatClients.Discord;
using Requestrr.WebApi.RequestrrBot.DownloadClients;
using Requestrr.WebApi.RequestrrBot.DownloadClients.Ombi;
using Requestrr.WebApi.RequestrrBot.DownloadClients.Overseerr;
using Requestrr.WebApi.RequestrrBot.DownloadClients.Radarr;
using Requestrr.WebApi.RequestrrBot.DownloadClients.Sonarr;
using Requestrr.WebApi.RequestrrBot.Locale;
using Requestrr.WebApi.RequestrrBot.Movies;
using Requestrr.WebApi.RequestrrBot.Notifications;
using Requestrr.WebApi.RequestrrBot.Notifications.Movies;
using Requestrr.WebApi.RequestrrBot.Notifications.TvShows;
using Requestrr.WebApi.RequestrrBot.TvShows;

namespace Requestrr.WebApi.RequestrrBot
{
    public class ChatBot
    {
        private DiscordClient _client;
        private MovieNotificationEngine _movieNotificationEngine;
        private TvShowNotificationEngine _tvShowNotificationEngine;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ChatBot> _logger;
        private readonly DiscordSettingsProvider _discordSettingsProvider;
        private readonly ConcurrentBag<Func<Task>> _refreshQueue = new ConcurrentBag<Func<Task>>();
        private DiscordSettings _currentSettings = new DiscordSettings();
        private MovieWorkflowFactory _movieWorkflowFactory;
        private TvShowWorkflowFactory _tvShowWorkflowFactory;
        private MovieNotificationsRepository _movieNotificationRepository = new MovieNotificationsRepository();
        private TvShowNotificationsRepository _tvShowNotificationRepository = new TvShowNotificationsRepository();
        private OverseerrClient _overseerrClient;
        private OmbiClient _ombiDownloadClient;
        private RadarrClient _radarrDownloadClient;
        private SonarrClient _sonarrDownloadClient;
        private SlashCommandsExtension _slashCommands = null;
        private HashSet<ulong> _currentGuilds = new HashSet<ulong>();
        private Language _previousLanguage = Language.Current;


        public ChatBot(IServiceProvider serviceProvider, ILogger<ChatBot> logger, DiscordSettingsProvider discordSettingsProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _discordSettingsProvider = discordSettingsProvider;
            _overseerrClient = new OverseerrClient(serviceProvider.Get<IHttpClientFactory>(), serviceProvider.Get<ILogger<OverseerrClient>>(), serviceProvider.Get<OverseerrSettingsProvider>());
            _ombiDownloadClient = new OmbiClient(serviceProvider.Get<IHttpClientFactory>(), serviceProvider.Get<ILogger<OmbiClient>>(), serviceProvider.Get<OmbiSettingsProvider>());
            _radarrDownloadClient = new RadarrClient(serviceProvider.Get<IHttpClientFactory>(), serviceProvider.Get<ILogger<RadarrClient>>(), serviceProvider.Get<RadarrSettingsProvider>());
            _sonarrDownloadClient = new SonarrClient(serviceProvider.Get<IHttpClientFactory>(), serviceProvider.Get<ILogger<SonarrClient>>(), serviceProvider.Get<SonarrSettingsProvider>());
            _movieWorkflowFactory = new MovieWorkflowFactory(_discordSettingsProvider, _movieNotificationRepository, _overseerrClient, _ombiDownloadClient, _radarrDownloadClient);
            _tvShowWorkflowFactory = new TvShowWorkflowFactory(serviceProvider.Get<TvShowsSettingsProvider>(), _discordSettingsProvider, _tvShowNotificationRepository, _overseerrClient, _ombiDownloadClient, _sonarrDownloadClient);
        }

        public async void Start()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        var previousGuildCount = _currentGuilds.Count;
                        var newSettings = _discordSettingsProvider.Provide();

                        try
                        {
                            var newGuilds = new HashSet<ulong>(_client?.Guilds.Keys.ToArray() ?? Array.Empty<ulong>());

                            if (newGuilds.Any())
                            {
                                _currentGuilds.UnionWith(newGuilds);
                            }

                        }
                        catch (System.Exception) { }

                        if (!_currentSettings.Equals(newSettings) || Language.Current != _previousLanguage || _currentGuilds.Count != previousGuildCount)
                        {
                            var previousSettings = _currentSettings;
                            _logger.LogWarning("Bot configuration changed: restarting bot");
                            _currentSettings = newSettings;
                            _previousLanguage = Language.Current;
                            await RestartBot(previousSettings, newSettings, _currentGuilds);
                            _logger.LogWarning("Bot has been restarted.");

                            SlashCommandBuilder.CleanUp();
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

        private async Task RestartBot(DiscordSettings previousSettings, DiscordSettings newSettings, HashSet<ulong> currentGuilds)
        {
            if (!string.IsNullOrEmpty(newSettings.BotToken))
            {
                if (!string.Equals(previousSettings.BotToken, newSettings.BotToken, StringComparison.OrdinalIgnoreCase))
                {
                    if (_client != null)
                    {
                        await _client.DisconnectAsync();
                        _client.Ready -= Connected;
                        _client.ComponentInteractionCreated -= DiscordComponentInteractionCreatedHandler;
                        _client.Dispose();
                    }

                    if (_slashCommands != null)
                    {
                        _slashCommands.SlashCommandErrored -= SlashCommandErrorHandler;
                    }

                    _client = new DiscordClient(new DiscordConfiguration()
                    {
                        Token = newSettings.BotToken,
                        TokenType = TokenType.Bot,
                        AutoReconnect = true,
                        MinimumLogLevel = LogLevel.Warning
                    });

                    _slashCommands = _client.UseSlashCommands(new SlashCommandsConfiguration
                    {
                        Services = new ServiceCollection()
                            .AddSingleton<DiscordClient>(_client)
                            .AddSingleton<ILogger>(_logger)
                            .AddSingleton<DiscordSettingsProvider>(_discordSettingsProvider)
                            .AddSingleton<MovieWorkflowFactory>(_movieWorkflowFactory)
                            .AddSingleton<TvShowWorkflowFactory>(_tvShowWorkflowFactory)
                            .BuildServiceProvider()
                    });

                    _slashCommands.SlashCommandErrored += SlashCommandErrorHandler;

                    _client.Ready += Connected;
                    _client.ComponentInteractionCreated += DiscordComponentInteractionCreatedHandler;

                    _currentGuilds = new HashSet<ulong>();
                    await _client.ConnectAsync();
                }

                if (_client != null)
                {
                    if (_client.Guilds.Any())
                    {
                        await _client.UpdateStatusAsync(new DiscordActivity(newSettings.StatusMessage, ActivityType.Playing));
                        
                        var prop = _slashCommands.GetType().GetProperty("_updateList", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        prop.SetValue(_slashCommands, new List<KeyValuePair<ulong?, Type>>());
                        
                        var slashCommandType = SlashCommandBuilder.Build(_logger, newSettings);

                        if (newSettings.EnableRequestsThroughDirectMessages)
                        {
                            _slashCommands.RegisterCommands(slashCommandType);

                            foreach (var guildId in _client.Guilds.Keys)
                            {
                                _slashCommands.RegisterCommands<EmptySlashCommands>(guildId);
                            }
                        }
                        else
                        {
                            _slashCommands.RegisterCommands<EmptySlashCommands>();

                            foreach (var guildId in _client.Guilds.Keys)
                            {
                                _slashCommands.RegisterCommands(slashCommandType, guildId);
                            }
                        }

                        await _slashCommands.RefreshCommands();
                    }
                }
                else
                {
                    _logger.LogWarning("No Bot Token for Discord has been configured.");
                }
            }
        }

        private async Task ApplyBotConfigurationAsync(DiscordSettings discordSettings)
        {
            await _client.UpdateStatusAsync(new DiscordActivity(discordSettings.StatusMessage, ActivityType.Playing));
        }

        private async Task Connected(DiscordClient client, ReadyEventArgs args)
        {
            await ApplyBotConfigurationAsync(_currentSettings);

            try
            {
                if (_movieNotificationEngine != null)
                {
                    await _movieNotificationEngine.StopAsync();
                }

                if (_currentSettings.MovieDownloadClient != DownloadClient.Disabled && _currentSettings.NotificationMode != NotificationMode.Disabled)
                {
                    _movieNotificationEngine = _movieWorkflowFactory.CreateMovieNotificationEngine(_client, _logger);
                }

                _movieNotificationEngine?.Start();
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
                    _tvShowNotificationEngine = _tvShowWorkflowFactory.CreateTvShowNotificationEngine(_client, _logger);
                }

                _tvShowNotificationEngine?.Start();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error while starting tv show notification engine: " + ex.Message);
            }
        }

        private async Task DiscordComponentInteractionCreatedHandler(DiscordClient client, ComponentInteractionCreateEventArgs e)
        {
            try
            {
                await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage);
                var authorId = ulong.Parse(e.Id.Split("/").Skip(1).First());

                if (e.User.Id == authorId)
                {
                    if (e.Id.ToLower().StartsWith("mr"))
                    {
                        await HandleMovieRequestAsync(e);
                    }
                    else if (e.Id.ToLower().StartsWith("mnr"))
                    {
                        await CreateMovieNotificationWorkflow(e)
                            .AddNotificationAsync(e.Id.Split("/").Skip(1).First(), int.Parse(e.Id.Split("/").Last()));
                    }
                    if (e.Id.ToLower().StartsWith("tr") || e.Id.ToLower().StartsWith("ts"))
                    {
                        await HandleTvRequestAsync(e);
                    }
                    else if (e.Id.ToLower().StartsWith("tnr"))
                    {
                        var splitValues = e.Id.Split("/").Skip(1).ToArray();
                        var userId = splitValues[0];
                        var tvDbId = int.Parse(splitValues[1]);
                        var seasonData = splitValues[02];

                        await CreateTvShowNotificationWorkflow(e)
                            .AddNotificationAsync(userId, tvDbId, seasonData);
                    }
                }
            }
            catch (System.Exception ex)
            {
                await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().WithContent(Language.Current.Error));
                _logger.LogError(ex, "Error while handling interaction: " + ex.Message);
            }
        }

        private async Task SlashCommandErrorHandler(SlashCommandsExtension extension, SlashCommandErrorEventArgs args)
        {
            try
            {
                if (args.Exception is SlashExecutionChecksFailedException slex)
                {
                    foreach (var check in slex.FailedChecks)
                        if (check is RequireChannelsAttribute requireChannelAttribute)
                            await args.Context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral(true).WithContent(Language.Current.DiscordCommandNotAvailableInChannel));
                        else if (check is RequireRolesAttribute requireRoleAttribute)
                            await args.Context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral(true).WithContent(Language.Current.DiscordCommandMissingRoles));
                        else
                            await args.Context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral(true).WithContent(Language.Current.DiscordCommandUnknownPrecondition));
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error while handling interaction error: " + ex.Message);
            }
        }

        private async Task HandleMovieRequestAsync(ComponentInteractionCreateEventArgs e)
        {
            if (e.Id.ToLower().StartsWith("mrs"))
            {
                if (e.Values != null && e.Values.Any())
                {
                    await CreateMovieRequestWorkFlow(e)
                        .HandleMovieSelectionAsync(int.Parse(e.Values.Single()));
                }
            }
            else if (e.Id.ToLower().StartsWith("mrc"))
            {
                await CreateMovieRequestWorkFlow(e)
                    .RequestMovieAsync(int.Parse(e.Id.Split("/").Last()));
            }
        }

        private async Task HandleTvRequestAsync(ComponentInteractionCreateEventArgs e)
        {
            if (e.Id.ToLower().StartsWith("trs"))
            {
                if (e.Values != null && e.Values.Any())
                {
                    await CreateTvShowRequestWorkFlow(e)
                        .HandleTvShowSelectionAsync(int.Parse(e.Values.Single()));
                }
            }
            else if (e.Id.ToLower().StartsWith("tss"))
            {
                if (e.Values != null && e.Values.Any())
                {
                    var splitValues = e.Values.Single().Split("/");
                    var tvDbId = int.Parse(splitValues.First());
                    var seasonNumber = int.Parse(splitValues.Last());

                    await CreateTvShowRequestWorkFlow(e)
                        .HandleSeasonSelectionAsync(tvDbId, seasonNumber);
                }
            }
            else if (e.Id.ToLower().StartsWith("trc"))
            {
                var splitValues = e.Id.Split("/").Skip(2);
                var tvDbId = int.Parse(splitValues.First());
                var seasonNumber = int.Parse(splitValues.Last());

                await CreateTvShowRequestWorkFlow(e)
                    .RequestSeasonSelectionAsync(tvDbId, seasonNumber);
            }
        }

        private MovieRequestingWorkflow CreateMovieRequestWorkFlow(ComponentInteractionCreateEventArgs e)
        {
            return _movieWorkflowFactory
                .CreateRequestingWorkflow(e.Interaction);
        }

        private IMovieNotificationWorkflow CreateMovieNotificationWorkflow(ComponentInteractionCreateEventArgs e)
        {
            return _movieWorkflowFactory
                .CreateNotificationWorkflow(e.Interaction);
        }

        private TvShowRequestingWorkflow CreateTvShowRequestWorkFlow(ComponentInteractionCreateEventArgs e)
        {
            return _tvShowWorkflowFactory
                .CreateRequestingWorkflow(e.Interaction);
        }

        private ITvShowNotificationWorkflow CreateTvShowNotificationWorkflow(ComponentInteractionCreateEventArgs e)
        {
            return _tvShowWorkflowFactory
                .CreateNotificationWorkflow(e.Interaction);
        }
    }
}