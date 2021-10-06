namespace Requestrr.WebApi.RequestrrBot.Movies
{
    public class MovieRequest
    {
        public int CategoryId { get; }
        public MovieUserRequester User { get; }

        public MovieRequest(MovieUserRequester user, int categoryId)
        {
            User = user;
            CategoryId = categoryId;
        }
    }
}