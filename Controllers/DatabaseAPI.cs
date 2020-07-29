using BeatSlayerServer.Controllers;
using BeatSlayerServer.Enums;
using BeatSlayerServer.ProjectManagement;
using BeatSlayerServer.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BeatSlayerServer.API
{
    public static class DatabaseAPI
    {
        public static string GetMessage()
        {
            string file = File.ReadAllText("Data/InfoTable.json");
            InfoTable table = JsonConvert.DeserializeObject<InfoTable>(file);

            return table.GameMessage;
        }




        // Deprecated. Use GetGroupsExtended()
        public static List<MapInfo> GetGroups()
        {
            string[] tracksFolders = Directory.GetDirectories("Tracks").OrderByDescending(c => new DirectoryInfo(c).CreationTime).ToArray();
            List<MapInfo> mapInfos = new List<MapInfo>();

            for (int i = 0; i < tracksFolders.Length; i++)
            {
                string trackname = new DirectoryInfo(tracksFolders[i]).Name;

                string[] mapsFolders = Directory.GetDirectories(tracksFolders[i]);

                GroupInfo groupInfo = new GroupInfo()
                {
                    author = trackname.Split('-')[0],
                    name = trackname.Split('-')[1],
                    mapsCount = mapsFolders.Length
                };

                mapInfos.Add(new MapInfo()
                {
                    group = groupInfo
                });
            }

            return mapInfos;
        }


        


        public static OperationResult SetStatistics(string trackname, string nick, string key, int value)
        {
            trackname = UrlHelper.Decode(trackname);
            nick = UrlHelper.Decode(nick);

            MapInfo info = ProjectManager.GetMapInfo(trackname, nick);

            if (key != "download" && key != "play" && key != "like" && key != "dislike") return new OperationResult(OperationResult.State.Fail, "Key is invalid");
            if (Math.Abs(value) > 1) return new OperationResult(OperationResult.State.Fail, "Че, самый умный что ли?");

            switch (key)
            {
                case "download":
                    info.downloads += value; break;
                case "play":
                    info.playCount += value; break;
                case "like":
                    info.likes += value; break;
                case "dislike":
                    info.dislikes += value; break;
                default:
                    break;
            }


            ProjectManager.SetMapInfo(trackname, nick, info);
            return new OperationResult(OperationResult.State.Success);
        }
        public static OperationResult SetDifficultyStatistics(string trackname, string nick, int difficultyId, string key)
        {
            MapInfo info = ProjectManager.GetMapInfo(trackname, nick);

            if (key != "download" && key != "play" && key != "like" && key != "dislike") return new OperationResult(OperationResult.State.Fail, "Key is invalid");

            DifficultyInfo d = null;
            if(key != "download")
            {
                d = info.difficulties.Find(c => c.id == difficultyId);
            }

            switch (key)
            {
                case "download":
                    info.downloads++; break;
                case "play":
                    d.playCount++; break;
                case "like":
                    d.likes++; break;
                case "dislike":
                    d.dislikes++; break;
            }

            ProjectManager.SetMapInfo(trackname, nick, info);


            return new OperationResult(OperationResult.State.Success);
        }

        public static MapInfo GetShortStatistics(string trackname, string nick)
        {
            trackname = UrlHelper.Decode(trackname);
            nick = UrlHelper.Decode(nick);

            return ProjectManager.GetMapInfo(trackname, nick);
        }



        //public static string UpgradePrelisten(bool accept = false)
        //{
        //    string log = "Upgrade map infos accept = " + accept;

        //    string tracksFolder = Payload.TracksFolder;
        //    string[] groupFolders = Directory.GetDirectories(tracksFolder);
        //    foreach (string groupFolder in groupFolders)
        //    {
        //        string trackname = new DirectoryInfo(groupFolder).Name;

        //        string[] nicksFolders = Directory.GetDirectories(groupFolder);
        //        foreach (string nickFolder in nicksFolders)
        //        {
        //            string nick = new DirectoryInfo(nickFolder).Name;

        //            log += "\n" + trackname + "  |  " + nick;


        //            // Upgrade code

        //            // To delete
        //            string filepathes = nickFolder + "/" + trackname;
        //            string mp3FilePath = filepathes + ".mp3";
        //            string mp3FullFilePath = filepathes + ".full.mp3";
        //            string oggFilePath = filepathes + ".ogg";
        //            string logFilePath = filepathes + ".log";
        //            string bszFilePath = filepathes + ".bsz";

        //            if (File.Exists(mp3FullFilePath)) log += "\n >>>> Mp3 full";
        //            if (File.Exists(mp3FilePath)) log += "\n >>>> Mp3 cut";
        //            if (File.Exists(oggFilePath)) log += "\n >>>> Ogg cut";


        //            if (!accept) continue;

        //            File.Delete(mp3FilePath);
        //            File.Delete(oggFilePath);
        //            File.Delete(logFilePath);



        //            Project proj = null;
        //            try
        //            {
        //                proj = ProjectManager.LoadProject(bszFilePath);
        //            }
        //            catch(Exception err)
        //            {
        //                log += " !!!! ERROR: " + err.Message;
        //                continue;
        //            }



        //            // Cutting audio
        //            if (mp3FullFilePath.Contains(".mp3"))
        //            {
        //                File.WriteAllBytes(mp3FullFilePath, proj.audioFile);

        //                AudioCutter cutter = new AudioCutter();

        //                cutter.LoadFile(mp3FullFilePath);
        //                byte[] result = cutter.CutAudioFile(proj.mins * 60 + proj.secs, 25, 10);

        //                File.WriteAllBytes(mp3FilePath, result);

        //                File.Delete(mp3FullFilePath);

        //                log += "\n --- Done";
        //            }
        //            else
        //            {
        //                log += "\n --- Ignored";
        //            }
        //        }
        //    }

        //    return log;
        //}
    }
}
