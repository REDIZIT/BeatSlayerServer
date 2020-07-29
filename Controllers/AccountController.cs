using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Newtonsoft.Json;
using BeatSlayerServer.ProjectManagement;
using BeatSlayerServer.Utils;
using System.Diagnostics;
using BeatSlayerServer.Services.Game;
using BeatSlayerServer.Services.Statistics;
using BeatSlayerServer.Utils.Statistics;
using BeatSlayerServer.Dtos;
using BeatSlayerServer.Models.Configuration;
using BeatSlayerServer.Services;
using BeatSlayerServer.Models.Database;
using System.Threading.Tasks;
using System.Text;

namespace BeatSlayerServer.Controllers
{
    /// <summary>
    /// Try to don't use AccountController coz all these methods should be in GameHub or WebAPI.
    /// This is legacy piece of code.
    /// </summary>
    public class AccountController : Controller
    {
        private readonly MyDbContext ctx;
        private readonly RankingService rankingService;
        private readonly HeartbeatService heartbeatService;
        private readonly AccountService accountService;
        private readonly ServerSettings settings;
        private readonly ConnectionService connectionService;

        public AccountController(MyDbContext ctx, ConnectionService connectionService, AccountService accountService, RankingService rankingService, HeartbeatService heartbeatService, SettingsWrapper wrapper)
        {
            this.ctx = ctx;
            this.connectionService = connectionService;
            this.rankingService = rankingService;
            this.heartbeatService = heartbeatService;
            this.accountService = accountService;
            settings = wrapper.settings;
        }

        public int Online()
        {
            return connectionService.ConnectedPlayers.Count();
        }


        public void OnGameLaunch(bool anonim = false)
        {
            //heartbeatService.OnGameLaunch(anonim);
            heartbeatService.AddData(new HeartbeatDataGameLaunch(anonim));
        }



        public IActionResult GetAvatarAsPicture(string nick)
        {
            string filepath = "Data/Accounts/Avatars/" + nick;
            string ext = System.IO.File.Exists(filepath + ".jpg") ? ".jpg" : System.IO.File.Exists(filepath + ".png") ? ".png" : "";

            if (ext == "") return File(System.IO.File.ReadAllBytes("Data/Accounts/default-avatar.png"), "image/png", "default-avatar.png");
            else
            {
                return File(System.IO.File.ReadAllBytes(filepath + ext), "image/" + (ext == ".png" ? "png" : "jpeg"));
            }
        }
        public IActionResult GetPublishedMaps(MyDbContext ctx, string nick, string password)
        {
            //Account acc = data.accounts.Find(c => c.nick == nick && c.password == password);
            var acc = ctx.Players.FirstOrDefault(c => c.Nick == nick && c.Password == password);
            if (acc == null) return Content(@"ERR: Invalid nick, password");


            string[] maps = Directory.GetDirectories(settings.TracksFolder);
            List<ProjectManagement.MapInfo> publishedMaps = new List<ProjectManagement.MapInfo>();

            foreach (string mapPath in maps)
            {
                
                string[] publishers = Directory.GetDirectories(mapPath);
                foreach (string publisher in publishers)
                {
                    string trackname = new DirectoryInfo(mapPath).Name;
                    string mapNick = new DirectoryInfo(publisher).Name;
                    if (mapNick == nick)
                    {
                        publishedMaps.Add(ProjectManager.GetMapInfo(trackname, nick));
                    }
                }
            }


            string json = JsonConvert.SerializeObject(publishedMaps);
            return Content(json);
        }

        public IActionResult CalculateLeaders()
        {
            return CalculateLeaderboardRP();
        }

        public IActionResult CalculateLeaderboardRP()
        {
            rankingService.CalculateLeaderboardPlaces();
            return Content("Success");
        }


        public IActionResult OnMapPlayed(bool approved)
        {
            heartbeatService.AddData(new HeartbeatDataMapPlayed(approved));
            Console.WriteLine("On map played. Approved? " + approved);
            return Content("Success");
        }
        
        public IActionResult GetBestReplay(string player, string trackname, string nick)
        {
            

            player = UrlHelper.Decode(player);
            trackname = UrlHelper.Decode(trackname);
            string author = trackname.Split('-')[0];
            string name = trackname.Split('-')[1];
            nick = UrlHelper.Decode(nick);

            var acc = ctx.Players.FirstOrDefault(c => c.Nick == player);
            if (acc == null) return Content("No such account");

            IEnumerable<ReplayInfo> allReplays = acc.Replays.Where(r => r.Map.Group.Author == author && r.Map.Group.Name == name && r.Map.Nick == nick);
            if (allReplays.Count() == 0) return Content("");

            ReplayInfo replay = allReplays.OrderByDescending(c => c.Score).First();

            Replay replayData = new Replay()
            {
                author = author,
                name = name,
                difficulty = replay.DifficultyStars,
                diffucltyName = replay.DifficultyName,
                cubesSpeed = 1,
                musicSpeed = 1,
                nick = nick,
                missed = replay.Missed,
                player = replay.Player.Nick,
                RP = replay.RP,
                score = replay.Score,
                sliced = replay.CubesSliced
            };

            return Content(JsonConvert.SerializeObject(replayData));
        }

       /* public IActionResult TransformRecordsToReplays()
        {
            foreach (Account acc in data.accounts)
            {
                foreach (AccountTrackRecord record in acc.records)
                {
                    Replay replay = new Replay(record);
                    acc.replays.Add(replay);
                }
            }

            SaveAccounts();
            return Content("Upgraded");
        }
       */
        public IActionResult GetMapGlobalLeaderboard(string trackname, string nick)
        {
            Console.WriteLine("Get map leaderboard for " + trackname + " by " + nick);

            trackname = UrlHelper.Decode(trackname);
            nick = UrlHelper.Decode(nick);

            string author = trackname.Split('-')[0].Trim();
            string name = trackname.Split('-')[1].Trim();


            //List<Account> accounts = data.accounts.Where(p => p.replays.Any(r => r.author + "-" + r.name == trackname && r.nick == nick)).ToList();
            //var accounts = Core.ctx.Players.Where(p => p.Replays.Any(r => r.Map.Group.Author == author && r.Map.Group.Name == name && r.Map.Nick == nick));

            List<Replay> replays = new List<Replay>();
            /*foreach (var account in accounts)
            {
                var accountReplays = account.Replays.Where(r => r.Map.Group.Author == author && r.Map.Group.Name == name && r.Map.Nick == nick)
                    .OrderByDescending(c => c.RP);
                if (accountReplays.Count() == 0) continue;

                var replay = accountReplays.First();
                replay.Player.Nick = account.Nick;
                replays.Add(replay);
            }
            */

            var dbaccounts = ctx.Players.Where(p =>
                p.Replays.Any(r =>
                    r.Map.Group.Author.Trim() == author && r.Map.Group.Name.Trim() == name && r.Map.Nick == nick)).ToList();

            foreach (var account in dbaccounts)
            {
                var accountReplays = account.Replays.Where(r => r.Map.Group.Author.Trim() == author && r.Map.Group.Name.Trim() == name)
                    .OrderByDescending(c => c.RP).ToList();
                if (accountReplays.Count() == 0) continue;

                var dbreplay = accountReplays.First();

                Replay replay = new Replay()
                {
                    author = author,
                    name = name,
                    nick = nick,
                    sliced = dbreplay.CubesSliced,
                    difficulty = dbreplay.DifficultyStars,
                    diffucltyName = dbreplay.DifficultyName,
                    missed = dbreplay.Missed,
                    player = dbreplay.Player.Nick,
                    RP = dbreplay.RP,
                    score = dbreplay.Score,
                    cubesSpeed = 1,
                    musicSpeed = 1
                };

                replays.Add(replay);
            }

            replays = replays.OrderByDescending(c => c.RP).ToList();

            return Content(JsonConvert.SerializeObject(replays));
        }
        public IActionResult GetMapLeaderboardPlace(string player, string trackname, string nick)
        {
            //List<Account> accounts = data.accounts.Where(p => p.replays.Any(r => r.author + "-" + r.name == trackname && r.nick == nick)).ToList();

            string author = trackname.Split('-')[0];
            string name = trackname.Split('-')[1];

            var accounts = ctx.Players.Where(p => p.Replays.Any(r => 
                r.Map.Group.Author.Trim() == author && r.Map.Group.Name.Trim() == name && r.Map.Nick == nick));

            List<Replay> replays = new List<Replay>();
            foreach (var account in accounts.ToList())
            {
                var accountReplays = account.Replays.Where(r => r.Map.Group.Author.Trim() == author && r.Map.Group.Name.Trim() == name && r.Map.Nick == nick)
                    .OrderByDescending(c => c.RP).ToList();
                if (accountReplays.Count() == 0) continue;

                Replay replay = CastToReplay(accountReplays.First());
                replay.player = account.Nick;
                replays.Add(replay);
            }
            replays = replays.OrderByDescending(c => c.RP).ToList();

            return Content((replays.FindIndex(c => c.player == player) + 1) + "");
        }
        public async Task<string> GetMapLeaderboard(string trackname, string mapper)
        {
            var items = await accountService.GetMapLeaderboard(trackname, mapper);

            StringBuilder b = new StringBuilder();
            foreach (var item in items)
            {
                b.AppendLine("#" + item.Place + " " + item.Grade + "   " + item.Nick + "   " + item.RP + "   " + item.Score + "    " + item.MissedCount + " / " + item.SlicedCount);
            }

            return b.ToString();
        }
        public string GetReplays(string nick)
        {
            var items = accountService.GetReplays(nick);

            StringBuilder b = new StringBuilder();
            foreach (var item in items)
            {
                b.AppendLine(item.Map.Group.Author + "-" + item.Map.Group.Name + "   " + item.RP + "   " + item.Score);
            }

            return b.ToString();
        }

        public IActionResult GetGlobalLeaderboard()
        {
            try
            {
                Stopwatch w = new Stopwatch();
                w.Start();
                //IEnumerable<Account> accounts = data.accounts.Where(c => c.RP > 0);
                var accounts = ctx.Players.Where(c => c.RP > 0).ToList();

                Console.WriteLine($"Got accounts in {w.ElapsedMilliseconds}ms");
                w.Restart();


                List<LeaderboardItem> leaderboardItems = new List<LeaderboardItem>();
                int place = 0;
                foreach (var acc in accounts)
                {
                    place++;
                    LeaderboardItem item = new LeaderboardItem()
                    {
                        Nick = acc.Nick,
                        Place = place,
                        PlayCount = acc.Replays.Count,
                        SlicedCount = acc.Hits,
                        MissedCount = acc.Misses,
                        Score = acc.AllScore,
                        RP = acc.RP
                    };
                    leaderboardItems.Add(item);
                }

                Console.WriteLine($"Got leaderboard in {w.ElapsedMilliseconds}ms. Avg time is {w.ElapsedMilliseconds / accounts.Count()}ms");
                w.Restart();

                leaderboardItems = leaderboardItems.OrderByDescending(c => c.RP).ToList();

                Console.WriteLine($"Got sorted time in {w.ElapsedMilliseconds}ms");
                w.Restart();

                string json = JsonConvert.SerializeObject(leaderboardItems);
                return Content(json);
            }
            catch(Exception err)
            {
                return Content("Error: " + err);
            }
            
        }

        //#endregion

        #region AccountUpdating

        /*public IActionResult UpdateInGameTime(string nick, float seconds)
        {
            Account acc = data.accounts.Find(c => c.nick == nick);
            if (acc == null) return Content("[ERR] No such account");

            acc.playTime += TimeSpan.FromSeconds(seconds);

            SaveAccounts();
            return Content("Success");
        }*/

        #endregion



        /*public IActionResult ReloadData()
        {
            LoadAccounts();

            return Content("Reloaded");
        }
        public IActionResult DoSaveAccounts()
        {
            SaveAccounts();
            return Content("Saved");
        }
        public static void LoadAccounts()
        {
            string filePath = "Data/Accounts/accounts.xml";

            if(!System.IO.File.Exists(filePath))
            {
                SaveAccounts(data);
            }

            XmlSerializer xml = new XmlSerializer(typeof(AccountData));
            using (var stream = System.IO.File.OpenRead(filePath))
            {
                data = (AccountData)xml.Deserialize(stream);
            }
        }

        public static void SaveAccounts()
        {
            SaveAccounts(data);
        }
        public static void SaveAccounts(AccountData data)
        {
            string folderPath = "Data/Accounts";
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            string filePath = folderPath + "/accounts.xml";

            XmlSerializer xml = new XmlSerializer(typeof(AccountData));
            using (var stream = System.IO.File.Create(filePath))
            {
                xml.Serialize(stream, data);
            }
        }*/

        private static Replay CastToReplay(ReplayInfo info)
        {
            return new Replay()
            {
                author = info.Map.Group.Author,
                name = info.Map.Group.Name,
                difficulty = info.DifficultyStars,
                diffucltyName = info.DifficultyName,
                cubesSpeed = 1,
                musicSpeed = 1,
                nick = info.Map.Nick,
                missed = info.Missed,
                player = info.Player.Nick,
                RP = info.RP,
                score = info.Score,
                sliced = info.CubesSliced
            };
        }
    }

    public class AccountData
    {
        public List<Account> accounts = new List<Account>();
    }






    public class Account
    {
        public string nick;
        public string email;
        public string password;
        public string role;
        public AccountRole Role 
        { 
            get 
            { 
                return role == null || role == "" ? AccountRole.Player : (AccountRole)Enum.Parse(typeof(AccountRole), role); 
            } 
        }

        public TimeSpan playTime;

        public DateTime regTime;
        public DateTime activeTime;


        public int ratingPlace;
        public float score;

        public List<AccountMapInfo> playedMaps = new List<AccountMapInfo>();
        public List<AccountTrackRecord> records = new List<AccountTrackRecord>();

        public double RP 
        {
            get
            {
                return replays.OrderByDescending(c => c.score).GroupBy(c => c.author + "-" + c.name).Select(c => c.First()).Sum(c => c.RP);
            }
        }
        public double TotalRP { get { return replays.Sum(c => c.RP); } }
        public List<Replay> replays = new List<Replay>();
    }
    public enum AccountRole
    {
        Player,
        Developer,
        Moderator
    }

    public class AccountTrackRecord
    {
        public string author, name, nick;

        public float score = 0, accuracy = 0; // Accuracy in 1.0
        public int missed = 0, sliced = 0;
    }

    /// <summary>
    /// Used for transfering from server to game, and in game leaderboards
    /// </summary>
    public class LeaderboardItem
    {
        public string Nick { get; set; }
        public int Place { get; set; }
        public int PlayCount { get; set; }
        public int SlicedCount { get; set; }
        public int MissedCount { get; set; }
        public double RP { get; set; }
        public double Score { get; set; }
        public Grade Grade { get; set; }
    }


    public class AccountMapInfo
    {
        public string author, name, nick;
        public int playTimes;
    }
}
