using BeatSlayerServer.Enums.Game;
using BeatSlayerServer.Models.Database;

namespace BeatSlayerServer.Dtos.Mapping
{
    /// <summary>
    /// For transfering between server and game
    /// </summary>
    public class ReplayData
    {
        public MapData Map { get; set; }
        public string DifficultyName { get; set; }
        public DifficultyData Difficulty { get; set; }
        public ModEnum Mods { get; set; }

        /// <summary>
        /// Player nick, who passed map
        /// </summary>
        public string Nick { get; set; }


        public Grade Grade { get; set; }

        public float Score { get; set; }
        public double RP { get; set; }

        public int Missed { get; set; }
        public int CubesSliced { get; set; }

        /// <summary>
        /// Range from 0 to 1
        /// </summary>
        public float Accuracy => CubesSliced + Missed == 0 ? 0 : (float)CubesSliced / (float)(CubesSliced + Missed);
    }
}
