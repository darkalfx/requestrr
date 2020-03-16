using System;
using System.Collections.Generic;
using System.Linq;

namespace Requestrr.WebApi.RequestrrBot.Notifications
{
    public class MovieNotificationsRepository
    {
        private class Notification
        {
            public string UserId { get; }
            public int MovieId { get; }
            public Notification(string userId, int movieId)
            {
                UserId = userId;
                MovieId = movieId;
            }

            public override bool Equals(object obj)
            {
                return obj is Notification notification &&
                       UserId == notification.UserId &&
                       MovieId == notification.MovieId;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(UserId, MovieId);
            }
        }

        private HashSet<Notification> _notifications = new HashSet<Notification>();

        private object _lock = new object();

        public MovieNotificationsRepository()
        {
            foreach (var notif in NotificationsFile.Read().Movies)
            {
                foreach (var movieId in notif.MovieIds)
                {
                    _notifications.Add(new Notification(notif.UserId.ToString(), (int)movieId));
                }
            }
        }

        public void AddNotification(string userId, int movieId)
        {
            lock (_lock)
            {
                if (_notifications.Add(new Notification(userId, movieId)))
                {
                    NotificationsFile.WriteMovies(_notifications.GroupBy(x => x.UserId).ToDictionary(x => x.Key, x => x.Select(n => n.MovieId).ToArray()));
                }
            }
        }

        public void RemoveNotification(string userId, int movieId)
        {
            lock (_lock)
            {
                if (_notifications.Remove(new Notification(userId, movieId)))
                {
                    NotificationsFile.WriteMovies(_notifications.GroupBy(x => x.UserId).ToDictionary(x => x.Key, x => x.Select(n => n.MovieId).ToArray()));
                }
            }
        }

        public Dictionary<int, HashSet<string>> GetAllMovieNotifications()
        {
            lock (_lock)
            {
                return _notifications
                    .GroupBy(x => x.MovieId)
                    .ToDictionary(x => x.Key, x => new HashSet<string>(x.Select(r => r.UserId)));
            }
        }

        public bool HasNotification(string userId, int movieId)
        {
            var hasRequest = false;

            lock (_lock)
            {
                hasRequest = _notifications.Contains(new Notification(userId, movieId));
            }

            return hasRequest;
        }
    }
}