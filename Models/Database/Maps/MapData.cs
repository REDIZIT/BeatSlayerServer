using BeatSlayerServer.Dtos.Mapping;
using BeatSlayerServer.Utils.Database;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BeatSlayerServer.Models.Database
{
    public class MapData
    {
        public GroupData Group { get; set; }
        [JsonIgnore] public string Trackname { get { return Group.Author + "-" + Group.Name; } }

        public string Nick { get; set; }

        public List<ReplayData> Replays { get; set; } = new List<ReplayData>();
    }
}
