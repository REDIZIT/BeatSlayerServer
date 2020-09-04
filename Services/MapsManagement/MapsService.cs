using BeatSlayerServer.Utils;
using BeatSlayerServer.ProjectManagement;
using System.Linq;
using System.IO;
using BeatSlayerServer.Models.Configuration;
using System.Collections.Generic;
using System;
using BeatSlayerServer.Enums;
using System.Threading.Tasks;
using MapInfo = BeatSlayerServer.ProjectManagement.MapInfo;
using System.Diagnostics;
using System.Text;

namespace BeatSlayerServer.Services.MapsManagement
{
    /// <summary>
    /// Working with maps, groups on server. Reading mapinfo, groupinfo, loading project files.
    /// </summary>
    public class MapsService
    {
        private readonly MyDbContext ctx;
        private readonly ServerSettings settings;
        private readonly ModerationService moderationService;

        public MapsService(MyDbContext ctx, SettingsWrapper wrapper, ModerationService moderationService)
        {
            this.ctx = ctx;
            settings = wrapper.settings;
            this.moderationService = moderationService;
        }



        public GroupInfoExtended GetGroup(string trackname)
        {
            string[] mapsFolders = Directory.GetDirectories(settings.TracksFolder + "/" + trackname);
            GroupInfoExtended groupInfo = new GroupInfoExtended()
            {
                author = trackname.Split('-')[0],
                name = trackname.Split('-')[1],
                mapsCount = mapsFolders.Length
            };


            groupInfo.nicks = new List<string>();
            foreach (string mapFolder in mapsFolders)
            {
                MapInfo info = ProjectManager.GetMapInfo(mapFolder, true);
                groupInfo.allDownloads += info.downloads;
                groupInfo.allPlays += info.playCount;
                groupInfo.allLikes += info.likes;
                groupInfo.allDislikes += info.dislikes;

                if (info.publishTime > groupInfo.updateTime) groupInfo.updateTime = info.publishTime;

                groupInfo.nicks.Add(new DirectoryInfo(mapFolder).Name);
            }

            return groupInfo;
        }
        public List<GroupInfoExtended> GetGroupsExtended()
        {
            string[] tracksFolders = Directory.GetDirectories(settings.TracksFolder).OrderByDescending(c => new DirectoryInfo(c).CreationTime).ToArray();
            List<GroupInfoExtended> groupInfos = new List<GroupInfoExtended>();

            for (int i = 0; i < tracksFolders.Length; i++)
            {
                string trackname = new DirectoryInfo(tracksFolders[i]).Name;

                string[] mapsFolders = Directory.GetDirectories(tracksFolders[i]);

                GroupInfoExtended groupInfo = new GroupInfoExtended()
                {
                    author = trackname.Split('-')[0],
                    name = trackname.Split('-')[1],
                    mapsCount = mapsFolders.Length
                };


                groupInfo.nicks = new List<string>();
                foreach (string mapFolder in mapsFolders)
                {
                    try
                    {
                        MapInfo info = ProjectManager.GetMapInfo(mapFolder, true);
                        groupInfo.allDownloads += info.downloads;
                        groupInfo.allPlays += info.playCount;
                        groupInfo.allLikes += info.likes;
                        groupInfo.allDislikes += info.dislikes;

                        if (info.publishTime > groupInfo.updateTime) groupInfo.updateTime = info.publishTime;

                        groupInfo.nicks.Add(new DirectoryInfo(mapFolder).Name);
                    }
                    catch (Exception err)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("GetGroupExtended: " + mapFolder);
                        Console.WriteLine("    " + err);
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }

                groupInfos.Add(groupInfo);
            }

            return groupInfos;
        }

        public List<GroupInfoExtended> GetGroupsExtended(bool fromDb)
        {
            if (!fromDb) return GetGroupsExtended();


            List<GroupInfoExtended> result = new List<GroupInfoExtended>();

            Stopwatch w = Stopwatch.StartNew();


            var groups = ctx.Groups.Select(g => new
            {
                author = g.Author,
                name = g.Name,
                maps = g.Maps.Select(m => new
                {
                    difficulties = m.Difficulties.Select(d => new
                    {
                        name = d.Name
                    })
                })
            });

            int count = groups.Count();
            //int allLikes = groups.Sum(c => c.allLikes);

            foreach (var item in groups)
            {
                Console.WriteLine(item.maps.FirstOrDefault()?.difficulties?.Count());
            }


            w.Stop();
            float elapsedTime = w.ElapsedMilliseconds;

            return result;
        }




        public async Task<string> GetRandomGroup()
        {
            return await Task.Run(() =>
            {
                string[] tracksFolders = Directory.GetDirectories(settings.TracksFolder).OrderByDescending(c => new DirectoryInfo(c).CreationTime).ToArray();

                int random = new Random().Next(0, tracksFolders.Length - 1);

                string trackname = new DirectoryInfo(tracksFolders[random]).Name;
                return trackname;
            });
        }

        public List<MapInfo> GetMaps(string trackname)
        {
            string groupFolder = settings.TracksFolder + "/" + trackname.Replace("%amp%", "&");
            if (!Directory.Exists(groupFolder)) throw new Exception("Group has been deleted");

            string[] mapsFolders = Directory.GetDirectories(groupFolder);
            List<MapInfo> mapInfos = new List<MapInfo>();
            for (int i = 0; i < mapsFolders.Length; i++)
            {
                string nick = new DirectoryInfo(mapsFolders[i]).Name;
                mapInfos.Add(ProjectManager.GetMapInfo(trackname, nick));
            }

            return mapInfos;
        }
        public OperationResult GetMapsWithResult(string trackname)
        {
            try
            {
                List<MapInfo> ls = GetMaps(trackname);
                return new OperationResult(OperationResult.State.Success, ls);
            }
            catch (Exception err)
            {
                return new OperationResult(err);
            }
        }
        public MapInfo GetMap(string trackname, string nick)
        {
            return ProjectManager.GetMapInfo(trackname, nick);
        }
        public bool DoesMapExist(string trackname, string nick)
        {
            string groupFolder = settings.TracksFolder + "/" + trackname.Replace("%amp%", "&");
            if (!Directory.Exists(groupFolder)) return false;

            if (nick == null || nick == "") return true; // Return true coz group exists

            string mapFolder = groupFolder + "/" + nick.Replace("%amp%", "&");
            return Directory.Exists(mapFolder);
        }
        public bool HasUpdateForMap(string trackname, string nick, long utcTicks)
        {
            long publishedTicks = new FileInfo(settings.TracksFolder + "/" + trackname + "/" + nick + "/" + trackname + ".bsz").LastWriteTimeUtc.Ticks;
            return publishedTicks > utcTicks;
        }




        public OperationMessage DeleteProject(string trackname, string nick, string password = "", string masterkey = "")
        {
            if (password == "" && masterkey != "sosipisun") return new OperationMessage(OperationType.Fail, "Wrong password");


            return new OperationMessage(OperationType.Fail, "Not implemented");
        }
        public OperationMessage RenameProject(string trackname, string nick, string newtrackname, string masterkey = "")
        {
            return new OperationMessage(OperationType.Fail, "Not implemented");
        }

        public OperationMessage RemoveMapFromApproved(string trackname, string nick, string masterkey)
        {
            if (masterkey != "sosipisun") return new OperationMessage(OperationType.Fail, "Wrong password");

            string author = trackname.Split('-')[0];
            string name = trackname.Split('-')[1];

            // Remove approved status for map
            /// Remove in MapInfo
            ProjectManagement.MapInfo info = ProjectManager.GetMapInfo(trackname, nick);
            info.approved = false;
            ProjectManager.SetMapInfo(info);

            /// Remove from approved list
            moderationService.data.approvedMaps.RemoveAll(c => c.group.author + "-" + c.group.name == trackname && c.nick == nick);
            moderationService.SaveData();


            // Reset all records with this map
            /*foreach (var accjs in AccountController.data.accounts)
            {
                accjs.replays.RemoveAll(c => c.author == info.group.author && c.name == info.group.name && c.nick == info.nick);
                accjs.playedMaps.RemoveAll(c => c.author == info.group.author && c.name == info.group.name && c.nick == info.nick);
                accjs.records.RemoveAll(c => c.author == info.group.author && c.name == info.group.name && c.nick == info.nick);
            }*/

            // Not implemented
            /*foreach (var acc in Core.ctx.Players)
            {
                acc.Replays.RemoveAll(c => c.Map.Group.Author == author && c.Map.Group.Name == name && c.Map.Nick == nick);
            }*/
            //AccountController.SaveAccounts();

            // No integration with actual db accounts (ef core)

            return new OperationMessage(OperationType.Success);
        }


        private bool TryGetCoverPath(string trackname, string nick, out string coverPath)
        {
            string groupFolder = settings.TracksFolder + "/" + trackname;
            string mapperFolder = groupFolder + "/" + nick;
            string possibleCoverPath;

            try
            {
                // If nick is empty or no such mapper folder return first mapper icon
                if (nick == "")
                {
                    mapperFolder = Directory.GetDirectories(groupFolder)[0];
                }

                possibleCoverPath = mapperFolder + "/" + trackname;
            }
            catch
            {
                coverPath = "";
                return false;
            }

            return FileHelper.TryFindFile(possibleCoverPath, out coverPath, ".jpg", ".png");
        }
        public byte[] GetDefaultCover()
        {
            return File.ReadAllBytes(settings.DefaultMapIcon);
        }
        public byte[] GetCover(string trackname, string nick)
        {
            if(TryGetCoverPath(trackname, nick, out string coverPath))
            {
                return File.ReadAllBytes(coverPath);
            }
            else
            {
                return GetDefaultCover();
            }
        }




        public bool TryGetGroupInfo(string author, string name, out Utils.Database.GroupInfo group)
        {
            group = ctx.Groups.FirstOrDefault(c => c.Author.Trim() == author.Trim() && c.Name.Trim() == name.Trim());
            return group != null;
        }
        public bool TryGetMapInfo(Utils.Database.GroupInfo group, string nick, out Models.Database.MapInfo map)
        {
            map = group.Maps.FirstOrDefault(c => c.Nick == nick);
            return map != null;
        }
    }


    /// <summary>
    /// Describes a maixum which player can get on map. Will be moved to GroupInfo and MapInfo in future.
    /// </summary>
    public class MapMaxResult
    {
        public int CubesCount { get; set; }
        public int MaxCombo { get; set; }
        public int MaxRP { get; set; }
    }
}
