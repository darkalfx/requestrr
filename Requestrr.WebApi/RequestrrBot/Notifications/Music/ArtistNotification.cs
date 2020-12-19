using System;

namespace Requestrr.WebApi.RequestrrBot.Notifications.Music
{
    public class AlbumNotification : ArtistNotification
    {
        public AlbumNotification(string userId, string artistId)
        : base(userId, artistId)
        {
        }

        public override bool Equals(object obj)
        {
            return obj is ArtistNotification notification &&
                   UserId == notification.UserId &&
                   ArtistId == notification.ArtistId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(UserId, ArtistId);
        }
    }

    public class FutureAlbumsNotification : ArtistNotification
    {
        public FutureAlbumsNotification(string userId, string artistId)
        : base(userId, artistId)
        {
        }

        public override bool Equals(object obj)
        {
            return obj is ArtistNotification notification &&
                   UserId == notification.UserId &&
                   ArtistId == notification.ArtistId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(UserId, ArtistId, typeof(FutureAlbumsNotification).Name);
        }
    }

    public abstract class ArtistNotification
    {
        public string UserId { get; }
        public string ArtistId { get; }
        public int SeasonNumber { get; }

        public ArtistNotification(string userId, string artistId)
        {
            UserId = userId;
            ArtistId = artistId;
        }
    }
}