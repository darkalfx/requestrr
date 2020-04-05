using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;

namespace Requestrr.WebApi.RequestrrBot.TvShows
{
    public interface ITvShowUserInterface
    {
        Task<bool> AskForSeasonNotificationRequestAsync(TvShow tvShow, TvSeason requestedSeason);
        Task<bool> GetTvShowRequestConfirmationAsync(TvSeason season);
        Task<TvShowSelection> GetTvShowSelectionAsync(IReadOnlyList<SearchedTvShow> tvShows);
        Task<TvSeasonsSelection> GetTvShowSeasonSelectionAsync(TvShow tvShowDetails);
        Task DisplayTvShowDetailsAsync(TvShow tvShow);
        Task DisplayRequestSuccessForSeasonAsync(TvSeason selectedSeason);
        Task DisplayNotificationSuccessForSeasonAsync(TvSeason requestedSeason);
        Task WarnAlreadySeasonAlreadyRequestedAsync(TvShow tvShow, TvSeason selectedSeason);
        Task WarnInvalidSeasonSelectionAsync();
        Task WarnAlreadyNotifiedForSeasonsAsync(TvShow tvShow, TvSeason requestedSeason);
        Task WarnInvalidTvShowSelectionAsync();
        Task WarnNoTvShowFoundAsync(string tvShowName);
        Task WarnShowHasEndedAsync(TvShow tvShow);
        Task WarnSeasonAlreadyAvailableAsync(TvSeason requestedSeason);
        Task DisplayRequestDeniedForSeasonAsync(TvSeason selectedSeason);
        Task WarnShowCannotBeRequestedAsync(TvShow tvShow);
        Task WarnNoTvShowFoundByTvDbIdAsync(string tvDbIdTextValue);
    }

    public class TvShowSelection
    {
        public Optional<SearchedTvShow> SelectedTvShow { get; set; }
        public bool IsCancelled { get; set; }
    }

    public class TvSeasonsSelection
    {
        public Optional<TvSeason> SelectedSeason { get; set; }
        public bool IsCancelled { get; set; }
    }
}