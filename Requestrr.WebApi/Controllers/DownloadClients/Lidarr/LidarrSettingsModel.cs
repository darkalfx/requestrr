using System;
using System.ComponentModel.DataAnnotations;

namespace Requestrr.WebApi.Controllers.DownloadClients.Lidarr
{
    public class LidarrSettingsModel : TestLidarrSettingsModel
    {
        [Required]
        public string MusicPath { get; set; }
        [Required]
        public int MusicProfile { get; set; }
        [Required]
        public int MusicMetadataProfile { get; set; }
        public int[] MusicTags { get; set; }
        public bool MusicUseAlbumFolders { get; set; }
        public bool SearchNewRequests { get; set; }
        public bool MonitorNewRequests { get; set; }
        [Required]
        public string Command { get; set; }
    }
}
