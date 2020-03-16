namespace Requestrr.WebApi.RequestrrBot.TvShows
{
    public class TvShowUserRequester
    {
        public string UserId { get; }
        public string Username { get; }

        public TvShowUserRequester(string userId, string username)
        {
            UserId = userId;
            Username = username;
        }
    }
}