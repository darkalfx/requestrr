namespace Requestrr.WebApi.RequestrrBot.Music
{
    public class MusicUserRequester
    {
        public string UserId { get; }
        public string Username { get; }

        public MusicUserRequester(string userId, string username)
        {
            UserId = userId;
            Username = username;
        }
    }
}