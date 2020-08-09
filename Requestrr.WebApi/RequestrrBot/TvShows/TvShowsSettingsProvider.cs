namespace Requestrr.WebApi.RequestrrBot.TvShows
{

    public class TvShowsSettingsProvider
    {
        public TvShowsSettings Provide()
        {
            dynamic settings = SettingsFile.Read();

            return new TvShowsSettings
            {
                Client = settings.TvShows.Client,
                Command = settings.TvShows.Command,
                Restrictions = settings.TvShows.Restrictions,
            };
        }
    }
}