using BeatSlayerServer.Utils;
using BeatSlayerServer.Utils.Database;
using BeatSlayerServer.ProjectManagement;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using BeatSlayerServer.Models.Configuration;
using System.Threading.Tasks;
using LeaderboardItem = BeatSlayerServer.Controllers.LeaderboardItem;
using System.Diagnostics;
using BeatSlayerServer.Extensions;
using BeatSlayerServer.Services.Messaging;
using BeatSlayerServer.Models.Database;

namespace BeatSlayerServer.Services.Game
{
    public class AccountService
    {
        private readonly MyDbContext ctx;
        private readonly ILogger<AccountService> logger;
        private readonly ServerSettings settings;

        private readonly AccountFilesService filesService;
        private readonly VerificationService verificationService;
        private readonly BotService botService;


        public AccountService(MyDbContext ctx, SettingsWrapper wrapper, ILogger<AccountService> logger, AccountFilesService filesService, VerificationService verificationService, BotService botService)
        {
            this.ctx = ctx;
            this.logger = logger;
            settings = wrapper.settings;

            this.filesService = filesService;
            this.verificationService = verificationService;
            this.botService = botService;
        }



        public OperationMessage SignUp(string nick, string password, string country, string email)
        {
            if (ctx.Players.Any(c => c.Nick == nick))
            {
                return new OperationMessage(OperationType.Fail, "Player with this nick already exists");
            }
            else if (email.Trim() != "" && ctx.Players.Any(c => c.Email == email))
            {
                return new OperationMessage(OperationType.Fail, "Player with this email already exists");
            }
            else
            {
                if (!IsMatch(nick)) return new OperationMessage(OperationType.Fail, "You should use English alphabet");

                Utils.Database.Account account = new Utils.Database.Account(nick, password, country)
                {
                    Email = email
                };
                account.Coins = 2000;
                ctx.Players.Add(account);
                ctx.SaveChanges();


                filesService.CreateDataFolder(nick);

                logger.LogInformation("[{action}] {nick}", "SIGNUP", nick);

                return new OperationMessage(OperationType.Success, "");
            }
        }
        public OperationMessage LogIn(string nick, string password)
        {
            try
            {
                string hash = SecurityHelper.GetMd5Hash(password);
                Utils.Database.Account acc = ctx.Players.FirstOrDefault(c => c.Nick == nick);
                if (acc == null)
                {
                    return new OperationMessage(OperationType.Fail, "No such account");
                }
                else
                {
                    if (acc.Password != hash)
                    {
                        return new OperationMessage(OperationType.Fail, "Invalid password");
                    }
                    else
                    {
                        UpdateActiveTime(acc);
                        return new OperationMessage(OperationType.Success, acc.Cut(Utils.Database.Account.ToStringType.Login, password));
                    }
                }
            }
            catch (Exception err)
            {
                return new OperationMessage(OperationType.Fail, "Server error: " + err.Message);
            }
        }



        public void UpdateInGameTime(string nick, int seconds)
        {
            Utils.Database.Account acc = ctx.Players.FirstOrDefault(c => c.Nick == nick);
            if (acc == null) return;
            if (seconds >= 5 * 60) return;

            acc.InGameTime += TimeSpan.FromSeconds(seconds);
            acc.LastActiveTimeUtc = DateTime.UtcNow;
            ctx.SaveChanges();
        }
        void UpdateActiveTime(Utils.Database.Account acc)
        {
            acc.LastActiveTimeUtc = DateTime.UtcNow;
            ctx.SaveChanges();
        }


        public List<Utils.Database.AccountData> Find(string str)
        {
            IEnumerable<Utils.Database.Account> matches = ctx.Players.Where(c => c.Nick.ToLower().Contains(str.ToLower()));
            matches = matches.OrderByDescending(c => c.Nick.ToLower() == str.ToLower());

            List<Utils.Database.AccountData> data = new List<Utils.Database.AccountData>();
            foreach (var acc in matches.Take(30))
            {
                data.Add(new Utils.Database.AccountData()
                {
                    Nick = acc.Nick,
                    LastActiveTimeUtcTicks = acc.LastActiveTimeUtc.Ticks
                });
            }
            return data;
        }


        public async Task<OperationMessage> Restore(string nick, string newpassword)
        {
            Utils.Database.Account acc = ctx.Players.FirstOrDefault(c => c.Nick == nick);
            if (acc == null) return new OperationMessage(OperationType.Fail, "No such account");
            if (acc.Email == null || !acc.Email.Contains("@")) return new OperationMessage(OperationType.Fail, "Mail isn't attached to account");

            Console.WriteLine("Restore for " + nick);

            string code = SecurityHelper.GetCode();
            verificationService.Send(nick, newpassword, code);

            await botService.SendRestorePasswordCode(acc.Nick, acc.Email, code);

            return new OperationMessage(OperationType.Success);
        }
        public bool ConfirmRestore(string code)
        {
            var info = ctx.VerificationRequests.FirstOrDefault(c => c.Code == code);
            if (info == null) return false;

            verificationService.Remove(info);

            Utils.Database.Account acc = ctx.Players.FirstOrDefault(c => c.Nick == info.Nick);
            if (acc == null) return false;

            acc.Password = SecurityHelper.GetMd5Hash(info.Value);
            ctx.SaveChanges();

            Console.WriteLine("Password changed successfully");

            return true;
        }
        public OperationMessage ChangePassword(string nick, string currentPassword, string newPassword)
        {
            string hash = SecurityHelper.GetMd5Hash(currentPassword);
            Utils.Database.Account acc = ctx.Players.FirstOrDefault(c => c.Nick == nick && c.Password == hash);
            if (acc == null) return new OperationMessage(OperationType.Fail, "No such account or invalid password");

            acc.Password = SecurityHelper.GetMd5Hash(newPassword);
            ctx.SaveChanges();

            return new OperationMessage(OperationType.Success);
        }


        public async Task SendChangeEmailCode(string nick, string newEmail)
        {
            Console.WriteLine("Change email");

            Utils.Database.Account acc = ctx.Players.FirstOrDefault(c => c.Nick == nick);
            if (acc == null) return;

            string code = SecurityHelper.GetCode();

            verificationService.Send(nick, newEmail, code);
            await botService.SendChangeEmailCode(acc.Nick, acc.Email, code);
        }
        public OperationMessage ChangeEmail(string nick, string code)
        {
            Utils.Database.Account acc = ctx.Players.FirstOrDefault(c => c.Nick == nick);
            if (acc == null) return new OperationMessage(OperationType.Fail, "No such account");


            var req = ctx.VerificationRequests.FirstOrDefault(c => c.Nick == nick && c.Code == code);
            if (req == null) return new OperationMessage(OperationType.Fail, "Invalid code");

            acc.Email = req.Value;
            verificationService.Remove(req);

            return new OperationMessage(OperationType.Success);
        }
        public OperationMessage ChangeEmptyEmail(string nick, string newEmail)
        {
            Utils.Database.Account acc = ctx.Players.FirstOrDefault(c => c.Nick == nick);
            if (acc == null) return new OperationMessage(OperationType.Fail, "No such account");

            if (string.IsNullOrEmpty(acc.Email))
            {
                acc.Email = newEmail;
                ctx.SaveChanges();
                return new OperationMessage(OperationType.Success);
            }
            else return new OperationMessage(OperationType.Fail, "Email isn't empty");
        }



        public OperationMessage GetPublishedMaps(string nick, string password)
        {
            if (!TryFindAccount(nick, out Utils.Database.Account acc)) return new OperationMessage(OperationType.Fail, "No such account");


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
            return new OperationMessage(OperationType.Success, json);
        }





        public byte[] GetAvatar(string nick)
        {
            return filesService.GetAvatar(nick);
        }
        public OperationMessage ChangeAvatar(string nick, byte[] bytes, string extenstion)
        {
            filesService.SetAvatar(nick, bytes, extenstion);
            return new OperationMessage(OperationType.Success);
        }


        public byte[] GetBackground(string nick)
        {
            return filesService.GetBackground(nick);
        }
        public OperationMessage ChangeBackground(string nick, byte[] bytes, string extenstion)
        {
            filesService.SetBackground(nick, bytes, extenstion);

            return new OperationMessage(OperationType.Success);
        }









        public AccountData GetAccountDataByNick(string nick)
        {
            if (!TryFindAccount(nick, out Account acc)) return null;

            AccountData data = acc.Cut(Account.ToStringType.View);
            return data;
        }

        public List<Utils.Database.AccountData> GetFriends(string nick)
        {
            if (!TryFindAccount(nick, out Utils.Database.Account acc)) return null;

            List<Utils.Database.AccountData> friends = new List<Utils.Database.AccountData>();

            foreach (var info in acc.Friends)
            {
                friends.Add(new Utils.Database.AccountData()
                {
                    Id = info.Id,
                    Nick = info.Nick,
                    LastActiveTimeUtcTicks = info.LastActiveTimeUtc.Ticks
                });
            }

            return friends;
        }
        



        public ReplayInfo GetBestReplay(string nick, string trackname, string creatornick)
        {
            if (!TryFindAccount(nick, out Utils.Database.Account acc)) return null;

            var infos = acc.Replays.Where(c => c.Map.Group.Author + "-" + c.Map.Group.Name == trackname && c.Map.Nick == creatornick);
            return infos.OrderByDescending(c => c.RP).FirstOrDefault();
        }
        public List<ReplayInfo> GetBestReplays(string nick, int count)
        {
            if (!TryFindAccount(nick, out Utils.Database.Account acc)) return null;

            return acc.Replays.OrderByDescending(c => c.RP).Take(count).ToList();
        }
        public List<ReplayInfo> GetReplays(string nick)
        {
            if (!TryFindAccount(nick, out Utils.Database.Account acc)) return null;

            return acc.Replays.OrderByDescending(c => c.RP).ToList();
        }



        //public void ClearMapLeaderboard(string trackname, string nick) { }

        public Task<List<LeaderboardItem>> GetMapLeaderboard(string trackname, string nick)
        {
            return Task.Run(() =>
            {
                Stopwatch w = new Stopwatch();
                w.Start();

                string author = trackname.Split('-')[0].Trim();
                string name = trackname.Split('-')[1].Trim();

                var group = ctx.Groups.FirstOrDefault(c => c.Author.Trim() == author && c.Name.Trim() == name);
                var map = group.Maps.FirstOrDefault(c => c.Nick == nick);


                List<LeaderboardItem> leaderboardItems = new List<LeaderboardItem>();
                int place = 0;

                foreach (var replay in map.Replays.ToList().OrderByDescending(c => c.RP).DistinctBy(c => c.Player.Nick))
                {
                    place++;
                    leaderboardItems.Add(new LeaderboardItem()
                    {
                        Nick = replay.Player.Nick,
                        Place = place,
                        SlicedCount = replay.CubesSliced,
                        MissedCount = replay.Missed,
                        Score = replay.Score,
                        RP = replay.RP,
                        Grade = replay.Grade,
                        //Mods = 
                    });
                }

                Console.WriteLine("Elapsed time is " + w.ElapsedMilliseconds);

                return leaderboardItems;
                
                
            });
        }
        public Task<List<LeaderboardItem>> GetGlobalLeaderboard()
        {
            return Task.Run(() =>
            {
                var accounts = ctx.Players.Where(c => c.RP > 0).ToList();

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
                return leaderboardItems.OrderByDescending(c => c.RP).ToList();
            });
        }

        public Task<bool> IsPassed(string nick, string author, string name)
        {
            return Task.Run(() =>
            {
                if (!TryFindAccount(nick, out Utils.Database.Account acc)) return false;

                try
                {
                    return acc.Replays.Any(c => c.Map.Group.Author.Trim() == author.Trim() && c.Map.Group.Name.Trim() == name.Trim());
                }
                catch
                {
                    // Replay.Map.Group is null
                    //Console.WriteLine("Got error IsPassed for " + nick + " " + author + "-" + name);
                    return false;
                }
            });
        }



        private bool IsMatch(string nick)
        {
            Regex r = new Regex(@"^[a-zA-Z0-9/_()!-+=<> ]+$");
            return r.IsMatch(nick);
        }

        public bool TryFindAccount(string nick, out Utils.Database.Account account)
        {
            account = ctx.Players.FirstOrDefault(c => c.Nick == nick);

            return account != null;
        }
    }
}
