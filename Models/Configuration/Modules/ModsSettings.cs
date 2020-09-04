using BeatSlayerServer.Enums.Game;
using System.Collections.Generic;

namespace BeatSlayerServer.Models.Configuration.Modules
{
    public class ModsSettings
    {
        public List<ModSO> Mods { get; set; } = new List<ModSO>();
    }

    public class ModSO
    {
        public ModEnum ModEnum { get; set; }

        public float ScoreMultiplier { get; set; }
        public float RpMultiplier { get; set; }

        public ModSO() { }
    }
}
