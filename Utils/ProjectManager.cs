using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace BeatSlayerServer.ProjectManagement
{
    public static class ProjectManager
    {
        public static string MapsFolder => Payload.TracksFolder;


        #region Project

        public static Project LoadProject(string path)
        {
            XmlSerializer xml = new XmlSerializer(typeof(Project));
            using (Stream s = new FileStream(path, FileMode.Open))
            {
                Project project = (Project)xml.Deserialize(s);
                return project;
            }
        }
        public static void SaveProject(Project proj, string path)
        {
            XmlSerializer xml = new XmlSerializer(typeof(Project));
            using (Stream s = new FileStream(path, FileMode.Create))
            {
                xml.Serialize(s, proj);
            }
        }
        public static void UnpackProject(Project proj, string folder)
        {
            string trackname = proj.author + "-" + proj.name;
            string audioPath = folder + "/" + trackname + Project.ToString(proj.audioExtension);
            string coverExtension = folder + "/" + trackname + Project.ToString(proj.imageExtension);


            File.WriteAllBytes(audioPath, proj.audioFile);

            if (proj.hasImage) File.WriteAllBytes(coverExtension, proj.image);
        }

        public static string GetTime(int mins, int secs)
        {
            return mins + ":" + (secs >= 10 ? secs.ToString() : "0" + 0);
        }
        #endregion

        #region Map Info

        #region Set
        public static void SetMapInfo(string filepath, MapInfo info)
        {
            XmlSerializer xml = new XmlSerializer(typeof(MapInfo));
            using Stream s = new FileStream(filepath, FileMode.Create);
            xml.Serialize(s, info);
        }
        public static void SetMapInfo(string trackname, string nick, MapInfo info)
        {
            SetMapInfo(MapsFolder + "/" + trackname + "/" + nick + "/" + "mapinfo.xml", info);
        }
        public static void SetMapInfo(MapInfo map)
        {
            SetMapInfo(MapsFolder + "/" + map.group.author + "-" + map.group.name + "/" + map.nick+ "/" + "mapinfo.xml", map);
        }
        #endregion

        #region Get
        public static MapInfo GetMapInfo(string path, bool isFolder = false)
        {
            string filepath = isFolder ? path + "/mapinfo.xml" : path;
            if (!File.Exists(filepath)) return null;

            XmlSerializer xml = new XmlSerializer(typeof(MapInfo));
            using (Stream s = File.OpenRead(filepath))
            {
                return (MapInfo)xml.Deserialize(s);
            }
        }
        public static MapInfo GetMapInfo(string trackname, string nick)
        {
            return GetMapInfo(MapsFolder + "/" + trackname + "/" + nick + "/mapinfo.xml");
        }
        #endregion

        #endregion

        #region GroupInfo

        #region Set
        public static void SetGroupInfo(string filepath, GroupInfo info)
        {
            XmlSerializer xml = new XmlSerializer(typeof(GroupInfo));
            using Stream s = new FileStream(filepath, FileMode.Create);
            xml.Serialize(s, info);
        }
        public static void SetGroupInfo(GroupInfo info)
        {
            string trackname = info.author + "-" + info.name;
            SetGroupInfo(MapsFolder + "/" + trackname + "/" + "groupinfo.xml", info);
        }
        #endregion

        #region Get

        public static List<GroupInfo> GetGroupInfosByPlayer(string nick)
        {
            string[] folders = Directory.GetDirectories(MapsFolder);
            List<GroupInfo> groups = new List<GroupInfo>();

            foreach (var folder in folders)
            {
                string trackname = new DirectoryInfo(folder).Name;
                foreach (var mapperFolder in Directory.GetDirectories(folder))
                {
                    var info = new DirectoryInfo(mapperFolder).Name;
                    if(info == nick)
                    {
                        groups.Add(GetGroupInfo(trackname));
                    }
                }
            }

            return groups;
        }
        public static GroupInfo GetGroupInfo(string path, bool isFolder = false)
        {
            string filepath = isFolder ? path + "/groupinfo.xml" : path;

            XmlSerializer xml = new XmlSerializer(typeof(GroupInfo));
            using Stream s = File.OpenRead(filepath);
            return (GroupInfo)xml.Deserialize(s);
        }
        public static GroupInfo GetGroupInfo(string trackname)
        {
            return GetGroupInfo(MapsFolder + "/" + trackname + "/groupinfo.xml");
        }
        #endregion

        #endregion

        public static void CheckInfoFiles(Project proj)
        {
            string trackname = proj.author + "-" + proj.name;
            string author = proj.author;
            string name = proj.name;

            // Check group info file
            string[] mapsFolders = Directory.GetDirectories(MapsFolder + "/" + trackname);
            GroupInfo groupInfo;
            groupInfo = new GroupInfo()
            {
                author = author,
                name = name,
                mapsCount = mapsFolders.Length
            };
            SetGroupInfo(groupInfo);



            // Check map info file
            MapInfo mapInfo;
            string mapInfoPath = MapsFolder +  "/" + trackname + "/" + proj.creatorNick + "/mapinfo.xml";

            if (!File.Exists(mapInfoPath))
            {
                mapInfo = new MapInfo(groupInfo)
                {
                    nick = proj.creatorNick,
                    difficultyName = proj.difficultName,
                    difficultyStars = proj.difficultStars,
                    publishTime = new FileInfo(MapsFolder + "/" + trackname + "/" + proj.creatorNick + "/" + trackname + ".bsz").LastWriteTimeUtc
                };

                mapInfo.difficulties = new List<DifficultyInfo>();
                if(proj.difficulties == null || proj.difficulties.Count == 0)
                {
                    DifficultyInfo dInfo = new DifficultyInfo()
                    {
                        name = proj.difficultName,
                        stars = proj.difficultStars,
                        id = 0
                    };
                    mapInfo.difficulties.Add(dInfo);
                }
                else
                {
                    foreach (Difficulty difficulty in proj.difficulties)
                    {
                        DifficultyInfo dInfo = new DifficultyInfo()
                        {
                            name = difficulty.name,
                            stars = difficulty.stars,
                            id = difficulty.id
                        };
                        mapInfo.difficulties.Add(dInfo);
                    }
                }
            }
            else
            {
                mapInfo = GetMapInfo(trackname, proj.creatorNick);

                mapInfo.difficultyName = proj.difficultName;
                mapInfo.difficultyStars = proj.difficultStars;
                mapInfo.publishTime = new FileInfo(MapsFolder + "/" + trackname + "/" + proj.creatorNick + "/" + trackname + ".bsz").LastWriteTimeUtc;

                // --- Обновляем сложности -----------

                List<DifficultyInfo> result = new List<DifficultyInfo>();

                foreach (Difficulty difficulty in proj.difficulties)
                {
                    DifficultyInfo d = new DifficultyInfo()
                    {
                        name = difficulty.name,
                        stars = difficulty.stars,
                        id = difficulty.id
                    };

                    // Copy stats
                    DifficultyInfo b = mapInfo.difficulties.Find(c => c.id == d.id);
                    if (b != null)
                    {
                        d.likes = b.likes;
                    }

                    result.Add(d);
                }

                mapInfo.difficulties = result;
            }

            SetMapInfo(trackname, proj.creatorNick, mapInfo);
        }
    }

    public class GroupInfo
    {
        public string author, name;
        public int mapsCount;
    }
    public class GroupInfoExtended
    {
        public string author, name;
        public int mapsCount;

        public int allDownloads, allPlays, allLikes, allDislikes;
        public DateTime updateTime;

        public List<string> nicks;
    }
    public class MapInfo
    {
        public GroupInfo group;

        //public string author { get { return group.author; } }
        //public string name { get { return group.name; } }

        public string nick;

        /// <summary>
        /// Deprecated. Use Likes, Dislikes, PlayCount and downloads (not deprecated)
        /// </summary>
        public int likes, dislikes, playCount, downloads;

        public int Downloads { get { return difficulties.Sum(c => c.downloads); } }
        public int PlayCount { get { return difficulties.Sum(c => c.playCount); } }
        public int Likes { get { return difficulties.Sum(c => c.likes); } }
        public int Dislikes { get { return difficulties.Sum(c => c.dislikes); } }

        /// <summary>
        /// Deprecated. Use difficulties
        /// </summary>
        public string difficultyName;
        /// <summary>
        /// Deprecated. Use difficulties
        /// </summary>
        public int difficultyStars;

        public List<DifficultyInfo> difficulties;
        

        public DateTime publishTime;

        public bool approved;
        public DateTime grantedTime;

        public bool IsGrantedNow
        {
            get
            {
                if (!approved) return false;
                else return grantedTime > publishTime;
            }
        }

        public MapInfo() { }
        public MapInfo(GroupInfo group)
        {
            this.group = group;
        }
    }

    public class DifficultyInfo
    {
        public string name;
        public int stars;
        public int id = -1;

        public int downloads, playCount, likes, dislikes;
    }
}
