using BeatSlayerServer.Extensions;
using BeatSlayerServer.Models.Configuration;
using BeatSlayerServer.Models.Database;
using BeatSlayerServer.ProjectManagement;
using BeatSlayerServer.Services;
using BeatSlayerServer.Services.MapsManagement;
using BeatSlayerServer.Services.Messaging;
using BeatSlayerServer.Utils;
using BeatSlayerServer.Utils.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using GroupInfo = BeatSlayerServer.Utils.Database.GroupInfo;

namespace BeatSlayerServer.Controllers.Messagers
{
    //[Authorize(Roles = "Developer")]
    public class MessagersController : Controller
    {
        private readonly MyDbContext ctx;
        private readonly ServerSettings settings;
        private readonly BotService botService;
        private readonly MapsService mapsService;

        public MessagersController(MyDbContext ctx, SettingsWrapper wrapper, BotService botService, MapsService mapsService)
        {
            this.ctx = ctx;
            settings = wrapper.settings;
            this.botService = botService;
            this.mapsService = mapsService;
        }

        public async Task<IActionResult> Status(string masterkey)
        {
            if (masterkey != "sosipisun") return Content("Invalid masterkey");

            StringBuilder builder = new StringBuilder();

            builder.Append("Discord\n");
            builder.Append("Alive? " + await botService.CheckAlive("Discord") + "\n");
            builder.Append("Enabled? " + await botService.CheckEnabled("Discord") + "\n");

            builder.Append("\n");

            builder.Append("vk\n");
            builder.Append("Alive? " + await botService.CheckAlive("Vk") + "\n");
            builder.Append("Enabled? " + await botService.CheckEnabled("Vk") + "\n");

            builder.Append("\n");

            builder.Append("Email\n");
            builder.Append("Alive? " + await botService.CheckAlive("Email") + "\n");
            builder.Append("Enabled? " + await botService.CheckEnabled("Email") + "\n");

            return Content(builder.ToString());
        }

        public async Task<IActionResult> KillDiscord(string masterkey)
        {
            if (masterkey != "sosipisun") return Content("Invalid masterkey");

            await botService.Kill("Discord");
            return Content("Killed");
        }
        public async Task<IActionResult> BuildDiscord(string masterkey)
        {
            if (masterkey != "sosipisun") return Content("Invalid masterkey");

            await botService.Build("Discord");
            return Content("Builded");
        }


        public async Task<IActionResult> KillEmail(string masterkey)
        {
            if (masterkey != "sosipisun") return Content("Invalid masterkey");

            await botService.Kill("Email");
            return Content("Killed");
        }
        public async Task<IActionResult> BuildEmail(string masterkey)
        {
            if (masterkey != "sosipisun") return Content("Invalid masterkey");

            await botService.Build("Email");
            return Content("Builded");
        }


        public async Task<string> GetRandomMap()
        {
            return await mapsService.GetRandomGroup();
        }

        public string GetPublishedMaps(string nick)
        {
            if (string.IsNullOrWhiteSpace(nick)) return null;

            List<string> result = new List<string>();

            var maps = ctx.Groups.Where(c => c.Maps.Any(c => c.Nick == nick)).Select(m => new { Author = m.Author, Name = m.Name });
            foreach (var map in maps)
            {
                result.Add(map.Author + " - " + map.Name);
            }

            return JsonConvert.SerializeObject(result);
        }

        public int GetMapsCount(bool isDb)
        {
            if (isDb)
            {
                return ctx.Groups.Count();
            }
            else
            {
                return mapsService.GetGroupsExtended().Count;
            }
        }

        public string GetMapInfo(string trackname, string mapper)
        {
            bool hasFolder = false;
            bool hasDb = false;

            string author = trackname.Split('-')[0];
            string name = trackname.Split('-')[1];

            string mapFolder = settings.TracksFolder + "/" + trackname + "/" + mapper;
            hasFolder = Directory.Exists(mapFolder);

            hasDb = ctx.Groups.Any(c => c.Author == author && c.Name == name && c.Maps.Any(m => m.Nick == mapper));

            StringBuilder b = new StringBuilder();

            if (!hasFolder) b.AppendLine("Does not exists");
            if (!hasDb) b.AppendLine("Does not exist in database");


            try
            {
                if (hasFolder)
                {
                    Project proj = ProjectManager.LoadProject(mapFolder + "/" + trackname + ".bsz");

                    if (proj.difficulties?.Count == 0)
                    {
                        b.AppendLine("Difficulty (integrated)");
                        b.AppendLine("Name: " + proj.difficultName);
                        b.AppendLine("Stars: " + proj.difficultStars);
                    }
                    else
                    {
                        b.AppendLine("Difficulties count is " + proj.difficulties.Count);
                        foreach (var diff in proj.difficulties)
                        {
                            b.AppendLine($"'{diff.name}' {diff.stars}* {diff.speed}x  blocks-count: {diff.beatCubeList.Count}");
                        }
                        b.AppendLine();
                    }
                }
            }
            catch(Exception err)
            {
                Console.WriteLine("[ERROR] " + err.Message);
                b.AppendLine("[error]");
            }
            

            return b.ToString();
        }

        public string GetPreferredDifficulty(string nick)
        {
            var replays = ctx.Replays.Where(c => c.Player.Nick == nick).Select(c => new { stars = c.DifficultyStars == 0 ? 4 : c.DifficultyStars, rp = c.RP });

            //replays = replays.OrderByDescending(c => c.stars * c.stars * 10 + c.rp / 10f).Take(2);


            //var result = replays.Count() == 0 ? 0 : replays.Sum(c => c.stars) / (float)replays.Count();
            string result = "";
            foreach (var item in replays.Select(c => c.stars))
            {
                result += item + " ";
            }
            return result;
        }
    }
}
