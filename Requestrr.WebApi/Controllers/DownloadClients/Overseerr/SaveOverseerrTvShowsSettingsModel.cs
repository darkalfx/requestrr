using System;
using System.ComponentModel.DataAnnotations;

namespace Requestrr.WebApi.Controllers.DownloadClients.Overseerr
{
    public class SaveOverseerrTvShowsSettingsModel : OverseerrSettingsModel
    {
        [Required]
        public string Restrictions { get; set; }

        [Required]
        public OverseerrTvShowSettings TvShows { get; set; } = new OverseerrTvShowSettings();
    }

    public class OverseerrTvShowSettings
    {
        public string DefaultApiUserId { get; set; }
        public OverseerrTvShowCategory[] Categories { get; set; } = Array.Empty<OverseerrTvShowCategory>();
    }

    public class OverseerrTvShowCategory
    {
        public int Id { get; set; } = -1;
        public bool Is4K { get; set; } = false;
        public string Name { get; set; } = string.Empty;
        public int ServiceId { get; set; } = -1;
        public int ProfileId { get; set; } = -1;
        public int LanguageProfileId { get; set; } = -1;
        public string RootFolder { get; set; } = string.Empty;
        public int[] Tags { get; set; } = Array.Empty<int>();
    }
}