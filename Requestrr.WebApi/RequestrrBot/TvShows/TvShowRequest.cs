namespace Requestrr.WebApi.RequestrrBot.TvShows
{
    public class TvShowRequest
    {
        public int CategoryId { get; }
        public TvShowUserRequester User { get; }

        public TvShowRequest(TvShowUserRequester user, int categoryId)
        {
            User = user;
            CategoryId = categoryId;
        }
    }
}