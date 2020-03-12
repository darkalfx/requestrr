using System.Linq;
using System.Threading.Tasks;
using Requestrr.WebApi.Requestrr.Notifications;

namespace Requestrr.WebApi.Requestrr.Movies
{
    public class MovieRequestingWorkflow
    {
        private readonly MovieUserRequester _user;
        private readonly IMovieSearcher _searcher;
        private readonly IMovieRequester _requester;
        private readonly IMovieUserInterface _userInterface;
        private readonly MovieNotificationsRepository _notificationRequestRepository;

        public MovieRequestingWorkflow(
            MovieUserRequester user,
            IMovieSearcher searcher,
            IMovieRequester requester,
            IMovieUserInterface userInterface,
            MovieNotificationsRepository notificationRequestRepository)
        {
            _user = user;
            _searcher = searcher;
            _requester = requester;
            _userInterface = userInterface;
            _notificationRequestRepository = notificationRequestRepository;
        }

        public async Task HandleMovieRequestAsync(string movieName)
        {
            var movies = await _searcher.SearchMovieAsync(movieName);

            if (!movies.Any())
            {
                await _userInterface.WarnNoMovieFoundAsync(movieName);
            }
            else if (movies.Count > 1)
            {
                var movieSelection = await _userInterface.GetMovieSelectionAsync(movies);

                if (!movieSelection.IsCancelled && movieSelection.Movie != null)
                {
                    var movie = movieSelection.Movie;
                    await HandleMovieSelectionAsync(movie);
                }
                else if (!movieSelection.IsCancelled)
                {
                    await _userInterface.WarnInvalidMovieSelectionAsync();
                }
            }
            else if (movies.Count == 1)
            {
                var movie = movies.Single();
                await HandleMovieSelectionAsync(movie);
            }
        }

        private async Task HandleMovieSelectionAsync(Movie movie)
        {
            await _userInterface.DisplayMovieDetails(movie);

            if (CanBeRequested(movie))
            {
                var isRequested = await _userInterface.GetMovieRequestAsync();

                if (isRequested)
                {
                    await _requester.RequestMovieAsync(_user.Username, movie);
                    await _userInterface.DisplayRequestSuccess(movie);
                    _notificationRequestRepository.AddNotification(_user.UserId, int.Parse(movie.TheMovieDbId));
                }
            }
            else
            {
                if (movie.Available)
                {
                    await _userInterface.WarnMovieAlreadyAvailable();
                }
                else if (IsAlreadyNotified(movie))
                {
                    await _userInterface.WarnMovieUnavailableAndAlreadyHasNotification();
                }
                else
                {
                    var isRequested = await _userInterface.AskForNotificationRequestAsync();

                    if (isRequested)
                    {
                        _notificationRequestRepository.AddNotification(_user.UserId, int.Parse(movie.TheMovieDbId));
                        await _userInterface.DisplayNotificationSuccessAsync(movie);
                    }
                }
            }
        }

        private bool IsAlreadyNotified(Movie movie)
        {
            return _notificationRequestRepository.HasNotification(_user.UserId, int.Parse(movie.TheMovieDbId));
        }

        private static bool CanBeRequested(Movie movie)
        {
            return !movie.Available && !movie.Requested && !movie.Approved;
        }
    }
}