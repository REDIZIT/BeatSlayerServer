using BeatSlayerServer.Models.Database.Maps;
using BeatSlayerServer.Utils.Database;
using System.Collections.Generic;
using System.Linq;

namespace BeatSlayerServer.Models.Database
{
    public class MapInfo
    {
        public int Id { get; set; }
        public virtual GroupInfo Group { get; set; }
        public string Nick { get; set; }
       
        
        public virtual List<ReplayInfo> Replays { get; set; }
        public virtual List<DifficultyInfo> Difficulties { get; set; }


        public MapPublishStatus PublishStatus { get; set; }

        public int Likes => Difficulties.Sum(c => c.Likes);
        public int Dislikes => Difficulties.Sum(c => c.Dislikes);
        public int PlayCount => Difficulties.Sum(c => c.PlayCount);

    }
}
