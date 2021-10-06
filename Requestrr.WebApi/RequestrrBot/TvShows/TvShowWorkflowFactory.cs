using System;
using System.Linq;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Requestrr.WebApi.RequestrrBot.ChatClients.Discord;
using Requestrr.WebApi.RequestrrBot.DownloadClients;
using Requestrr.WebApi.RequestrrBot.DownloadClients.Ombi;
using Requestrr.WebApi.RequestrrBot.DownloadClients.Overseerr;
using Requestrr.WebApi.RequestrrBot.DownloadClients.Sonarr;
using Requestrr.WebApi.RequestrrBot.Notifications;
using Requestrr.WebApi.RequestrrBot.Notifications.TvShows;

namespace Requestrr.WebApi.RequestrrBot.TvShows
{
    public class TvShowWorkflowFactory
    {
        private readonly TvShowsSettingsProvider _tvShowsSettingsProvider;
        private readonly DiscordSettingsProvider _settingsProvider;
        private readonly TvShowNotificationsRepository _notificationsRepository;
        private OverseerrClient _overseerrClient;
        private OmbiClient _ombiDownloadClient;
        private SonarrClient _sonarrDownloadClient;

        public TvShowWorkflowFactory(
            TvShowsSettingsProvider tvShowsSettingsProvider,
            DiscordSettingsProvider settingsProvider,
            TvShowNotificationsRepository notificationsRepository,
            OverseerrClient overseerrClient,
            OmbiClient ombiDownloadClient,
            SonarrClient radarrDownloadClient)
        {
            _tvShowsSettingsProvider = tvShowsSettingsProvider;
            _settingsProvider = settingsProvider;
            _notificationsRepository = notificationsRepository;
            _overseerrClient = overseerrClient;
            _ombiDownloadClient = ombiDownloadClient;
            _sonarrDownloadClient = radarrDownloadClient;
        }

        public TvShowRequestingWorkflow CreateRequestingWorkflow(DiscordInteraction interaction, int categoryId)
        {
            var settings = _settingsProvider.Provide();

            return new TvShowRequestingWorkflow(new TvShowUserRequester(interaction.User.Id.ToString(), interaction.User.Username),
                                                categoryId,
                                                GetTvShowClient<ITvShowSearcher>(settings),
                                                GetTvShowClient<ITvShowRequester>(settings),
                                                new DiscordTvShowUserInterface(interaction),
                                                CreateMovieNotificationWorkflow(interaction, settings, GetTvShowClient<ITvShowSearcher>(settings)),
                                                _tvShowsSettingsProvider.Provide());
        }

        public ITvShowNotificationWorkflow CreateNotificationWorkflow(DiscordInteraction interaction)
        {
            var settings = _settingsProvider.Provide();
            return CreateMovieNotificationWorkflow(interaction, settings, GetTvShowClient<ITvShowSearcher>(settings));
        }

        public TvShowNotificationEngine CreateTvShowNotificationEngine(DiscordClient client, ILogger logger)
        {
            var settings = _settingsProvider.Provide();

            ITvShowNotifier tvShowNotifier = null;

            if (settings.NotificationMode == NotificationMode.PrivateMessage)
            {
                tvShowNotifier = new PrivateMessageTvShowNotifier(client, _settingsProvider, logger);
            }
            else if (settings.NotificationMode == NotificationMode.Channels)
            {
                tvShowNotifier = new ChannelTvShowNotifier(client, _settingsProvider, settings.NotificationChannels.Select(x => ulong.Parse(x)).ToArray(), logger);
            }
            else
            {
                throw new Exception($"Could not create tv show notifier of type \"{settings.NotificationMode}\"");
            }

            return new TvShowNotificationEngine(GetTvShowClient<ITvShowSearcher>(settings), tvShowNotifier, logger, _notificationsRepository);
        }

        private ITvShowNotificationWorkflow CreateMovieNotificationWorkflow(DiscordInteraction interaction, DiscordSettings settings, ITvShowSearcher tvShowSearcher)
        {
            var userInterface = new DiscordTvShowUserInterface(interaction);
            ITvShowNotificationWorkflow movieNotificationWorkflow = new DisabledTvShowNotificationWorkflow(userInterface);

            if (settings.NotificationMode != NotificationMode.Disabled)
            {
                movieNotificationWorkflow = new TvShowNotificationWorkflow(_notificationsRepository, userInterface, tvShowSearcher, settings.AutomaticallyNotifyRequesters);
            }

            return movieNotificationWorkflow;
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
    }
}