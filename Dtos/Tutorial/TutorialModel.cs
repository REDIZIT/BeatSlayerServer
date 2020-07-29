using BeatSlayerServer.Dtos.Mapping;
using System.Collections.Generic;

namespace BeatSlayerServer.Dtos.Tutorial
{
    public class TutorialResult
    {
        public int AllSliced { get; set; }
        public int AllMissed { get; set; }
        /// <summary>
        /// Accuracy in range 0-1
        /// </summary>
        public float Accuracy => AllSliced / (float)(AllSliced + AllMissed);

        public List<TutorialStep> Steps { get; set; } = new List<TutorialStep>();


        public void AddStep(string name, ReplayData replay)
        {
            Steps.Add(new TutorialStep(name, replay.CubesSliced - AllSliced, replay.Missed - AllMissed));

            AllSliced = replay.CubesSliced;
            AllMissed = replay.Missed;
        }
    }
    public class TutorialStep
    {
        public string Name { get; set; }
        public int Sliced { get; set; }
        public int Missed { get; set; }
        /// <summary>
        /// Accuracy in range 0-1
        /// </summary>
        public float Accuracy => Sliced / (float)(Sliced + Missed);

        public TutorialStep(string name, int sliced, int missed)
        {
            Name = name;
            Sliced = sliced;
            Missed = missed;
        }
    }
}
