using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Requestrr.WebApi.Requestrr.Movies;

namespace Requestrr.WebApi.Requestrr.Notifications
{
    public class MovieNotificationEngine
    {
        private object _lock = new object();
        private readonly IMovieSearcher _movieSearcher;
        private readonly UserMovieNotifier _userMovieNotifier;
        private readonly ILogger<RequestrrBot> _logger;
        private readonly MovieNotificationsRepository _notificationRequestRepository;
        private Task _notificationTask = null;
        private CancellationTokenSource _tokenSource = new CancellationTokenSource();

        public MovieNotificationEngine(
            IMovieSearcher movieSearcher,
            UserMovieNotifier userMovieNotifier,
            ILogger<RequestrrBot> logger,
            MovieNotificationsRepository notificationRequestRepository)
        {
            _movieSearcher = movieSearcher;
            _userMovieNotifier = userMovieNotifier;
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
                         var currentRequests = _notificationRequestRepository.GetAllMovieNotifications();
                         var availableMovies = await _movieSearcher.SearchAvailableMoviesAsync(new HashSet<int>(currentRequests.Keys), _tokenSource.Token);

                         foreach (var request in currentRequests.Where(x => availableMovies.ContainsKey(x.Key)))
                         {
                             foreach (var userId in request.Value)
                             {
                                 if (_tokenSource.IsCancellationRequested)
                                     return;

                                 try
                                 {
                                     await _userMovieNotifier.NotifyAsync(userId, availableMovies[request.Key]);
                                     _notificationRequestRepository.RemoveNotification(userId, request.Key);
                                 }
                                 catch (Exception ex)
                                 {
                                     _logger.LogWarning("An error occurred while handling a movie notification: " + ex.Message);
                                 }
                             }
                         }
                     }
                     catch (Exception ex)
                     {
                         _logger.LogWarning("An error occurred while retrieving all movie notification: " + ex.Message);
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