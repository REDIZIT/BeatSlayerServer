using System.Collections.Generic;

namespace BeatSlayerServer.Models.Configuration.Modules
{
    public class BanSettings
    {
        public List<string> ApproveBans { get; set; } = new List<string>();
    }
}
