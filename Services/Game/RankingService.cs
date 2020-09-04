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
using BeatSlayerServer.Enums.Game;
using System.Collections.Generic;
using BeatSlayerServer.Models.Configuration.Modules;
using BeatSlayerServer.Models.Configuration;

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

        private readonly ServerSettings settings;

        public RankingService(MyDbContext ctx, ILogger<RankingService> logger, SettingsWrapper wrapper, MapsService mapsService,
            ModerationService moderationService, AccountService accountService, DashboardService dashboardService)
        {
            this.ctx = ctx;
            this.logger = logger;
            settings = wrapper.settings;

            this.mapsService = mapsService;
            this.moderationService = moderationService;
            this.accountService = accountService;
            this.dashboardService = dashboardService;
        }


        /// <summary>
        /// Invoked on map passed (Only approved maps)
        /// </summary>
        public async Task<ReplaySendData> AddReplay(ReplayData replay)
        {
            if (!mapsService.TryGetGroupInfo(replay.Map.Group.Author, replay.Map.Group.Name, out GroupInfo group))
            {
                logger.LogError("Failed to find group " + (replay.Map.Group.Author + "-" + replay.Map.Group.Name));
                return null;
            }
            if (!mapsService.TryGetMapInfo(group, replay.Map.Nick, out MapInfo map))
            {
                logger.LogError("Failed to find map " + (replay.Map.Group.Author + "-" + replay.Map.Group.Name + " by " + replay.Map.Nick));
                return null;
            }
            if (!accountService.TryFindAccount(replay.Nick, out Account acc))
            {
                logger.LogError("Failed to find account " + replay.Nick);
                return null;
            }


            List<ModSO> selectedMods = settings.Mods.Mods.Where(c => replay.Mods.HasFlag(c.ModEnum)).ToList();
            logger.LogInformation("[DEBUG] Selected modsSo are {selectedModsJson}", JsonConvert.SerializeObject(selectedMods, Formatting.Indented));

            ReplayInfo info = new ReplayInfo()
            {
                Map = map,
                Player = acc,
                DifficultyName = replay.Difficulty.Name,
                DifficultyStars = replay.Difficulty.Stars,
                Grade = GetGrade(replay.Accuracy, replay.Missed),
                RP = GetRP(replay.Accuracy, replay.Difficulty.Stars, replay.CubesSliced, replay.Missed, replay.Difficulty.CubesSpeed, selectedMods),
                Score = replay.Score,
                CubesSliced = replay.CubesSliced,
                Missed = replay.Missed
            };

            int coins = GetCoins(replay.Score, replay.Accuracy, replay.CubesSliced);

            acc.Coins += coins;


            // If map isn't approve return only coins and save coins in db
            if (!CanGetRP(replay))
            {
                dashboardService.OnMapPlayed(replay.Map.Trackname, replay.Map.Nick, replay.Nick, replay.Score, 0, replay.Accuracy);

                logger.LogInformation("[PLAYED] NON-RP {@replay}", replay);

                await ctx.SaveChangesAsync();

                return new ReplaySendData()
                {
                    Coins = coins,
                    RP = 0,
                    Grade = info.Grade
                };
            }
            // Go through if map is approved




            //ReplayInfo bestReplay = accountService.GetBestReplay(acc.Nick, replay.Map.Trackname, replay.Map.Nick);
            //bool isNewRecord = bestReplay == null || replay.RP > bestReplay.RP;


            map.Replays.Add(info); /// Add replay to map's replays
            acc.Replays.Add(info); /// Add replay to player's replays

            acc.RP += info.RP;
            acc.AllScore += info.Score;
            acc.Misses += info.Missed;
            acc.Hits += info.CubesSliced;

            RecalculateGrades(acc);


            dashboardService.OnMapPlayed(replay.Map.Trackname, replay.Map.Nick, replay.Nick, replay.Score, (float)info.RP, replay.Accuracy);

            logger.LogInformation("[PLAYED] RP {@replay}", replay);

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



        public float GetRP(float accuracy, float difficulty, float cubesCount, int missed, float cubesSpeed, List<ModSO> mods)
        {
            float cubesSpeedMultiplier = (cubesSpeed + 1) / 2f;
            float modsMultiplier = 1;
            if(mods != null && mods.Count > 0)
            {
                modsMultiplier = mods.Select(c => c.RpMultiplier).Aggregate((mult, e) => mult * e);
            }
            logger.LogInformation("RP multiplier = {multiplier}", modsMultiplier);

            missed += 1;
            return (accuracy * difficulty * cubesCount * modsMultiplier * cubesSpeedMultiplier) / missed;
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




        private bool CanGetRP(ReplayData replay)
        {
            bool isApproved = moderationService.IsMapApproved(replay.Map.Trackname, replay.Map.Nick);
            bool hasNoFailMod = replay.Mods.HasFlag(ModEnum.NoFail);

            if (!isApproved) return false;

            if (hasNoFailMod) return false;

            return true;
        }
    }
}
