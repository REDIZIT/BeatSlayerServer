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
        public static OperationResult SetDifficultyStatistics(string trackname, string nick, int difficultyId, DifficultyStatisticsKey key)
        {
            MapInfo info = ProjectManager.GetMapInfo(trackname, nick);

            DifficultyInfo d = null;
            if(key != DifficultyStatisticsKey.Download)
            {
                d = info.difficulties.Find(c => c.id == difficultyId);
            }

            switch (key)
            {
                case DifficultyStatisticsKey.Download:
                    info.downloads++; break;
                case DifficultyStatisticsKey.Play:
                    d.playCount++; break;
                case DifficultyStatisticsKey.Launch:
                    d.launches++; break;
                case DifficultyStatisticsKey.Like:
                    d.likes++; break;
                case DifficultyStatisticsKey.Dislike:
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
    }

    public enum DifficultyStatisticsKey
    {
        Download, Launch, Play, Like, Dislike
    }
}
