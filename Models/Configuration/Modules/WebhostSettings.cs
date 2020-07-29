namespace BeatSlayerServer.Models.Configuration.Modules
{
    public class WebhostSettings
    {
        public ServerType ServerType { get; set; }
        public string ServerUrl 
        {
            get
            {
                return ServerType == ServerType.Production ? ProdUrl : DevUrl;
            }
        }
        public string ProdUrl { get; set; }
        public string DevUrl { get; set; }
    }
    public enum ServerType
    {
        Production, Development
    }
}
