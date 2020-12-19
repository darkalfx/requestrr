using System.ComponentModel.DataAnnotations;

namespace Requestrr.WebApi.Controllers.DownloadClients.Lidarr
{
    public class LidarrProfile
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
    }
}
