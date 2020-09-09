using BeatSlayerServer.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BeatSlayerServer.Models.Maps
{
    /// <summary>
    /// Data model of maps for trackname.<br/>
    /// <b>Server returns <see cref="MapsData"/> on game request all maps list</b>
    /// </summary>
    public class MapsData
    {
        public string Name { get; set; }
        public string Author { get; set; }
        [JsonIgnore] public string Trackname => Name + "-" + Author;

        public GroupType MapType { get; set; } = GroupType.Author;



        public int Downloads { get; set; }
        public int PlayCount { get; set; }
        public int Likes { get; set; }
        public int Dislikes { get; set; }


        public DateTime UpdateTime { get; set; }

        [JsonIgnore] public bool IsNew => (DateTime.Now - UpdateTime).TotalDays <= 5;

        public List<string> MappersNicks { get; set; } = new List<string>();
    }
}
