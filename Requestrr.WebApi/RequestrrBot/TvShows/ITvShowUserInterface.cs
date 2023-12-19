using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace Requestrr.WebApi.RequestrrBot.TvShows
{
    public interface ITvShowUserInterface
    {
        Task ShowTvShowSelection(TvShowRequest request, IReadOnlyList<SearchedTvShow> searchedTvShows);
        Task ShowTvShowIssueSelection(TvShowRequest request, IReadOnlyList<SearchedTvShow> searchedTvShows);
        Task WarnNoTvShowFoundAsync(string tvShowName);
        Task WarnNoTvShowFoundByTvDbIdAsync(int tvDbId);
        Task WarnShowCannotBeRequestedAsync(TvShow tvShow);
        Task WarnShowHasEndedAsync(TvShow tvShow);
        Task WarnAlreadySeasonAlreadyRequestedAsync(TvShow tvShow, TvSeason tvSeason);
        Task WarnSeasonAlreadyAvailableAsync(TvShow tvShow, TvSeason selectedSeason);
        Task DisplayTvShowDetailsForSeasonAsync(TvShowRequest request, TvShow tvShow, TvSeason tvSeasons);
        Task DisplayTvShowIssueDetailsAsync(TvShowRequest request, TvShow tvShow, string issue);
        Task DisplayTvShowIssueModalAsync(TvShowRequest request, TvShow tvShow, string issue);
        Task CompleteTvShowIssueModalRequestAsync(TvShow tvShow, bool success);
        Task DisplayMultiSeasonSelectionAsync(TvShowRequest request, TvShow tvShow, TvSeason[] tvSeasons);
        Task DisplayRequestDeniedForSeasonAsync(TvShow tvShow, TvSeason selectedSeason);
        Task DisplayRequestSuccessForSeasonAsync(TvShow tvShow, TvSeason selectedSeason);
        Task WarnAlreadyNotifiedForSeasonsAsync(TvShow tvShow, TvSeason selectedSeason);
        Task AskForSeasonNotificationRequestAsync(TvShow tvShow, TvSeason selectedSeason);
        Task DisplayNotificationSuccessForSeasonAsync(TvShow tvShow, TvSeason selectedSeason);
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