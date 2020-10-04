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
        public FullMapData GetMap(string trackname, string nick)
        {
            return new FullMapData(ProjectManager.GetMapInfo(trackname, nick));
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


        public byte[] GetDefaultCover()
        {
            return File.ReadAllBytes(settings.DefaultMapIcon);
        }
        public byte[] GetCover(string trackname, string nick, ImageSize size)
        {
            if (TryGetCoverPath(trackname, out string coverPath, nick, size))
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
            string coverPath = settings.TracksFolder + "/" + trackname + "/" + mapper + "/" + trackname + ".jpg";
            if (!File.Exists(coverPath))
            {
                coverPath = Path.ChangeExtension(coverPath, ".png");
                if (!File.Exists(coverPath)) return;
            }

            string baseName = coverPath.Replace(Path.GetExtension(coverPath), "");

            string filepathHigh = baseName + $"_{512}x{512}" + Path.GetExtension(coverPath);
            string filepathLow = baseName + $"_{128}x{128}" + Path.GetExtension(coverPath);

            if (File.Exists(filepathHigh) && File.Exists(filepathLow)) return;

            using (Bitmap square = PictureHelper.CutImage(coverPath))
            {
                CreateCover(square, 512, baseName, Path.GetExtension(coverPath));
                CreateCover(square, 128, baseName, Path.GetExtension(coverPath));
            }
        }
        private void CreateCover(Bitmap image, int size, string baseName, string extension)
        {
            string filepath = baseName + $"_{size}x{size}" + extension;
            if (File.Exists(filepath)) return;

            using (Bitmap map = PictureHelper.ResizeImage(image, size, size))
            {
                map.Save(filepath);
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




        /// <summary>
        /// Try find path to cover file
        /// </summary>
        /// <param name="size">Preferred size, if won't be found then will returned another</param>
        /// <param name="coverPath">Result path to map cover file or null</param>
        private bool TryGetCoverPath(string trackname, out string coverPath, string nick = "", ImageSize size = ImageSize._512x512)
        {
            // Tracks/Author-Name
            string groupFolder = settings.TracksFolder + "/" + trackname;

            if (!Directory.Exists(groupFolder))
            {
                coverPath = "";
                return false;
            }

            // If nick is given
            if (!string.IsNullOrWhiteSpace(nick))
            {
                string mapperFolder = groupFolder + "/" + groupFolder;
                if (TryGetMapCoverByMapper(mapperFolder, trackname, size, out coverPath))
                {
                    return true;
                }
            }
            else
            {
                if (TryGetFirstMapCover(groupFolder, trackname, size, out coverPath))
                {
                    return true;
                }
            }

            coverPath = "";
            return false;
        }
        private bool TryFindFileInFolder(string[] mappersFolder, string trackname, out string foundImagePath)
        {
            foreach (string mapperFolder in mappersFolder)
            {
                string possibleCoverPath = mapperFolder + "/" + trackname;

                if (FileHelper.TryFindFile(possibleCoverPath, out foundImagePath, ".jpg", ".png"))
                {
                    return true;
                }
            }

            foundImagePath = "";
            return false;
        }

        private bool TryGetMapCoverByMapper(string mapperFolder, string trackname, ImageSize size, out string coverFilePath)
        {
            string preferredFilePath = mapperFolder + "/" + trackname + size.ToString();

            // Successfuly found preferred file
            if (FileHelper.TryFindFile(preferredFilePath, out string foundPreferredFilepath, ".jpg", ".png"))
            {
                coverFilePath = foundPreferredFilepath;
                return true;
            }

            // Checking for all sizes
            foreach (ImageSize anotherSize in Enum.GetValues(typeof(ImageSize)))
            {
                string anotherFilePath = mapperFolder + "/" + trackname + anotherSize.ToString();
                if (File.Exists(anotherFilePath))
                {
                    coverFilePath = anotherFilePath;
                    return true;
                }
            }

            // If no cover with any size in ImageSize
            coverFilePath = "";
            return false;
        }

        private bool TryGetFirstMapCover(string groupFolder, string trackname, ImageSize size, out string coverFilePath)
        {
            string[] mappersFolders = Directory.GetDirectories(groupFolder);
            foreach (string mapperFolder in mappersFolders)
            {
                if(TryGetMapCoverByMapper(mapperFolder, trackname, size, out coverFilePath))
                {
                    return true;
                }
            }

            coverFilePath = "";
            return false;
        }
    }
}
