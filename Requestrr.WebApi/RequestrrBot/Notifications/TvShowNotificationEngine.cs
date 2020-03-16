using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Requestrr.WebApi.RequestrrBot.TvShows;

namespace Requestrr.WebApi.RequestrrBot.Notifications
{
    public class TvShowNotificationEngine
    {
        private object _lock = new object();
        private readonly ITvShowSearcher _tvShowSearcher;
        private readonly UserTvShowNotifier _userTvShowNotifier;
        private readonly ILogger<ChatBot> _logger;
        private readonly TvShowNotificationsRepository _notificationRequestRepository;
        private Task _notificationTask = null;
        private CancellationTokenSource _tokenSource = new CancellationTokenSource();

        public TvShowNotificationEngine(
            ITvShowSearcher tvShowSearcher,
            UserTvShowNotifier userTvShowNotifier,
            ILogger<ChatBot> logger,
            TvShowNotificationsRepository notificationRequestRepository)
        {
            _tvShowSearcher = tvShowSearcher;
            _userTvShowNotifier = userTvShowNotifier;
            _logger = logger;
            _notificationRequestRepository = notificationRequestRepository;
        }

        public void Start()
        {
            _notificationTask = Task.Run(async () =>
             {
                 while (!_tokenSource.IsCancellationRequested)
                 {
                     try
                     {
                         var currentRequests = _notificationRequestRepository.GetAllTvShowNotifications();
                         var tvShows = (await _tvShowSearcher.GetTvShowDetailsAsync(new HashSet<int>(currentRequests.Keys), _tokenSource.Token)).ToDictionary(x => x.TheTvDbId, x => x);

                         foreach (var request in currentRequests.Where(x => tvShows.ContainsKey(x.Key)))
                         {
                             foreach (var notification in request.Value)
                             {
                                 try
                                 {
                                     if (_tokenSource.IsCancellationRequested)
                                         return;

                                     if (tvShows[notification.TvShowId].Seasons.Any(x => x.SeasonNumber == notification.SeasonNumber && x.IsAvailable))
                                     {
                                         await _userTvShowNotifier.NotifyAsync(notification.UserId, tvShows[notification.TvShowId], notification.SeasonNumber);

                                         if (notification is FutureSeasonsNotification)
                                         {
                                             _notificationRequestRepository.RemoveSeasonNotification(notification);
                                             _notificationRequestRepository.AddSeasonNotification(notification.UserId, notification.TvShowId, new FutureTvSeasons { SeasonNumber = notification.SeasonNumber + 1 });
                                         }
                                         else
                                         {
                                             _notificationRequestRepository.RemoveSeasonNotification(notification);
                                         }
                                     }
                                 }
                                 catch (Exception ex)
                                 {
                                     _logger.LogWarning("An error occurred while handling a tv show notification: " + ex.Message);
                                 }
                             }
                         }
                     }
                     catch (Exception ex)
                     {
                         _logger.LogWarning("An error occurred while retrieving all tv show notification: " + ex.Message);
                     }

                     await Task.Delay(TimeSpan.FromMinutes(5), _tokenSource.Token);
                 }
             }, _tokenSource.Token);
        }

        public async Task StopAsync()
        {
            try
            {
                _tokenSource.Cancel();
                await _notificationTask;
            }
            catch (System.Exception)
            {
                _tokenSource.Dispose();
                _tokenSource = new CancellationTokenSource();
            }
        }
    }
}