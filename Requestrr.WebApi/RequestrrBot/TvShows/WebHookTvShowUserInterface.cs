// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;

// namespace Requestrr.WebApi.RequestrrBot.TvShows
// {
//     public class WebHookTvShowUserInterface : ITvShowUserInterface
//     {
//         private readonly string _seasonSpecified;
//         private readonly ITvShowUserInterface _tvShowUserInterface;
//         private TvShow _selectedTvShow;

//         public WebHookTvShowUserInterface(
//             ITvShowUserInterface tvShowUserInterface,
//             string seasonSpecified)
//         {
//             _seasonSpecified = seasonSpecified;
//             _tvShowUserInterface = tvShowUserInterface;
//         }

//         public Task<bool> AskForSeasonNotificationRequestAsync(TvShow tvShow, TvSeason requestedSeason)
//         {
//             return Task.FromResult(false);
//         }

//         public Task DisplayNotificationSuccessForSeasonAsync(TvSeason requestedSeason)
//         {
//             return Task.CompletedTask;
//         }

//         public Task DisplayRequestDeniedForSeasonAsync(TvSeason selectedSeason)
//         {
//             return Task.CompletedTask;
//         }

//         public async Task DisplayRequestSuccessForSeasonAsync(TvSeason selectedSeason)
//         {
//             await _tvShowUserInterface.DisplayTvShowDetailsAsync(_selectedTvShow);
//             await _tvShowUserInterface.DisplayRequestSuccessForSeasonAsync(selectedSeason);
//         }

//         public Task DisplayTvShowDetailsAsync(TvShow tvShow)
//         {
//             _selectedTvShow = tvShow;
//             return Task.CompletedTask;
//         }

//         public Task<bool> GetTvShowRequestConfirmationAsync(TvSeason season)
//         {
//             return Task.FromResult(true);
//         }

//         public Task<TvSeasonsSelection> GetTvShowSeasonSelectionAsync(TvShow tvShow)
//         {
//             var tvShowSeasons = tvShow.Seasons;

//             if (_seasonSpecified.ToLower().StartsWith("all") && tvShowSeasons.OfType<AllTvSeasons>().Any())
//             {
//                 return Task.FromResult(new TvSeasonsSelection
//                 {
//                     SelectedSeason = tvShowSeasons.OfType<AllTvSeasons>().First(),
//                 });
//             }
//             else if (_seasonSpecified.ToLower().StartsWith("future") && tvShowSeasons.OfType<FutureTvSeasons>().Any())
//             {
//                 return Task.FromResult(new TvSeasonsSelection
//                 {
//                     SelectedSeason = tvShowSeasons.OfType<FutureTvSeasons>().First(),
//                 });
//             }
//             else if (int.TryParse(_seasonSpecified, out var selectedSeasonNumber) && tvShowSeasons.Any(x => x.SeasonNumber == selectedSeasonNumber))
//             {
//                 var selectedSeason = tvShowSeasons.Single(x => x.SeasonNumber == selectedSeasonNumber);

//                 return Task.FromResult(new TvSeasonsSelection
//                 {
//                     SelectedSeason = selectedSeason
//                 });
//             }
//             else
//             {
//                 return Task.FromResult(new TvSeasonsSelection
//                 {
//                     IsCancelled = true
//                 });
//             }
//         }

//         public Task<TvShowSelection> GetTvShowSelectionAsync(IReadOnlyList<SearchedTvShow> tvShows)
//         {
//             return Task.FromResult(new TvShowSelection
//             {
//                 IsCancelled = false,
//                 SelectedTvShow = tvShows.First()
//             });
//         }

//         public Task WarnAlreadyNotifiedForSeasonsAsync(TvShow tvShow, TvSeason requestedSeason)
//         {
//             return Task.CompletedTask;
//         }

//         public Task WarnAlreadySeasonAlreadyRequestedAsync(TvShow tvShow, TvSeason selectedSeason)
//         {
//             return Task.CompletedTask;
//         }

//         public Task WarnInvalidSeasonSelectionAsync()
//         {
//             return Task.CompletedTask;
//         }

//         public Task WarnInvalidTvShowSelectionAsync()
//         {
//             return Task.CompletedTask;
//         }

//         public Task WarnNoTvShowFoundAsync(string tvShowName)
//         {
//             return Task.CompletedTask;
//         }

//         public Task WarnNoTvShowFoundByTvDbIdAsync(string tvDbIdTextValue)
//         {
//             return Task.CompletedTask;
//         }

//         public Task WarnSeasonAlreadyAvailableAsync(TvSeason requestedSeason)
//         {
//             return Task.CompletedTask;
//         }

//         public Task WarnShowCannotBeRequestedAsync(TvShow tvShow)
//         {
//             return Task.CompletedTask;
//         }

//         public Task WarnShowHasEndedAsync(TvShow tvShow)
//         {
//             return Task.CompletedTask;
//         }
//     }
// }