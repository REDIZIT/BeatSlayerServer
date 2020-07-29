using BeatSlayerServer.Dtos;
using BeatSlayerServer.Dtos.Mapping;
using BeatSlayerServer.Utils.Database;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace BeatSlayerServer.Models.Database
{
    public class ReplayInfo
    {
        public int Id { get; set; }

        [Required] public virtual Account Player { get; set; }
        [Required] public virtual MapInfo Map { get; set; }
        [Required] public string DifficultyName { get; set; }
        [Required] public int DifficultyStars { get; set; }



        public Grade Grade { get; set; }

        public float Score { get; set; }
        public double RP { get; set; }

        public int Missed { get; set; }
        public int CubesSliced { get; set; }
        public float Accuracy => CubesSliced + Missed == 0 ? 0 : CubesSliced / (float)(CubesSliced + Missed);




        public override string ToString()
        {
            return JsonConvert.SerializeObject(CutInfo(), new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }
        public ReplayData CutInfo()
        {
            return new ReplayData()
            {
                Map = new MapData()
                {
                    Group = new GroupData()
                    {
                        Author = Map.Group.Author,
                        Name = Map.Group.Name
                    },
                    Nick = Map.Nick
                },
                Nick = Player.Nick,
                Score = Score,
                RP = RP,
                Missed = Missed,
                CubesSliced = CubesSliced,
                DifficultyName = DifficultyName
            };
        }
    }
}
