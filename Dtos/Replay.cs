using BeatSlayerServer.Controllers;
using Newtonsoft.Json;
using System;

namespace BeatSlayerServer.Dtos
{
    public class Replay
    {
        public string player;
        // MapInfo
        public string author, name, nick;
        public int difficulty;
        public string diffucltyName;

        public double RP;

        public float score;
        public int sliced, missed;
        [JsonIgnore] public float AllCubes { get { return sliced + missed; } }
        /// <summary>
        /// Accuracy in 1.0 (sliced / allCubes)
        /// </summary>
        [JsonIgnore] public float Accuracy { get { return AllCubes == 0 ? 0 : (float)sliced / (float)AllCubes; } }

        public float cubesSpeed = 1, musicSpeed = 1;

        public Replay(string author, string name, string nick, int difficulty, float score, int sliced, int missed, float cubesSpeed, float musicSpeed)
        {
            this.author = author;
            this.name = name;
            this.nick = nick;
            this.difficulty = difficulty;
            this.score = score;
            this.sliced = sliced;
            this.missed = missed;
            this.cubesSpeed = Math.Clamp(cubesSpeed, 0.5f, 1.5f);
            this.musicSpeed = Math.Clamp(musicSpeed, 0.5f, 1.5f);
        }
        public Replay(AccountTrackRecord record)
        {
            author = record.author;
            name = record.name;
            nick = record.nick;
            difficulty = 4; // USED DEFAULT VALUE
            score = record.score;
            sliced = record.sliced;
            missed = record.missed;
        }

        public Replay() { }
    }
}
