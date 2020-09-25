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
using BeatSlayerServer.Models.Maps;
using System.Drawing;

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



        public MapsData GetGroup(string trackname)
        {
            string[] mapsFolders = Directory.GetDirectories(settings.TracksFolder + "/" + trackname);
            MapsData groupInfo = new MapsData()
            {
                Author = trackname.Split('-')[0],
                Name = trackname.Split('-')[1]
            };


            foreach (string mapFolder in mapsFolders)
            {
                MapInfo info = ProjectManager.GetMapInfo(mapFolder, true);
                groupInfo.Downloads += info.downloads;
                groupInfo.PlayCount += info.playCount;
                groupInfo.Launches += info.LaunchesCount;
                groupInfo.Likes += info.likes;
                groupInfo.Dislikes += info.dislikes;

                if (info.publishTime > groupInfo.UpdateTime) groupInfo.UpdateTime = info.publishTime;

                groupInfo.MappersNicks.Add(new DirectoryInfo(mapFolder).Name);
            }

            return groupInfo;
        }
        public List<MapsData> GetGroupsExtended()
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
                        groupInfo.Downloads += info.downloads;
                        groupInfo.PlayCount += info.playCount;
                        groupInfo.Launches += info.LaunchesCount;
                        groupInfo.Likes += info.likes;
                        groupInfo.Dislikes += info.dislikes;
                        if (!groupInfo.IsApproved) groupInfo.IsApproved = info.approved;

                        if (info.publishTime > groupInfo.UpdateTime) groupInfo.UpdateTime = info.publishTime;

                        groupInfo.MappersNicks.Add(new DirectoryInfo(mapFolder).Name);
                    }
                    catch (Exception err)
                    {
                        throw new Exception($"GetGroupsExtended error with map {trackname}.\n" + err);
                    }
                }

                groupInfos.Add(groupInfo);
            }

            return groupInfos;
        }

        public List<MapsData> GetGroupsExtended(bool fromDb)
        {
            if (!fromDb) return GetGroupsExtended();


            List<MapsData> result = new List<MapsData>();

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

        public List<FullMapData> GetMaps(string trackname)
        {
            string groupFolder = settings.TracksFolder + "/" + trackname.Replace("%amp%", "&");
            if (!Directory.Exists(groupFolder)) throw new Exception("Group has been deleted");

            string[] mapsFolders = Directory.GetDirectories(groupFolder);
            List<FullMapData> mapInfos = new List<FullMapData>();
            for (int i = 0; i < mapsFolders.Length; i++)
            {
                string nick = new DirectoryInfo(mapsFolders[i]).Name;
                mapInfos.Add(new FullMapData(ProjectManager.GetMapInfo(trackname, nick)));
            }

            return mapInfos;
        }
        public OperationResult GetMapsWithResult(string trackname)
        {
            try
            {
                List<FullMapData> ls = GetMaps(trackname);
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
        public void CreateCovers(string trackname, string mapper)
        {
            if (!TryGetCoverPath(trackname, mapper, out string coverPath)) return;

            string baseName = coverPath.Replace(Path.GetExtension(coverPath), "");
            Bitmap square = PictureHelper.CutImage(coverPath);

            CreateCover(square, 512, baseName, Path.GetExtension(coverPath));
            CreateCover(square, 256, baseName, Path.GetExtension(coverPath));
            CreateCover(square, 128, baseName, Path.GetExtension(coverPath));
        }
        private void CreateCover(Bitmap image, int size, string baseName, string extension)
        {
            Bitmap huge = PictureHelper.ResizeImage(image, size, size);

            huge.Save(baseName + $"_{size}x{size}" + extension);
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
}
