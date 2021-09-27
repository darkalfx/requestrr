using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Requestrr.WebApi.RequestrrBot.TvShows;

namespace Requestrr.WebApi.RequestrrBot.Notifications.TvShows
{
    public class TvShowNotificationEngine
    {
        private object _lock = new object();
        private readonly ITvShowSearcher _tvShowSearcher;
        private readonly ITvShowNotifier _notifier;
        private readonly ILogger _logger;
        private readonly TvShowNotificationsRepository _notificationRequestRepository;
        private Task _notificationTask = null;
        private CancellationTokenSource _tokenSource = new CancellationTokenSource();

        public TvShowNotificationEngine(
            ITvShowSearcher tvShowSearcher,
            ITvShowNotifier notifier,
            ILogger logger,
            TvShowNotificationsRepository notificationRequestRepository)
        {
            _tvShowSearcher = tvShowSearcher;
            _notifier = notifier;
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
                             var notificationsBySeasons = request.Value.GroupBy(x => x.SeasonNumber);

                             foreach (var seasonNotifications in notificationsBySeasons)
                             {
                                 var userIds = seasonNotifications.Select(x => x.UserId).ToArray();

                                 try
                                 {
                                     if (_tokenSource.IsCancellationRequested)
                                         return;

                                     if (tvShows[request.Key].Seasons.Any(x => x.SeasonNumber == seasonNotifications.Key && x.IsAvailable))
                                     {
                                         var notifiedUsers = await _notifier.NotifyAsync(userIds, tvShows[request.Key], seasonNotifications.Key, _tokenSource.Token);

                                         foreach (var sentNotification in seasonNotifications.Where(x => notifiedUsers.Contains(x.UserId)))
                                         {
                                             if (sentNotification is FutureSeasonsNotification)
                                             {
                                                 _notificationRequestRepository.RemoveSeasonNotification(sentNotification);
                                                 _notificationRequestRepository.AddSeasonNotification(sentNotification.UserId, sentNotification.TvShowId, new FutureTvSeasons { SeasonNumber = sentNotification.SeasonNumber + 1 });
                                             }
                                             else
                                             {
                                                 _notificationRequestRepository.RemoveSeasonNotification(sentNotification);
                                             }
                                         }
                                     }
                                 }
                                 catch (Exception ex)
                                 {
                                     _logger.LogError(ex, "An error occurred while handling a tv show notification: " + ex.Message);
                                 }
                             }
                         }
                     }
                     catch (Exception ex)
                     {
                         _logger.LogError(ex, "An error occurred while retrieving all tv show notification: " + ex.Message);
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