using BeatSlayerServer.Models.Database;
using System.Collections.Generic;

namespace BeatSlayerServer.Utils.Database
{
    public class GroupInfo
    {
        public int Id { get; set; }
        public string Author { get; set; }
        public string Name { get; set; }

        public virtual List<MapInfo> Maps { get; set; } = new List<MapInfo>();
    }

    public class GroupData
    {
        public string Author { get; set; }
        public string Name { get; set; }

        public List<MapData> Maps { get; set; }
    }
}
