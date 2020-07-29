using System.Collections.Generic;

namespace BeatSlayerServer.Models.Database
{
    public class DifficultyInfo
    {
        /// <summary>
        /// Id difficulty has in database
        /// </summary>
        public int Id { get; set; }

        public virtual MapInfo Map { get; set; }

        public virtual List<ReplayInfo> Replays { get; set; }

        /// <summary>
        /// Id difficulty has in project
        /// </summary>
        public int InnerId { get; set; }

        /// <summary>
        /// Difficulty stars count ( 0-10 )
        /// </summary>
        public int Stars { get; set; }
        /// <summary>
        /// Name of difficulty
        /// </summary>
        public string Name { get; set; }


        /// <summary>
        /// <see cref="MapInfo"/> downloads count
        /// </summary>
        public int Downloads { get; set; }
        /// <summary>
        /// Difficulty play count
        /// </summary>
        public int PlayCount { get; set; }
        /// <summary>
        /// Difficulty likes
        /// </summary>
        public int Likes { get; set; }
        /// <summary>
        /// Difficulty dislikes
        /// </summary>
        public int Dislikes { get; set; }



        /// <summary>
        /// Count of cubes, lines
        /// </summary>
        public int ObjectsCount => CubesCount + LinesCount + BombsCount;
        public int CubesCount { get; set; }
        public int LinesCount { get; set; }
        public int BombsCount { get; set; }



        /// <summary>
        /// Max score that player can get
        /// </summary>
        public float MaxScore { get; set; }

        /// <summary>
        /// Max RP that player can get
        /// </summary>
        public float MaxRP { get; set; }
    }
}
