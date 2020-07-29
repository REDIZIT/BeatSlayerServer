namespace BeatSlayerServer.Models.Configuration.Modules
{
    public class HeartbeatSettings
    {
        public string DataFilePath { get; set; }
        public int DataCollectingTime { get; set; } = 10;
    }
}
