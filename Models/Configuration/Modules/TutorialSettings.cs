using System.Collections.Generic;

namespace BeatSlayerServer.Models.Configuration.Modules
{
    public class TutorialSettings
    {
        public KeyValuePair<string, string> TutorialMap { get; set; } = new KeyValuePair<string, string>("Beat Slayer-Tutorial", "REDIZIT");
        public Dictionary<string, string> EasyMaps { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> HardMaps { get; set; } = new Dictionary<string, string>();
    }
}
