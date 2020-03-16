using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Requestrr.WebApi.Extensions;
using Requestrr.WebApi.RequestrrBot.Movies;

namespace Requestrr.WebApi.RequestrrBot.DownloadClients
{
    public static class TheMovieDb
    {
        public static async Task<HttpResponseMessage> GetMovieFromTheMovieDbAsync(HttpClient client, string theMovieDbId)
        {
            return await HttpGetAsync(client, $"https://api.themoviedb.org/3/movie/{theMovieDbId}?api_key=0d3ad5bb96218b24cd6917ccd6d673bc&language=en-US");
        }

        public static async Task<MovieDetails> GetMovieDetailsAsync(HttpClient client, string theMovieDbId, ILogger logger)
        {
            try
            {
                var response = await HttpGetAsync(client, $"https://api.themoviedb.org/3/movie/{theMovieDbId}/release_dates?api_key=0d3ad5bb96218b24cd6917ccd6d673bc");
                await response.ThrowIfNotSuccessfulAsync("TheMovieDbFindReleaseDates failed", x => x.status_message);

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var jsonReleaseDetails = JsonConvert.DeserializeObject<MovieReleaseDetailsResult>(jsonResponse);

                var usReleaseDates = jsonReleaseDetails.results.FirstOrDefault(x => x.iso_3166_1.Equals("us", StringComparison.InvariantCultureIgnoreCase));

                if (usReleaseDates != null)
                {
                    var theaterRelease = usReleaseDates.release_dates.FirstOrDefault(x => x.type == 3);
                    var physicalRelease = usReleaseDates.release_dates.FirstOrDefault(x => x.type == 4 || x.type == 5);

                    return new MovieDetails
                    {
                        InTheatersDate = theaterRelease != null ? theaterRelease.release_date.ToString("MMMM dd yyyy", DateTimeFormatInfo.InvariantInfo) : null,
                        PhysicalReleaseName = physicalRelease != null ? physicalRelease.type == 5 ? "Physical" : "Digital" : null,
                        PhysicalReleaseDate = physicalRelease != null ? physicalRelease.release_date.ToString("MMMM dd yyyy", DateTimeFormatInfo.InvariantInfo) : null,
                    };
                }

                return new MovieDetails
                {
                    InTheatersDate = ""
                };
            }
            catch (System.Exception ex)
            {
                logger.LogWarning(ex, ex.Message);
                throw;
            }
        }

        private static async Task<HttpResponseMessage> HttpGetAsync(HttpClient client, string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Accept", "application/json");

            return await client.SendAsync(request);
        }

        public class MovieReleaseDetailsResult
        {
            public List<MovieReleaseDetails> results { get; set; }
        }

        public class MovieReleaseDetails
        {
            public string iso_3166_1 { get; set; }
            public List<MovieReleaseDate> release_dates { get; set; }
        }

        public class MovieReleaseDate
        {
            public DateTime release_date { get; set; }
            public int type { get; set; }
        }
    }
}