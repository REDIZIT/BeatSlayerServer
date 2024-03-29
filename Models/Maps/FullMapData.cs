﻿using BeatSlayerServer.ProjectManagement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BeatSlayerServer.Models.Maps
{
    public class FullMapData : BasicMapData
    {
        [JsonIgnore] public int Downloads { get { return downloads; } }
        [JsonIgnore] public int PlayCount { get { return Difficulties.Sum(c => c.playCount); } }
        [JsonIgnore] public int Likes { get { return Difficulties.Sum(c => c.likes); } }
        [JsonIgnore] public int Dislikes { get { return Difficulties.Sum(c => c.dislikes); } }

        public int downloads;

        public List<DifficultyInfo> Difficulties { get; set; }

        public DateTime PublishTime { get; set; }
        //public DateTime ApprovedTime { get; set; }


        /// <summary>Used for converting mapinfo.xml to <see cref="BasicMapData"/></summary>
        public FullMapData(MapInfo projectMapInfo)
        {
            Author = projectMapInfo.group.author;
            Name = projectMapInfo.group.name;
            MapperNick = projectMapInfo.nick;
            IsApproved = projectMapInfo.approved;

            downloads = projectMapInfo.downloads;

            Difficulties = projectMapInfo.difficulties;
        }
    }
}
