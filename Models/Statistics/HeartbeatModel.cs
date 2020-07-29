using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeatSlayerServer.Utils.Statistics
{
    public class HeartbeatFrame
    {
        public DateTime Time { get; set; }
        public int OldOnline { get; set; }
        public int SignalrOnline { get; set; }
        public int GameLaunchCount { get; set; }
        public int GameLaunchAnonimCount { get; set; }

        /// <summary>
        /// Count of not approved maps played
        /// </summary>
        public int GamesCount { get; set; }

        /// <summary>
        /// Count of approved maps played (Replays got count)
        /// </summary>
        public int GamesApprovedCount { get; set; }
    }
}
