using System;
using System.ComponentModel.DataAnnotations;

namespace Requestrr.WebApi.Controllers.DownloadClients.Overseerr
{
    public class SaveOverseerrMoviesSettingsModel : OverseerrSettingsModel
    {
        [Required]
        public bool UseMovieIssue { get; set; }


        [Required]
        public OverseerrMovieSettings Movies { get; set; } = new OverseerrMovieSettings();
    }

        public class OverseerrMovieSettings
    {
        public string DefaultApiUserId { get; set; }
        public OverseerrMovieCategory[] Categories { get; set; } = Array.Empty<OverseerrMovieCategory>();
    }

    public class OverseerrMovieCategory
    {
        public int Id { get; set; } = -1;
        public bool Is4K { get; set; } = false;
        public string Name { get; set; } = string.Empty;
        public int ServiceId { get; set; } = -1;
        public int ProfileId { get; set; } = -1;
        public string RootFolder { get; set; } = string.Empty;
        public int[] Tags { get; set; } = Array.Empty<int>();
    }
}