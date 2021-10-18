namespace Requestrr.WebApi.RequestrrBot.Movies
{
    public class MoviesSettingsProvider
    {
        public MoviesSettings Provide()
        {
            dynamic settings = SettingsFile.Read();

            return new MoviesSettings
            {
                Client = settings.Movies.Client,
                Categories = settings.Movie.Categories,
            };
        }
    }
}