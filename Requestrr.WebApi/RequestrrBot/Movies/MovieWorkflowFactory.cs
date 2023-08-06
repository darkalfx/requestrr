using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Requestrr.WebApi.RequestrrBot.ChatClients.Discord;
using Requestrr.WebApi.RequestrrBot.DownloadClients;
using Requestrr.WebApi.RequestrrBot.DownloadClients.Ombi;
using Requestrr.WebApi.RequestrrBot.DownloadClients.Overseerr;
using Requestrr.WebApi.RequestrrBot.DownloadClients.Radarr;
using Requestrr.WebApi.RequestrrBot.Notifications;
using Requestrr.WebApi.RequestrrBot.Notifications.Movies;

namespace Requestrr.WebApi.RequestrrBot.Movies
{
    public class MovieWorkflowFactory
    {
        private readonly DiscordSettingsProvider _settingsProvider;
        private readonly MovieNotificationsRepository _notificationsRepository;
        private OverseerrClient _overseerrClient;
        private OmbiClient _ombiDownloadClient;
        private RadarrClient _radarrDownloadClient;

        public MovieWorkflowFactory(
            DiscordSettingsProvider settingsProvider,
            MovieNotificationsRepository notificationsRepository,
            OverseerrClient overseerrClient,
            OmbiClient ombiDownloadClient,
            RadarrClient radarrDownloadClient)
        {
            _settingsProvider = settingsProvider;
            _notificationsRepository = notificationsRepository;
            _overseerrClient = overseerrClient;
            _ombiDownloadClient = ombiDownloadClient;
            _radarrDownloadClient = radarrDownloadClient;
        }

        public MovieRequestingWorkflow CreateRequestingWorkflow(DiscordInteraction interaction, int categoryId)
        {
            var settings = _settingsProvider.Provide();

            return new MovieRequestingWorkflow(new MovieUserRequester(interaction.User.Id.ToString(),  interaction.User.Username),
                                                categoryId,
                                                GetMovieClient<IMovieSearcher>(settings),
                                                GetMovieClient<IMovieRequester>(settings),
                                                new DiscordMovieUserInterface(interaction, GetMovieClient<IMovieSearcher>(settings)),
                                                CreateMovieNotificationWorkflow(interaction, settings));
        }


        /// <summary>
        /// This handles creating a issue for a movie
        /// </summary>
        /// <param name="interaction"></param>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public MovieIssueWorkflow CreateIssueWorkflow(DiscordInteraction interaction, int categoryId)
        {
            var settings = _settingsProvider.Provide();

            return new MovieIssueWorkflow(new MovieUserRequester(interaction.User.Id.ToString(), interaction.User.Username),
                                                categoryId,
                                                GetMovieClient<IMovieSearcher>(settings),
                                                GetMovieClient<IMovieRequester>(settings),
                                                new DiscordMovieUserInterface(interaction, GetMovieClient<IMovieSearcher>(settings)),
                                                CreateMovieNotificationWorkflow(interaction, settings));
        }

        public IMovieNotificationWorkflow CreateNotificationWorkflow(DiscordInteraction interaction)
        {
            var settings = _settingsProvider.Provide();
            return CreateMovieNotificationWorkflow(interaction, settings);
        }

        public MovieNotificationEngine CreateMovieNotificationEngine(DiscordClient client, ILogger logger)
        {
            var settings = _settingsProvider.Provide();

            IMovieNotifier movieNotifier = null;

            if (settings.NotificationMode == NotificationMode.PrivateMessage)
            {
                movieNotifier = new PrivateMessageMovieNotifier(client, logger);
            }
            else if (settings.NotificationMode == NotificationMode.Channels)
            {
                movieNotifier = new ChannelMovieNotifier(client, settings.NotificationChannels.Select(x => ulong.Parse(x)).ToArray(), logger);
            }
            else
            {
                throw new Exception($"Could not create movie notifier of type \"{settings.NotificationMode}\"");
            }

            return new MovieNotificationEngine(GetMovieClient<IMovieSearcher>(settings), movieNotifier, logger, _notificationsRepository);
        }

        private IMovieNotificationWorkflow CreateMovieNotificationWorkflow(DiscordInteraction interaction, DiscordSettings settings)
        {
            var userInterface = new DiscordMovieUserInterface(interaction, GetMovieClient<IMovieSearcher>(settings));
            IMovieNotificationWorkflow movieNotificationWorkflow = new DisabledMovieNotificationWorkflow(userInterface);

            if (settings.NotificationMode != NotificationMode.Disabled)
            {
                movieNotificationWorkflow = new MovieNotificationWorkflow(_notificationsRepository, userInterface, GetMovieClient<IMovieSearcher>(settings), settings.AutomaticallyNotifyRequesters);
            }

            return movieNotificationWorkflow;
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
    }
}