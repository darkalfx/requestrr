using System.ComponentModel.DataAnnotations;

namespace Requestrr.WebApi.Controllers.DownloadClients.Lidarr
{
    public class LidarrPath
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Path { get; set; }
    }
}
