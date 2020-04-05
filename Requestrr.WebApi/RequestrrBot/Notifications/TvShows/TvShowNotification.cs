using System;

namespace Requestrr.WebApi.RequestrrBot.Notifications.TvShows
{
    public class SeasonNotification : TvShowNotification
    {
        public SeasonNotification(string userId, int tvShowId, int seasonNumber)
        : base(userId, tvShowId, seasonNumber)
        {
        }

        public override bool Equals(object obj)
        {
            return obj is TvShowNotification notification &&
                   UserId == notification.UserId &&
                   TvShowId == notification.TvShowId &&
                   SeasonNumber == notification.SeasonNumber;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(UserId, TvShowId, SeasonNumber);
        }
    }

    public class FutureSeasonsNotification : TvShowNotification
    {
        public FutureSeasonsNotification(string userId, int tvShowId, int seasonNumber)
        : base(userId, tvShowId, seasonNumber)
        {
        }

        public FutureSeasonsNotification(string userId, int tvShowId)
        : base(userId, tvShowId, -1)
        {
        }

        public override bool Equals(object obj)
        {
            return obj is TvShowNotification notification &&
                   UserId == notification.UserId &&
                   TvShowId == notification.TvShowId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(UserId, TvShowId, typeof(FutureSeasonsNotification).Name);
        }
    }

    public abstract class TvShowNotification
    {
        public string UserId { get; }
        public int TvShowId { get; }
        public int SeasonNumber { get; }

        public TvShowNotification(string userId, int tvShowId, int seasonNumber)
        {
            UserId = userId;
            TvShowId = tvShowId;
            SeasonNumber = seasonNumber;
        }
    }
}