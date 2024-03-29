﻿using BeatSlayerServer.API;
using BeatSlayerServer.Enums;
using BeatSlayerServer.Models.Configuration;
using BeatSlayerServer.Models.Maps;
using BeatSlayerServer.ProjectManagement;
using BeatSlayerServer.Services;
using BeatSlayerServer.Services.MapsManagement;
using BeatSlayerServer.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeatSlayerServer.Controllers.Wrappers
{
    public class DatabaseController : Controller
    {
        private readonly MyDbContext ctx;
        private readonly ServerSettings settings;
        private readonly MapsService mapsService;

        private readonly ILogger<DatabaseController> logger;

        public DatabaseController(MyDbContext ctx, ILogger<DatabaseController> logger, SettingsWrapper wrapper, MapsService mapsService)
        {
            this.ctx = ctx;
            this.logger = logger;
            settings = wrapper.settings;
            this.mapsService = mapsService;
        }



        public IActionResult Index () { return Content(""); }

        public IActionResult GetMessage()
        {
            return Content(DatabaseAPI.GetMessage());
        }


        public IActionResult GetGroup(string trackname)
        {
            trackname = UrlHelper.Decode(trackname);

            MapsData group = mapsService.GetGroup(trackname);
            return Content(JsonConvert.SerializeObject(group));
        }
        public IActionResult GetGroupsExtended(bool intended = false)
        {
            List<MapsData> ls = mapsService.GetGroupsExtended();

            //MapsData tutorialMap = ls.FirstOrDefault(c => c.Author + "-" + c.Name == settings.TutorialTrackname);
            //if(tutorialMap != null)
            //{
            //    tutorialMap.MapType = GroupType.Tutorial;
            //}

            return Content(JsonConvert.SerializeObject(ls, intended ? Formatting.Indented : Formatting.None));
        }
        public IActionResult GetGroupsExtendedFromDb()
        {
            List<MapsData> ls = mapsService.GetGroupsExtended(true);
            return Content(JsonConvert.SerializeObject(ls, Formatting.Indented));
        }
        public IActionResult GetTutorialGroup()
        {
            MapsData group = mapsService.GetGroup(settings.TutorialTrackname);
            return Content(JsonConvert.SerializeObject(group));
        }



        public IActionResult GetMaps(string trackname)
        {
            string _trackname = trackname.Replace("%amp%", "&");

            var ls = mapsService.GetMaps(_trackname);
          
            return Content(JsonConvert.SerializeObject(ls));
        }
        public IActionResult GetMapsWithResult(string trackname)
        {
            trackname = UrlHelper.Decode(trackname);

            OperationResult op = mapsService.GetMapsWithResult(trackname);

            if (op.exception == null)
            {
                return Content(JsonConvert.SerializeObject(op.obj));
            }
            else
            {
                return Content("[ERR] " + op.exception.Message);
            }
        }
        public IActionResult GetMap(string trackname, string nick)
        {
            trackname = trackname.Replace("%amp%", "&");
            nick = nick.Replace("%amp%", "&");

            FullMapData map = mapsService.GetMap(trackname, nick);
            return Content(JsonConvert.SerializeObject(map));
        }



        public IActionResult DoesMapExist(string trackname, string nick)
        {
            trackname = UrlHelper.Decode(trackname);
            return Content(mapsService.DoesMapExist(trackname, nick).ToString());
        }


        

        public IActionResult HasUpdateForMap(string trackname, string nick, long utcTicks)
        {
            trackname = trackname.Replace("%amp%", "&");
            nick = nick.Replace("%amp%", "&");

            return Content(mapsService.HasUpdateForMap(trackname, nick, utcTicks).ToString());
        }

        public IActionResult GetCover(string trackname = "", string nick = "")
        {
            if (nick == null) nick = "";

            trackname = UrlHelper.Decode(trackname);
            nick = UrlHelper.Decode(nick);

            byte[] bytes = mapsService.GetCover(trackname, nick, ImageSize._512x512);

            return File(bytes, System.Net.Mime.MediaTypeNames.Application.Octet, "cover.jpg");
        }
        public IActionResult GetCoverAsPicture(string trackname = "", string nick = "")
        {
            trackname = UrlHelper.Decode(trackname);
            nick = UrlHelper.Decode(nick);

            if (nick == null) nick = "";

            byte[] bytes = mapsService.GetCover(trackname, nick, ImageSize._512x512);

            return File(bytes, System.Net.Mime.MediaTypeNames.Image.Jpeg, "cover");
        }



        public IActionResult SetStatistics(string trackname, string nick, string key, int value)
        {
            trackname = trackname.Replace("%amp%", "&");
            nick = nick.Replace("%amp%", "&");

            OperationResult op = DatabaseAPI.SetStatistics(trackname, nick, key, value);

            if (op.state == OperationResult.State.Fail)
            {
                return Content("[ERR] " + op.message);
            }
            else
            {
                return Content("Success");
            }
        }
        public async Task<IActionResult> SetDifficultyStatistics(string trackname, string nick, int difficultyId, DifficultyStatisticsKey key)
        {
            trackname = trackname.Replace("%amp%", "&");
            string author = trackname.Split('-')[0];
            string name = trackname.Split('-')[1];
            nick = nick.Replace("%amp%", "&");



            MapInfo map = ProjectManager.GetMapInfo(trackname, nick);
            var diff = map.difficulties.FirstOrDefault(c => c.id == difficultyId);
            int stars = -1;
            if (diff != null)
            {
                stars = map.difficulties.FirstOrDefault(c => c.id == difficultyId).stars;
            }

            logger.LogInformation("[{action}] {trackname} by {nick} ({difficultyStars} with id {difficultyId})", key.ToString().ToUpper() + " MAP", trackname, nick, stars, difficultyId);


            OperationResult op = DatabaseAPI.SetDifficultyStatistics(trackname, nick, difficultyId, key);


            // Update published maps statistics
            var mapperAcc = ctx.Players.FirstOrDefault(c => c.Nick == nick);
            switch (key)
            {
                case DifficultyStatisticsKey.Play:
                    mapperAcc.PublishedMapsPlayed++; break;
                case DifficultyStatisticsKey.Like:
                    mapperAcc.PublishedMapsLiked++; break;
            }
            await ctx.SaveChangesAsync();



            if (op.state == OperationResult.State.Fail)
            {
                return Content("[ERR] " + op.message);
            }
            else
            {
                return Content("Success");
            }
        }
    }
}
