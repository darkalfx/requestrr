namespace Requestrr.WebApi.config
{
    public abstract class DownloadClientSettings
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ClientType { get { return this.GetType().Name; } }
    }
}