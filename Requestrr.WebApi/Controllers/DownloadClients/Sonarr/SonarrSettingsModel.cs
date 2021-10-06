using System;

namespace Requestrr.WebApi.Controllers.DownloadClients.Sonarr
{
    public class SonarrSettingsModel : TestSonarrSettingsModel
    {
        public SonarrSettingsCategory[] Categories { get; set; } = Array.Empty<SonarrSettingsCategory>();
        public bool SearchNewRequests { get; set; }
        public bool MonitorNewRequests { get; set; }
    }

    public class SonarrSettingsCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ProfileId { get; set; }
        public string RootFolder { get; set; }
        public int[] Tags { get; set; } = Array.Empty<int>();
        public int LanguageId { get; set; }
        public bool UseSeasonFolders { get; set; } = true;
        public string SeriesType { get; set; }
    }
}
