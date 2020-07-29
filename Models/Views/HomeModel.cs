using BeatSlayerServer.Models.Configuration.Modules;

namespace BeatSlayerServer.Utils
{
    public class HomeModel
    {
        public ServerType ServerType { get; set; }
        public ChartInfo chartInfo;

        public HomeModel() { }
    }

    public class ChartInfo
    {
        public string subtitle;
        public string[] times;
        public int[] oldOnlineData, signalrOnlineData, gameLaunches, gameLaunchesAnonim;
        public int[] gamesApprovedCount, gamesCount;
    }
}
