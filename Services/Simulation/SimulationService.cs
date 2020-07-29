using BeatSlayerServer.Services.Game;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace InEditor.Analyze
{
    /// <summary>
    /// Need RankingService for work
    /// </summary>
    public class SimulationService
    {
        public SimulationResult result;

        // Values from game (ScoringManager.cs)

        private float comboValue = 0, comboValueMax = 16;
        private float comboMultiplier = 1;
        private float maxCombo = 1;
        /// <summary>
        /// Value from mods
        /// </summary>
        private float scoreMultiplier = 1;

        /// <summary>
        /// Value for smooth earning score on line slices
        /// </summary>
        private float earnedScore;


        private readonly RankingService ranking;


        public SimulationService(RankingService ranking)
        {
            this.ranking = ranking;
        }



        public List<AnalyzeResult> Analyze(Project proj)
        {

            List<Difficulty> diffs = GetDifficulties(proj);

            List<AnalyzeResult> results = new List<AnalyzeResult>();

            foreach (Difficulty diff in diffs)
            {
                int cubesCount = diff.beatCubeList.Where(c => c.type == BeatCubeClass.Type.Dir || c.type == BeatCubeClass.Type.Point).Count();
                int linesCount = diff.beatCubeList.Where(c => c.type == BeatCubeClass.Type.Line).Count();


                SimulationResult simResult = Simulate(diff);

                results.Add(new AnalyzeResult()
                {
                    DifficultyName = diff.name,
                    DifficultyStars = diff.stars,
                    DifficultyId = diff.id,
                    CubesCount = cubesCount,
                    LinesCount = linesCount,
                    MaxScore = simResult.MaxScore,
                    MaxRP = simResult.MaxRP,
                    ScorePerBlock = simResult.ScorePerBlock,
                    RPPerBlock = simResult.RPPerBlock
                });
            }

            return results;
        }

        

        public SimulationResult Simulate(Difficulty diff)
        {
            //Stopwatch w = new Stopwatch();
            //w.Start();

            //Reset();


            result = new SimulationResult();

            for (int i = 0; i < diff.beatCubeList.Count; i++)
            {
                // Spawned beat note (cube or line)
                BeatCubeClass cls = diff.beatCubeList[i];

                if(cls.type == BeatCubeClass.Type.Line)
                {
                    OnLineHold(cls);
                    OnLineHit();
                }
                else
                {
                    OnCubeHit();
                }

                ClampCombo();
            }

            
            float blocksCount = diff.beatCubeList.Count;

            result.MaxRP = ranking.GetRP(1, diff.stars, blocksCount, 0, diff.speed, 1);
            result.ScorePerBlock = result.MaxScore / blocksCount;
            result.RPPerBlock = result.MaxRP / blocksCount;

            //Debug.Log($"Simulation time is {w.ElapsedMilliseconds}ms");
            return result;
        }

        private List<Difficulty> GetDifficulties(Project proj)
        {
            List<Difficulty> diffs = new List<Difficulty>();


            if (proj.difficulties.Count > 0)
            {
                diffs.AddRange(proj.difficulties);
            }
            else
            {
                diffs.Add(new Difficulty()
                {
                    beatCubeList = proj.beatCubeList,
                    name = proj.difficultName,
                    stars = proj.difficultStars == 0 ? 4 : proj.difficultStars,
                    id = -1
                });
            }

            return diffs;
        }





        private void Reset()
        {
            comboValue = 0;
            comboValueMax = 16;
            comboMultiplier = 1;
            maxCombo = 1;
            scoreMultiplier = 1;
            earnedScore = 0;
        }


        private void ClampCombo()
        {
            // Code from game (ScoringManager.cs)
            // == // == == Combo == == // == //

            if (comboValue >= comboValueMax && comboMultiplier < 16)
            {
                comboValue = 2;
                comboMultiplier *= 2;
                comboValueMax = 8 * comboMultiplier;
            }
            else if (comboValue <= 0)
            {
                if (comboMultiplier != 1)
                {
                    comboMultiplier /= 2;
                    comboValue = comboValueMax - 5;
                }
                else
                {
                    comboValue = 0;
                }
            }
            if (comboValue > 0)
            {
                //comboValue -= Time.deltaTime * comboMultiplier * 0.4f;
            }

            if (comboMultiplier > maxCombo) maxCombo = comboMultiplier;



            // == // == == Lines == == // == //
            // Seems not working xD

            if (earnedScore >= 1)
            {
                float rounded = (float)Math.Floor(earnedScore) * scoreMultiplier;
                earnedScore -= rounded;
                result.MaxScore += rounded;
            }
        }



        private void OnCubeHit()
        {
            result.MaxScore += comboMultiplier * scoreMultiplier;
            comboValue += 1;
        }

        /// <summary>
        /// Invoked every frame while line is exists
        /// </summary>
        private void OnLineHold(BeatCubeClass cls)
        {
            float frameCount = cls.lineLenght * 60;
            result.MaxScore += comboMultiplier * scoreMultiplier * 0.04f * (frameCount);
        }

        private void OnLineHit()
        {
            comboValue += 1;
        }
    }
}