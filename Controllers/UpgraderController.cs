using BeatSlayerServer.Utils;
using BeatSlayerServer.ProjectManagement;
using BeatSlayerServer.Services.Game;
using BeatSlayerServer.Services.MapsManagement;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text;
using AccountDb = BeatSlayerServer.Utils.Database.Account;
using BeatSlayerServer.Models.Configuration;
using Microsoft.Extensions.Logging;
using BeatSlayerServer.Services;
using BeatSlayerServer.Services.Messaging.Discord;
using Microsoft.Extensions.Hosting;
using InEditor.Analyze;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using BeatSlayerServer.Models.Database.Maps;
using Microsoft.AspNetCore.Authorization;
using BeatSlayerServer.Controllers;
using BeatSlayerServer.Utils.Shop;
using BeatSlayerServer.Models.Maps;

namespace BeatSlayerServer.Core
{
    public class UpgraderController : Controller
    {
        private readonly MyDbContext ctx;
        private readonly ModerationService moderationService;
        private readonly RankingService rankingService;
        private readonly MapsService mapsService;
        private readonly AccountService accountService;
        private readonly ShopService shopService;
        private readonly ILogger<UpgraderController> logger;

        private readonly ServerSettings settings;
        private readonly IHostEnvironment env;


        private readonly DiscordBotWrapper discordBotService;
        private readonly SimulationService simulationService;

        public UpgraderController(SettingsWrapper wrapper, ILogger<UpgraderController> logger, IHostEnvironment env, MyDbContext ctx, ModerationService moderationService, 
            RankingService rankingService, MapsService mapsService, DiscordBotWrapper discordBotService, SimulationService simulationService, AccountService accountService, 
            ShopService shopService)
        {
            this.logger = logger;
            this.ctx = ctx;
            this.moderationService = moderationService;
            this.rankingService = rankingService;
            this.mapsService = mapsService;
            this.accountService = accountService;

            settings = wrapper.settings;

            this.discordBotService = discordBotService;
            this.simulationService = simulationService;

            this.shopService = shopService;
        }


        public IActionResult GiveCoins(string nick, int count, string masterkey)
        {
            if (masterkey != "sosipisun") return Content("Wrong masterkey");

            AccountDb acc = ctx.Players.FirstOrDefault(c => c.Nick == nick);
            acc.Coins += count;
            ctx.SaveChanges();

            return Content("*click* noice");
        }


        public string GetGroupsByDifficulty(int stars)
        {
            string[] tracksFolders = Directory.GetDirectories(settings.TracksFolder).OrderByDescending(c => new DirectoryInfo(c).CreationTime).ToArray();
            List<MapsData> groupInfos = new List<MapsData>();

            for (int i = 0; i < tracksFolders.Length; i++)
            {
                string trackname = new DirectoryInfo(tracksFolders[i]).Name;

                string[] mapsFolders = Directory.GetDirectories(tracksFolders[i]);

                MapsData groupInfo = new MapsData()
                {
                    Author = trackname.Split('-')[0],
                    Name = trackname.Split('-')[1]
                };


                foreach (string mapFolder in mapsFolders)
                {
                    try
                    {
                        MapInfo info = ProjectManager.GetMapInfo(mapFolder, true);

                        if(info.difficulties.Any(c => c.stars == stars))
                        {
                            groupInfos.Add(groupInfo);
                        }
                    }
                    catch (Exception err)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("GetGroupExtended: " + mapFolder);
                        Console.WriteLine("    " + err);
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }
            }

            return JsonConvert.SerializeObject(groupInfos, Formatting.Indented);
        }






        /// <summary>
        /// Clear all purchases with upgrading by client side
        /// </summary>
        public string DropPurchases(string nick, string masterpass)
        {
            logger.LogInformation("DropPurchases for {nick} with masterpass = {masterpass}", nick, masterpass == "sosipisun");

            if (masterpass != "sosipisun") return "Not master";
            if (!accountService.TryFindAccount(nick, out AccountDb acc)) return "No such player";

            acc.Purchases.Clear();
            ctx.SaveChanges();

            return "Done";
        }
        /// <summary>
        /// Set all purchases to defaults without upgrading by client side
        /// </summary>
        public string ResetPurchases(string nick, string masterpass)
        {
            logger.LogInformation("ResetPurchases for {nick} with masterpass = {masterpass}", nick, masterpass == "sosipisun");

            if (masterpass != "sosipisun") return "Not master";
            if (!accountService.TryFindAccount(nick, out AccountDb acc)) return "No such player";

            acc.Purchases.RemoveAll(c => c.Cost != 0);
            ctx.SaveChanges();

            return "Done";
        }

        public string ViewPurchases(string nick, string masterpass)
        {
            if (masterpass != "sosipisun") return "Not master";
            if (!accountService.TryFindAccount(nick, out AccountDb acc)) return "No such player";


            StringBuilder b = new StringBuilder();

            if (acc.Purchases == null) b.AppendLine("<null>");
            if (acc.Purchases.Count == 0) b.AppendLine("<empty>");

            foreach (var purchase in acc.Purchases)
            {
                b.AppendLine($"{purchase.Name} ({purchase.ItemId}) - {purchase.Cost}");
            }

            return b.ToString();
        }






        public string UpgradeDifficultiesForAll()
        {
            StringBuilder b = new StringBuilder();
            Stopwatch w = Stopwatch.StartNew();

            var tracknames = ctx.Groups.Select(c => new { trackname = c.Author.ToString() + "-" + c.Name.ToString(), mappers = c.Maps.Select(c => c.Nick) }).ToList();
            

            foreach (var map in tracknames)
            {
                foreach (string mapper in map.mappers)
                {
                    b.AppendLine(UpgradeDifficulties(map.trackname, mapper));
                }
            }

            w.Stop();

            Console.WriteLine("\n\nDone in " + w.Elapsed.TotalSeconds + " seconds");

            b.AppendLine("\n\nDone in " + w.Elapsed.TotalSeconds + " seconds");

            return b.ToString();
        }
        public string UpgradeDifficulties(string trackname, string mapper)
        {
            string author = trackname.Split('-')[0];
            string name = trackname.Split('-')[1];

            var group = ctx.Groups.FirstOrDefault(c => c.Author == author && c.Name == name);
            if (group == null) return "No such group";

            var map = group.Maps.FirstOrDefault(c => c.Nick == mapper);
            if (map == null) return "No such map";


            Stopwatch w = Stopwatch.StartNew();


            string projectPath = settings.TracksFolder + "/" + trackname + "/" + mapper + "/" + trackname + ".bsz";
            Console.WriteLine("Upgrade " + trackname);
            Project proj = ProjectManager.LoadProject(projectPath);

            var mapInfo = ProjectManager.GetMapInfo(trackname, mapper);

            List<AnalyzeResult> analyzeResults = simulationService.Analyze(proj);

            StringBuilder b = new StringBuilder();
            

            foreach (var diff in proj.difficulties)
            {
                if (map.Difficulties.Any(c => c.Name == diff.name))
                {
                    Console.WriteLine("|-- [ERROR SKIP] " + diff.name + $" ({diff.id})");
                    continue;
                }
                Console.WriteLine("|-- " + diff.name);

                AnalyzeResult analyzeResult = analyzeResults.FirstOrDefault(c => c.DifficultyName == diff.name);

                var diffInfo = new Models.Database.DifficultyInfo()
                {
                    InnerId = diff.id,
                    Name = diff.name,
                    Stars = diff.stars,
                    //Map = map,
                    CubesCount = diff.beatCubeList.Count(c => c.type == BeatCubeClass.Type.Point || c.type == BeatCubeClass.Type.Dir),
                    LinesCount = diff.beatCubeList.Count(c => c.type == BeatCubeClass.Type.Line),
                    BombsCount = diff.beatCubeList.Count(c => c.type == BeatCubeClass.Type.Bomb),
                    Likes = mapInfo.Likes + mapInfo.likes,
                    Dislikes = mapInfo.Dislikes + mapInfo.dislikes,
                    PlayCount = mapInfo.PlayCount + mapInfo.playCount,
                    Downloads = mapInfo.downloads,
                    MaxRP = analyzeResult.MaxRP,
                    MaxScore = analyzeResult.MaxScore
                };

                b.AppendLine(JsonConvert.SerializeObject(diffInfo, Formatting.Indented));
            }

            Console.WriteLine("...done in " + w.ElapsedMilliseconds + "ms");

            return b.ToString();
        }


        public void CheckStatuses()
        {
            string filepath = @"C:\Users\REDIZIT\Desktop\GameServer\check-result.txt";

            foreach (var group in ctx.Groups.Select(c => new { id = c.Id, author = c.Author, name = c.Name, maps = c.Maps.Select(m => new { status = m.PublishStatus, nick = m.Nick, id = m.Id }) }))
            {
                foreach (var map in group.maps)
                {
                    string result = map.status.ToString() + "  " +  map.id + $"({group.id})" + " " + group.author + "-" + group.name + " by " + map.nick;
                    System.IO.File.AppendAllText(filepath, "\n" + result);
                    Console.WriteLine(result);
                }
            }
        }




        public string GetGroupById(int id = 769)
        {
            StringBuilder b = new StringBuilder();

            var group = ctx.Groups.FirstOrDefault(c => c.Id == id);
            if (group == null) return "No such group";

            b.AppendLine(group.Author + "-" + group.Name);
            b.AppendLine("Mapers:\n  " + string.Join("\n  ", group.Maps.Select(c => new { id = c.Id, nick = c.Nick, replays = c.Replays.Count, status = c.PublishStatus })));


            //string originalTrackname = "Stray Kids God's Menu (神메뉴) (Color Coded Lyrics EngRomHan가사)-God's menu";
            //string normalTrackname = "Stray Kids God's Menu (Color Coded Lyrics EngRomHan)-God's menu";

            //var isntGood = Regex.IsMatch(normalTrackname, @"[^a-zA-Z0-9_?а-яёА-Я- '()\[\]\{\}+*~!@#$%^&\/\\`>=<.,;]");


            //b.AppendLine("Is not good? " + isntGood);

            return b.ToString();
        }

        public void SetStatus(int groupId, string mapper, int statusId)
        {
            var group = ctx.Groups.FirstOrDefault(c => c.Id == groupId);
            var map = group.Maps.FirstOrDefault(c => c.Nick == mapper);

            map.PublishStatus = (MapPublishStatus)statusId;
            ctx.SaveChanges();
        }


        public string GetAllFolders()
        {
            return string.Join("\n", Directory.GetDirectories(settings.TracksFolder));
        }
        public string GetAllGroups()
        {
            return string.Join("\n", ctx.Groups.Select(c => c.Author.ToString() + "-" + c.Name.ToString()));
        }

        public string RenameMapInDb(string trackname, string newtrackname)
        {
            StringBuilder b = new StringBuilder();

            string author = trackname.Split('-')[0];
            string name = trackname.Split('-')[1];

            var group = ctx.Groups.FirstOrDefault(c => c.Author == author && c.Name == name);
            if (group == null) return "No such group";



            group.Author = newtrackname.Split('-')[0];
            group.Name = newtrackname.Split('-')[1];

            ctx.SaveChanges();

            return "Done";
        }

        //[Authorize(Roles = "Developer")]
        public string SetStars(string trackname, string mapper, int stars)
        {
            var mapInfo = ProjectManager.GetMapInfo(trackname, mapper);
            if (mapInfo == null) return "No such MapInfo";
            if (mapInfo.difficulties.Count > 1) return "Too many difficulties";

            mapInfo.difficultyStars = stars;
            mapInfo.difficulties[0].stars = stars;

            string author = trackname.Split('-')[0];
            string name = trackname.Split('-')[1];

            var group = ctx.Groups.FirstOrDefault(c => c.Author == author && c.Name == name);
            if (group == null) return "No such group";

            var map = group.Maps.FirstOrDefault(c => c.Nick == mapper);
            if (map == null) return "No such map in db";

            var diff = map.Difficulties.First();
            if (diff == null) return "No such difficulty";

            diff.Stars = stars;

            ProjectManager.SetMapInfo(mapInfo);
            ctx.SaveChanges();

            return "Done!";
        }



        public string GiveItem(string nick, int itemId, string masterpass)
        {
            if (!accountService.TryFindAccount(nick, out AccountDb acc)) return "No such account";

            return shopService.GiveItem(acc, itemId, masterpass) ? "Success" : "Failed";
        }




        /*public string UpgradeProject()
        {
            StringBuilder b = new StringBuilder();
            b.AppendLine("Tracks with old difficulties\n");


            string[] groupFolders = Directory.GetDirectories(settings.TracksFolder);
            foreach (var groupFolder in groupFolders)
            {
                string trackname = new DirectoryInfo(groupFolder).Name;
                string[] mapperFolders = Directory.GetDirectories(groupFolder);


                foreach (var mapperFolder in mapperFolders)
                {
                    string mapper = new FileInfo(mapperFolder).Name;

                    try
                    {
                        string path = settings.TracksFolder + "/" + trackname + "/" + mapper + "/" + trackname + ".bsz";
                        Project proj = ProjectManager.LoadProject(path);

                        if(proj.difficulties.Count == 0 || proj.beatCubeList != null || proj.beatCubeList.Count != 0)
                        {
                            b.AppendLine(trackname);
                           
                            List<Difficulty> diffsToRemove = new List<Difficulty>();
                            foreach (var diff in proj.difficulties)
                            {
                                if(diff.beatCubeList.Count == 0)
                                {
                                    diffsToRemove.Add(diff);
                                }
                            }
                            proj.difficulties.RemoveAll(c => diffsToRemove.Contains(c));


                            if(proj.difficulties.Count == 0)
                            {
                                string defaultName = "Standard";
                                int defaultStars = 4;
                                bool useDefault = string.IsNullOrWhiteSpace(proj.difficultName);

                                b.AppendLine(" " + proj.difficultStars + "* " + proj.difficultName + " use default? " + useDefault);

                                proj.lastGivenDifficultyId = proj.lastGivenDifficultyId == -1 ? 0 : proj.lastGivenDifficultyId;
                                proj.difficulties.Add(new Difficulty()
                                {
                                    name = useDefault ? defaultName : proj.difficultName,
                                    beatCubeList = proj.beatCubeList,
                                    stars = useDefault ? defaultStars : proj.difficultStars,
                                    speed = 1,
                                    id = 0
                                });




                                var cls = new
                                {
                                    trackname = proj.author + " - " + proj.name,
                                    cubesCount = proj.beatCubeList?.Count,
                                    difficulties = proj.difficulties.Select(c => new { name = c.name, stars = c.stars, count = c.beatCubeList.Count }),
                                    diffName = proj.difficultName,
                                    Stars = proj.difficultStars
                                };

                                b.AppendLine(JsonConvert.SerializeObject(cls, Formatting.Indented));
                            }

                            //proj.beatCubeList.Clear();


                            ProjectManager.SaveProject(proj, path);
                        }
                    }
                    catch (Exception err)
                    {
                        logger.LogError(err.Message);
                    }
                }
            }

            return b.ToString();
        }

        public void UpgradeDifficulties(bool accept = false)
        {
            Response.StatusCode = 200;
            Response.ContentType = "text/html";

            // the easiest way to implement a streaming response, is to simply flush the stream after every write.
            // If you are writing to the stream asynchronously, you will want to use a Synchronized StreamWriter.

            Stopwatch w = Stopwatch.StartNew();

            string[] groupFolders = Directory.GetDirectories(settings.TracksFolder);


            using (var sw = StreamWriter.Synchronized(new StreamWriter(Response.Body)))
            {
                foreach (var groupFolder in groupFolders)
                {
                    string trackname = new DirectoryInfo(groupFolder).Name;
                    string[] mapperFolders = Directory.GetDirectories(groupFolder);

                    foreach (var mapperFolder in mapperFolders)
                    {
                        string mapper = new FileInfo(mapperFolder).Name;
                        
                        try
                        {
                            string response = GetDifficulties(trackname, mapper);
                            sw.WriteLine(response.Replace("\n", "<br>"));
                            logger.LogInformation(response);
                        }
                        catch (Exception err)
                        {
                            sw.WriteLine("<br>[ERR] " + trackname + " by " + mapper + "  " + err.Message);
                            logger.LogError(err.Message);
                        }


                        sw.WriteLine($"<br>[Processing time {w.ElapsedMilliseconds}ms]<br><br>");
                        sw.Flush();
                    }
                }
            };

            w.Stop();
        }
        public string GetDifficulties(string trackname, string mapper)
        {
            Stopwatch w = Stopwatch.StartNew();

            string mapFolder = settings.TracksFolder + "/" + trackname + "/" + mapper;
            string projectFile = mapFolder + "/" + trackname + ".bsz";

            Project proj = ProjectManager.LoadProject(projectFile);

            StringBuilder b = new StringBuilder();
            b.AppendLine("Difficulties count is " + proj.difficulties.Count);

            foreach (var diff in proj.difficulties)
            {
                b.AppendLine($"[{diff.id}] {diff.stars}* {diff.speed}x {diff.name}");
            }

            w.Stop();
            b.AppendLine("Time elapsed: " + w.ElapsedMilliseconds + "ms");

            return b.ToString();
        }*/
    }
}
