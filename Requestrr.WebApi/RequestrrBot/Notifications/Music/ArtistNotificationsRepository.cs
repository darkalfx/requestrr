using System;
using System.Collections.Generic;
using System.Linq;

namespace Requestrr.WebApi.RequestrrBot.Notifications.Music
{
    public class ArtistNotificationsRepository
    {
        private class Notification
        {
            public string UserId { get; }
            public string ArtistId { get; }
            public Notification(string userId, string artistId)
            {
                UserId = userId;
                ArtistId = artistId;
            }

            public override bool Equals(object obj)
            {
                return obj is Notification notification &&
                       UserId == notification.UserId &&
                       ArtistId == notification.ArtistId;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(UserId, ArtistId);
            }
        }

        private HashSet<Notification> _notifications = new HashSet<Notification>();

        private object _lock = new object();

        public ArtistNotificationsRepository()
        {
            foreach (var notif in NotificationsFile.Read().Music)
            {
                foreach (var artistId in notif.MovieIds)
                {
                    _notifications.Add(new Notification(notif.UserId.ToString(), artistId));
                }
            }
        }

        public void AddNotification(string userId, string artistId)
        {
            lock (_lock)
            {
                if (_notifications.Add(new Notification(userId, artistId)))
                {
                    NotificationsFile.WriteArtists(_notifications.GroupBy(x => x.UserId).ToDictionary(x => x.Key, x => x.Select(n => n.ArtistId).ToArray()));
                }
            }
        }

        public void RemoveNotification(string userId, string artistId)
        {
            lock (_lock)
            {
                if (_notifications.Remove(new Notification(userId, artistId)))
                {
                    NotificationsFile.WriteArtists(_notifications.GroupBy(x => x.UserId).ToDictionary(x => x.Key, x => x.Select(n => n.ArtistId).ToArray()));
                }
            }
        }

        public Dictionary<string, HashSet<string>> GetAllMovieNotifications()
        {
            lock (_lock)
            {
                return _notifications
                    .GroupBy(x => x.ArtistId)
                    .ToDictionary(x => x.Key, x => new HashSet<string>(x.Select(r => r.UserId)));
            }
        }

        public bool HasNotification(string userId, string artistId)
        {
            var hasRequest = false;

            lock (_lock)
            {
                hasRequest = _notifications.Contains(new Notification(userId, artistId));
            }

            return hasRequest;
        }
    }
}