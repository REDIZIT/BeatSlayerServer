namespace BeatSlayerServer.Models.Configuration.Modules
{
    public class DatabaseSettings
    {
        public string SelectedConnectionName { get; set; }
        public string SelectedConnectionString
        {
            get
            {
                return SelectedConnectionName switch
                {
                    "ConnectionProd" => ConnectionProd,
                    "ConnectionLocal" => ConnectionLocal,
                    "ConnectionRemoteProd" => ConnectionRemoteProd,
                    _ => "",
                };
            }
        }

        public string ConnectionProd { get; set; }
        public string ConnectionLocal { get; set; }
        public string ConnectionRemoteProd { get; set; }
    }
}