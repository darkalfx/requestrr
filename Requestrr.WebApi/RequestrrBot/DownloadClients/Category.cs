namespace Requestrr.WebApi.RequestrrBot.DownloadClients
{
    public class Category
    {
        public string CategoryType { get { return this.GetType().Name; } }
        public int Id { get; set; } = -1;
        public int DownloadClientId { get; set; } = -1;
        public string Name { get; set; } = string.Empty;
    }
}