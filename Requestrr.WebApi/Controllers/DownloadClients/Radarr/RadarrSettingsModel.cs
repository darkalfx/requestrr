using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Requestrr.WebApi.Controllers.DownloadClients.Radarr
{
    public class RadarrSettingsModel : TestRadarrSettingsModel
    {
        [Required]
        public RadarrSettingsCategory[] Categories { get; set; } = Array.Empty<RadarrSettingsCategory>();
        public bool SearchNewRequests { get; set; }
        public bool MonitorNewRequests { get; set; }
    }

    public class RadarrSettingsCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ProfileId { get; set; }
        public string RootFolder { get; set; }
        public string MinimumAvailability { get; set; }
        public int[] Tags { get; set; } = Array.Empty<int>();
    }
}
