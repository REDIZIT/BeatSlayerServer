using BeatSlayerServer.Enums;
using Newtonsoft.Json;

namespace BeatSlayerServer.Models.Maps
{
    public class BasicMapData
    {
        public string Author { get; set; }
        public string Name { get; set; }
        public string Nick { get; set; }
        public bool IsApproved { get; set; }
        public GroupType MapType { get; set; } = GroupType.Author;

        [JsonIgnore] public string Trackname { get { return Author + "-" + Name; } }


        public BasicMapData() { }
    }
}
