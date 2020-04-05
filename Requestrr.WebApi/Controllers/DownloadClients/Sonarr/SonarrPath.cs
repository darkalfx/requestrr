using System.ComponentModel.DataAnnotations;

namespace Requestrr.WebApi.Controllers.DownloadClients.Sonarr
{
    public class SonarrPath
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Path { get; set; }
    }
}
