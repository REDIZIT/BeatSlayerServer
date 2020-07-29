using BeatSlayerServer.Utils;
using BeatSlayerServer.Utils.Database;
using BeatSlayerServer.Services.MapsManagement;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using BeatSlayerServer.Services.Messaging;
using BeatSlayerServer.Dtos;
using BeatSlayerServer.Dtos.Mapping;
using System.Reflection;
using System.Threading.Tasks;
using BeatSlayerServer.Services.Dashboard;
using BeatSlayerServer.Models.Database;

namespace BeatSlayerServer.Services.Game
{
    /// <summary>
    /// Working with replays and leaderboards
    /// </summary>
    public class RankingService
    {
        private readonly MyDbContext ctx;
        private readonly ILogger<RankingService> logger;

        private readonly MapsService mapsService;
        private readonly ModerationService moderationService;
        private readonly AccountService accountService;
        private readonly DashboardService dashboardService;

        public RankingService(MyDbContext ctx, ILogger<RankingService> logger, MapsService mapsService,
            ModerationService moderationService, AccountService accountService, DashboardService dashboardService)
        {
            this.ctx = ctx;
            this.logger = logger;

            this.mapsService = mapsService;
            this.moderationService = moderationService;
            this.accountService = accountService;
            this.dashboardService = dashboardService;
        }


        /// <summary>
        /// Convert to <see cref="Replay"/>, validate and invoke <see cref="AddReplay(Replay)"/>
        /// </summary>
        [Obsolete("Last usage 1.59.1. Use AddReplay(ReplayData)")]
        public ReplaySendData AddReplay(string json)
        {
            // Received from player
            Replay replay = JsonConvert.DeserializeObject<Replay>(json);

            float RP = GetRP(replay.Accuracy, replay.difficulty, replay.sliced, replay.missed, replay.cubesSpeed, replay.musicSpeed);

            replay.RP = RP;

            return AddReplay(replay);
        }
        /// <summary>
        /// Invoked on map passed (Only approved maps)
        /// </summary>
        public async Task<ReplaySendData> AddReplay(ReplayData replay)
        {
            if (!mapsService.TryGetGroupInfo(replay.Map.Group.Author, replay.Map.Group.Name, out GroupInfo group))
            {
                Console.WriteLine("Failed due to no group found " + replay.Map.Trackname);
                return null;
            }
            if (!mapsService.TryGetMapInfo(group, replay.Map.Nick, out MapInfo map))
            {
                Console.WriteLine("Failed due to no map found " + replay.Map.Nick);
                return null;
            }

            if (!accountService.TryFindAccount(replay.Nick, out Account acc))
            {
                Console.WriteLine("Failed due to no player found " + replay.Nick);
                return null;
            }

            Console.WriteLine("[ENTRY] " + JsonConvert.SerializeObject(replay));



            ReplayInfo info = new ReplayInfo()
            {
                Map = map,
                Player = acc,
                DifficultyName = replay.Difficulty.Name,
                DifficultyStars = replay.Difficulty.Stars,
                Grade = GetGrade(replay.Accuracy, replay.Missed),
                RP = GetRP(replay.Accuracy, replay.Difficulty.Stars, replay.CubesSliced, replay.Missed, replay.Difficulty.CubesSpeed, 1),
                Score = replay.Score,
                CubesSliced = replay.CubesSliced,
                Missed = replay.Missed
            };

            Console.WriteLine("[ENTRY RP] RP " + info.RP);

            int coins = GetCoins(replay.Score, replay.Accuracy, replay.CubesSliced);

            acc.Coins += coins;


            // If map isn't approve return only coins and save coins in db
            if (!moderationService.IsMapApproved(replay.Map.Trackname, replay.Map.Nick))
            {
                Console.WriteLine("Map isn't approved " + replay.Map.Trackname + " by " + replay.Map.Nick);

                dashboardService.OnMapPlayed(replay.Map.Trackname, replay.Map.Nick, replay.Nick, replay.Score, 0, replay.Accuracy);

                await ctx.SaveChangesAsync();

                return new ReplaySendData()
                {
                    Coins = coins,
                    RP = 0,
                    Grade = info.Grade
                };
            }
            // Go through if map is approved




            ReplayInfo bestReplay = accountService.GetBestReplay(acc.Nick, replay.Map.Trackname, replay.Map.Nick);
            bool isNewRecord = bestReplay == null || replay.RP > bestReplay.RP;


            map.Replays.Add(info); /// Add replay to map's replays
            acc.Replays.Add(info); /// Add replay to player's replays

            acc.RP += info.RP;
            acc.AllScore += info.Score;
            acc.Misses += info.Missed;
            acc.Hits += info.CubesSliced;

            RecalculateGrades(acc);


            dashboardService.OnMapPlayed(replay.Map.Trackname, replay.Map.Nick, replay.Nick, replay.Score, (float)info.RP, replay.Accuracy);

            await ctx.SaveChangesAsync();


            return new ReplaySendData()
            {
                Grade = info.Grade,
                Coins = coins,
                RP = info.RP
            };
        }

        /// <summary>
        /// Add replays without validation
        /// </summary>
        [Obsolete]
        public ReplaySendData AddReplay(Replay replay)
        {
            Console.WriteLine("Add replay");

            if (!mapsService.TryGetGroupInfo(replay.author, replay.name, out GroupInfo group))
            {
                Console.WriteLine("Failed due to no group found " + replay.author + "-" + replay.name);
                return null;
            }
            if (!mapsService.TryGetMapInfo(group, replay.nick, out MapInfo map))
            {
                Console.WriteLine("Failed due to no map found " + replay.nick);
                return null;
            }
            if (!accountService.TryFindAccount(replay.player, out Account acc))
            {
                Console.WriteLine("Failed due to no player found " + replay.player);
                return null;
            }



            ReplayInfo info = new ReplayInfo()
            {
                Player = acc,
                RP = (float)replay.RP,
                Score = replay.score,
                Map = map,
                CubesSliced = replay.sliced,
                Missed = replay.missed,
                DifficultyName = replay.diffucltyName
            };

            map.Replays.Add(info); /// Add replay to map's replays
            acc.Replays.Add(info); /// Add replay to player's replays
            acc.RP += info.RP;
            acc.AllScore += info.Score;
            acc.Misses += info.Missed;
            acc.Hits += info.CubesSliced;

            int coins = GetCoins(info.Score, info.Accuracy, info.CubesSliced);
            acc.Coins += coins;

          

            ctx.SaveChanges();

            return new ReplaySendData()
            {
                Coins = coins,
                RP = info.RP
            };
        }



        public float GetRP(float accuracy, float difficulty, float cubesCount, int missed, float cubesSpeed, float musicSpeed)
        {
            float modsMultiplier = (cubesSpeed * 1 + musicSpeed * 1) / 2f;

            missed += 1;
            return (accuracy * difficulty * cubesCount * modsMultiplier) / missed;
        }
        public int GetCoins(float score, float accuracy, int sliced)
        {
            float coins = accuracy * sliced * 5 + (score / 10f * accuracy);
            return (int)Math.Round(coins);
        }
        public Grade GetGrade(float accuracy, int misses)
        {
            if(accuracy == 1)
            {
                return Grade.SS;
            }

            if(accuracy >= 0.96f && misses <= 2)
            {
                return Grade.S;
            }

            if(accuracy >= 0.93f)
            {
                return Grade.A;
            }

            if(accuracy >= 0.7f)
            {
                return Grade.B;
            }

            if(accuracy >= 0.5f)
            {
                return Grade.C;
            }

            return Grade.D;
        }


        public void RecalculateGrades(Account acc)
        {
            Stopwatch w = new Stopwatch();
            w.Start();

            int SS = 0, S = 0, A = 0, B = 0, C = 0, D = 0;

            foreach (var replay in acc.ReplaysUniq)
            {
                switch (replay.Grade)
                {
                    case Grade.SS: SS++; break;
                    case Grade.S: S++; break;
                    case Grade.A: A++; break;
                    case Grade.B: B++; break;
                    case Grade.C: C++; break;
                    case Grade.D: D++; break;
                }
            }

            w.Stop();

            acc.SS = SS;
            acc.S = S;
            acc.A = A;
            acc.B = B;
            acc.C = C;
            acc.D = D;

            Console.WriteLine("Replays count " + acc.Replays.Count);
            Console.WriteLine("RecalculateGrades elapsed time is " + w.ElapsedMilliseconds);
        }









        /// <summary>
        /// Sort Account.PlaceInRanking by Account.RP
        /// </summary>
        public void CalculateLeaderboardPlaces()
        {
            Stopwatch w = new Stopwatch();
            w.Start();

            var allaccounts = ctx.Players;

            int place = 0;
            foreach (var acc in allaccounts.OrderByDescending(c => c.RP))
            {
                place++;
                acc.PlaceInRanking = place;
            }

            ctx.SaveChanges();

            logger.LogInformation($"CalculateLeaderboard (db) took {w.ElapsedMilliseconds}ms");
        }
        /// <summary>
        /// Get sum of RP, update accounts and Invoke <see cref="CalculateLeaderboardPlaces"/>
        /// </summary>
        public void RecalculateLeaderboard()
        {
            Stopwatch w = new Stopwatch();
            w.Start();

            var allaccounts = ctx.Players.ToList();

            foreach (var acc in allaccounts)
            {
                var RP = acc.Replays.Sum(c => c.RP);
                acc.RP = RP;
            }

            Console.WriteLine($"RecalculateLeaderboard took {w.ElapsedMilliseconds}ms");

            CalculateLeaderboardPlaces();
        }





        /// <summary>
        /// Simple check on cheats
        /// </summary>
        /// <returns>False if cheated, True if no cheats</returns>
        private bool ValidateReplay(Replay replay)
        {
            if (replay.Accuracy < 0 || replay.Accuracy > 1) return false;
            //if (replay.musicSpeed < 0.5f || replay.musicSpeed > 1.5f) return false;
            if (replay.score > 20000) return false;
            if (replay.sliced > 1500) return false;

            return true;
        }
    }
}
