using Newtonsoft.Json;

namespace Requestrr.WebApi.RequestrrBot.Locale
{
    public static class LanguageTokens
    {
        public static string TvShowTitle = "[TvShowTitle]";
        public static string TvShowTVDBID = "[TvShowTVDBID]";
        public static string SeasonNumber = "[SeasonNumber]";
        public static string MovieTitle = "[MovieTitle]";
        public static string AuthorUsername = "[AuthorUsername]";
        public static string BotUsername = "[BotUsername]";
        public static string MovieTMDB = "[MovieTMDB]";
        public static string CommandPrefix = "[CommandPrefix]";
        public static string TvShowCommandTvDb = "[TvShowCommandTvDb]";
        public static string MovieCommandTmDb = "[MovieCommandTmDb]";
        public static string MovieCommandTitle = "[MovieCommandTitle]";
        public static string TvShowCommandTitle = "[TvShowCommandTitle]";
    }

    public class Language
    {
        public static Language Current = null;

        public string Error { get; set; }

        [JsonProperty("Discord.Command.Movie.Request.Help.Dropdown")]
        public string DiscordCommandMovieRequestHelpDropdown { get; set; }

        [JsonProperty("Discord.Command.Tv.Request.Help.Search.Dropdown")]
        public string DiscordCommandTvRequestHelpSearchDropdown { get; set; }

        [JsonProperty("Discord.Command.Tv.Request.Help.Seasons.Dropdown")]
        public string DiscordCommandTvRequestHelpSeasonsDropdown { get; set; }

        [JsonProperty("Discord.Command.MissingRoles")]
        public string DiscordCommandMissingRoles { get; set; }

        [JsonProperty("Discord.Command.NotAvailableInChannel")]
        public string DiscordCommandNotAvailableInChannel { get; set; }

        [JsonProperty("Discord.Command.UnknownPrecondition")]
        public string DiscordCommandUnknownPrecondition { get; set; }

        [JsonProperty("Discord.Command.Sending")]
        public string DiscordCommandSending { get; set; }

        [JsonProperty("Discord.Command.Request.Group.Name")]
        public string DiscordCommandRequestGroupName { get; set; }

        [JsonProperty("Discord.Command.Request.Group.Description")]
        public string DiscordCommandRequestGroupDescription { get; set; }

        [JsonProperty("Discord.Command.Movie.Request.Title.Name")]
        public string DiscordCommandMovieRequestTitleName { get; set; }

        [JsonProperty("Discord.Command.Movie.Request.Title.Description")]
        public string DiscordCommandMovieRequestTitleDescription { get; set; }

        [JsonProperty("Discord.Command.Movie.Request.Title.Option.Name")]
        public string DiscordCommandMovieRequestTitleOptionName { get; set; }

        [JsonProperty("Discord.Command.Movie.Request.Title.Option.Description")]
        public string DiscordCommandMovieRequestTitleOptionDescription { get; set; }

        [JsonProperty("Discord.Command.Movie.Request.Tmbd.Name")]
        public string DiscordCommandMovieRequestTmbdName { get; set; }

        [JsonProperty("Discord.Command.Movie.Request.Tmbd.Description")]
        public string DiscordCommandMovieRequestTmbdDescription { get; set; }

        [JsonProperty("Discord.Command.Movie.Request.Tmbd.Option.Name")]
        public string DiscordCommandMovieRequestTmbdOptionName { get; set; }

        [JsonProperty("Discord.Command.Movie.Request.Tmbd.Option.Description")]
        public string DiscordCommandMovieRequestTmbdOptionDescription { get; set; }

        [JsonProperty("Discord.Command.Tv.Request.Title.Name")]
        public string DiscordCommandTvRequestTitleName { get; set; }

        [JsonProperty("Discord.Command.Tv.Request.Title.Description")]
        public string DiscordCommandTvRequestTitleDescription { get; set; }

        [JsonProperty("Discord.Command.Tv.Request.Title.Option.Name")]
        public string DiscordCommandTvRequestTitleOptionName { get; set; }

        [JsonProperty("Discord.Command.Tv.Request.Title.Option.Description")]
        public string DiscordCommandTvRequestTitleOptionDescription { get; set; }

        [JsonProperty("Discord.Command.Tv.Request.Tvdb.Name")]
        public string DiscordCommandTvRequestTvdbName { get; set; }

        [JsonProperty("Discord.Command.Tv.Request.Tvdb.Description")]
        public string DiscordCommandTvRequestTvdbDescription { get; set; }

        [JsonProperty("Discord.Command.Tv.Request.Tvdb.Option.Name")]
        public string DiscordCommandTvRequestTvdbOptionName { get; set; }

        [JsonProperty("Discord.Command.Tv.Request.Tvdb.Option.Description")]
        public string DiscordCommandTvRequestTvdbOptionDescription { get; set; }

        [JsonProperty("Discord.Command.Ping.Request.Name")]
        public string DiscordCommandPingRequestName { get; set; }

        [JsonProperty("Discord.Command.Ping.Request.Description")]
        public string DiscordCommandPingRequestDescription { get; set; }

        [JsonProperty("Discord.Command.Help.Request.Name")]
        public string DiscordCommandHelpRequestName { get; set; }

        [JsonProperty("Discord.Command.Help.Request.Description")]
        public string DiscordCommandHelpRequestDescription { get; set; }

        [JsonProperty("Discord.Notification.Tv.Channel.Season")]
        public string DiscordNotificationTvChannelSeason { get; set; }

        [JsonProperty("Discord.Notification.Tv.ChannelFirstEpisode")]
        public string DiscordNotificationTvChannelFirstEpisode { get; set; }

        [JsonProperty("Discord.Notification.Tv.DM.Season")]
        public string DiscordNotificationTvDMSeason { get; set; }

        [JsonProperty("Discord.Notification.Tv.DM.FirstEpisode")]
        public string DiscordNotificationTvDMFirstEpisode { get; set; }

        [JsonProperty("Discord.Notification.Movie.Channel")]
        public string DiscordNotificationMovieChannel { get; set; }

        [JsonProperty("Discord.Notification.Movie.DM")]
        public string DiscordNotificationMovieDM { get; set; }

        [JsonProperty("Discord.Command.Ping.Response")]
        public string DiscordCommandPingResponse { get; set; }

        [JsonProperty("Discord.Command.Help")]
        public string DiscordCommandHelp { get; set; }

        [JsonProperty("Discord.Command.Help.Message")]
        public string DiscordCommandHelpMessage { get; set; }

        [JsonProperty("Discord.Command.Movie.NotFound")]
        public string DiscordCommandMovieNotFound { get; set; }

        [JsonProperty("Discord.Command.Movie.NotFoundTMDB")]
        public string DiscordCommandMovieNotFoundTMDB { get; set; }

        [JsonProperty("Discord.Command.Movie.AlreadyAvailable")]
        public string DiscordCommandMovieAlreadyAvailable { get; set; }

        [JsonProperty("Discord.Command.Movie.Request.Success")]
        public string DiscordCommandMovieRequestSuccess { get; set; }

        [JsonProperty("Discord.Command.RequestButtonDenied")]
        public string DiscordCommandRequestButtonDenied { get; set; }

        [JsonProperty("Discord.Command.RequestButtonSuccess")]
        public string DiscordCommandRequestButtonSuccess { get; set; }

        [JsonProperty("Discord.Command.NotifyMe")]
        public string DiscordCommandNotifyMe { get; set; }

        [JsonProperty("Discord.Command.NotifyMeSuccess")]
        public string DiscordCommandNotifyMeSuccess { get; set; }

        [JsonProperty("Discord.Command.RequestedButton")]
        public string DiscordCommandRequestedButton { get; set; }

        [JsonProperty("Discord.Command.AvailableButton")]
        public string DiscordCommandAvailableButton { get; set; }

        [JsonProperty("Discord.Command.RequestButton")]
        public string DiscordCommandRequestButton { get; set; }

        [JsonProperty("Discord.Command.Movie.Request.Help")]
        public string DiscordCommandMovieRequestHelp { get; set; }

        [JsonProperty("Discord.Command.Movie.Request.Confirm")]
        public string DiscordCommandMovieRequestConfirm { get; set; }

        [JsonProperty("Discord.Command.Movie.Request.AlreadyExist")]
        public string DiscordCommandMovieRequestAlreadyExist { get; set; }

        [JsonProperty("Discord.Command.Movie.Request.AlreadyExistNotified")]
        public string DiscordCommandMovieRequestAlreadyExistNotified { get; set; }

        [JsonProperty("Discord.Command.Movie.Request.Denied")]
        public string DiscordCommandMovieRequestDenied { get; set; }

        [JsonProperty("Discord.Command.Movie.NotReleased")]
        public string DiscordCommandMovieNotReleased { get; set; }

        [JsonProperty("Discord.Command.Movie.Notification.Success")]
        public string DiscordCommandMovieNotificationSuccess { get; set; }

        [JsonProperty("Discord.Command.Movie.Notification.Request")]
        public string DiscordCommandMovieNotificationRequest { get; set; }

        [JsonProperty("Discord.Command.Movie.CancelCommand")]
        public string DiscordCommandMovieCancelCommand { get; set; }

        [JsonProperty("Discord.Embed.Movie.Search")]
        public string DiscordEmbedMovieSearch { get; set; }

        [JsonProperty("Discord.Embed.Movie.SearchResult")]
        public string DiscordEmbedMovieSearchResult { get; set; }

        [JsonProperty("Discord.Embed.Movie.Quality")]
        public string DiscordEmbedMovieQuality { get; set; }

        [JsonProperty("Discord.Embed.Movie.InTheaters")]
        public string DiscordEmbedMovieInTheaters { get; set; }

        [JsonProperty("Discord.Embed.Movie.Release")]
        public string DiscordEmbedMovieRelease { get; set; }

        [JsonProperty("Discord.Embed.Movie.WatchNow")]
        public string DiscordEmbedMovieWatchNow { get; set; }

        [JsonProperty("Discord.Command.Tv.Request.Help.Search")]
        public string DiscordCommandTvRequestHelpSearch { get; set; }

        [JsonProperty("Discord.Command.Tv.Request.Help.Seasons")]
        public string DiscordCommandTvRequestHelpSeasons { get; set; }

        [JsonProperty("Discord.Command.Tv.Request.Confirm.Season")]
        public string DiscordCommandTvRequestConfirmSeason { get; set; }

        [JsonProperty("Discord.Command.Tv.Request.Confirm.FutureSeasons")]
        public string DiscordCommandTvRequestConfirmFutureSeasons { get; set; }

        [JsonProperty("Discord.Command.Tv.Request.Confirm.AllSeasons")]
        public string DiscordCommandTvRequestConfirmAllSeasons { get; set; }

        [JsonProperty("Discord.Command.Tv.Request.Success.Season")]
        public string DiscordCommandTvRequestSuccessSeason { get; set; }

        [JsonProperty("Discord.Command.Tv.Request.Success.FutureSeasons")]
        public string DiscordCommandTvRequestSuccessFutureSeasons { get; set; }

        [JsonProperty("Discord.Command.Tv.Request.Success.AllSeasons")]
        public string DiscordCommandTvRequestSuccessAllSeasons { get; set; }

        [JsonProperty("Discord.Command.Tv.Request.Unsupported")]
        public string DiscordCommandTvRequestUnsupported { get; set; }

        [JsonProperty("Discord.Command.Tv.Request.AlreadyAvailable.Series")]
        public string DiscordCommandTvRequestAlreadyAvailableSeries { get; set; }

        [JsonProperty("Discord.Command.Tv.Request.AlreadyAvailable.Season")]
        public string DiscordCommandTvRequestAlreadyAvailableSeason { get; set; }

        [JsonProperty("Discord.Command.Tv.Request.AlreadyExist.Series")]
        public string DiscordCommandTvRequestAlreadyExistSeries { get; set; }

        [JsonProperty("Discord.Command.Tv.Request.AlreadyExist.Season")]
        public string DiscordCommandTvRequestAlreadyExistSeason { get; set; }

        [JsonProperty("Discord.Command.Tv.Request.AlreadyExist.FutureSeason.Found")]
        public string DiscordCommandTvRequestAlreadyExistFutureSeasonFound { get; set; }

        [JsonProperty("Discord.Command.Tv.Request.AlreadyExist.FutureSeason.Requested")]
        public string DiscordCommandTvRequestAlreadyExistFutureSeasonRequested { get; set; }

        [JsonProperty("Discord.Command.Tv.Request.AlreadyExist.FutureSeason.Available")]
        public string DiscordCommandTvRequestAlreadyExistFutureSeasonAvailable { get; set; }

        [JsonProperty("Discord.Command.Tv.Request.AlreadyExistNotified.Season")]
        public string DiscordCommandTvRequestAlreadyExistNotifiedSeason { get; set; }

        [JsonProperty("Discord.Command.Tv.Request.AlreadyExistNotified.FutureSeason.Found")]
        public string DiscordCommandTvRequestAlreadyExistNotifiedFutureSeasonFound { get; set; }

        [JsonProperty("Discord.Command.Tv.Request.AlreadyExistNotified.FutureSeason.Requested")]
        public string DiscordCommandTvRequestAlreadyExistNotifiedFutureSeasonRequested { get; set; }

        [JsonProperty("Discord.Command.Tv.Request.AlreadyExistNotified.FutureSeason.Available")]
        public string DiscordCommandTvRequestAlreadyExistNotifiedFutureSeasonAvailable { get; set; }

        [JsonProperty("Discord.Command.Tv.Request.Denied")]
        public string DiscordCommandTvRequestDenied { get; set; }

        [JsonProperty("Discord.Command.Tv.NotFoundTVDBID")]
        public string DiscordCommandTvNotFoundTVDBID { get; set; }

        [JsonProperty("Discord.Command.Tv.NotFound")]
        public string DiscordCommandTvNotFound { get; set; }

        [JsonProperty("Discord.Command.Tv.Notification.Request.Season")]
        public string DiscordCommandTvNotificationRequestSeason { get; set; }

        [JsonProperty("Discord.Command.Tv.Notification.Request.FutureSeason.Missing")]
        public string DiscordCommandTvNotificationRequestFutureSeasonMissing { get; set; }

        [JsonProperty("Discord.Command.Tv.Notification.Request.FutureSeason.Requested")]
        public string DiscordCommandTvNotificationRequestFutureSeasonRequested { get; set; }

        [JsonProperty("Discord.Command.Tv.Notification.Request.FutureSeason.Available")]
        public string DiscordCommandTvNotificationRequestFutureSeasonAvailable { get; set; }

        [JsonProperty("Discord.Command.Tv.Notification.Success.FutureSeasons")]
        public string DiscordCommandTvNotificationSuccessFutureSeasons { get; set; }

        [JsonProperty("Discord.Command.Tv.Notification.Success.Season")]
        public string DiscordCommandTvNotificationSuccessSeason { get; set; }

        [JsonProperty("Discord.Command.Tv.CancelCommand")]
        public string DiscordCommandTvCancelCommand { get; set; }

        [JsonProperty("Discord.Embed.Tv.Search")]
        public string DiscordEmbedTvSearch { get; set; }

        [JsonProperty("Discord.Embed.Tv.SearchResult")]
        public string DiscordEmbedTvSearchResult { get; set; }

        [JsonProperty("Discord.Embed.Tv.Network")]
        public string DiscordEmbedTvNetwork { get; set; }

        [JsonProperty("Discord.Embed.Tv.Status")]
        public string DiscordEmbedTvStatus { get; set; }

        [JsonProperty("Discord.Embed.Tv.Quality")]
        public string DiscordEmbedTvQuality { get; set; }

        [JsonProperty("Discord.Embed.Tv.AllSeasons")]
        public string DiscordEmbedTvAllSeasons { get; set; }

        [JsonProperty("Discord.Embed.Tv.FutureSeasons")]
        public string DiscordEmbedTvFutureSeasons { get; set; }

        [JsonProperty("Discord.Embed.Tv.Season")]
        public string DiscordEmbedTvSeason { get; set; }

        [JsonProperty("Discord.Embed.Tv.FullyRequested")]
        public string DiscordEmbedTvFullyRequested { get; set; }

        [JsonProperty("Discord.Embed.Tv.PartiallyRequested")]
        public string DiscordEmbedTvPartiallyRequested { get; set; }

        [JsonProperty("Discord.Embed.Tv.Seasons")]
        public string DiscordEmbedTvSeasons { get; set; }

        [JsonProperty("Discord.Embed.Tv.WatchNow")]
        public string DiscordEmbedTvWatchNow { get; set; }
    }
}