using System.Threading.Tasks;


namespace Requestrr.WebApi.RequestrrBot.Movies
{
    public interface IMovieIssueRequester
    {
        Task<bool> SubmitMovieIssueAsync(int theMovieDbId, string issueName, string issueDescription);
    }
}
