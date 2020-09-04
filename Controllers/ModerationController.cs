using BeatSlayerServer.Enums;
using BeatSlayerServer.Models.Moderation;
using BeatSlayerServer.ProjectManagement;
using BeatSlayerServer.Services.MapsManagement;
using BeatSlayerServer.Services.Messaging;
using BeatSlayerServer.Utils;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BeatSlayerServer.Controllers.Wrappers
{
    public class ModerationController : Controller
    {
        private readonly ModerationService moderationService;
        private readonly BotService botService;
        private readonly MyDbContext ctx;

        public ModerationController(MyDbContext ctx, ModerationService moderationService, BotService botService)
        {
            this.ctx = ctx;
            this.moderationService = moderationService;
            this.botService = botService;
        }


        public IActionResult GetOperations()
        {
            List<ModerateOperation> ls = moderationService.GetModerationMaps();
            string json = JsonConvert.SerializeObject(ls);
            return Content(json);
        }
        public async Task<IActionResult> CreateRequest(string trackname, string nick)
        {
            trackname = WebUtility.UrlDecode(trackname);
            nick = WebUtility.UrlDecode(nick);

            OperationResult result = await moderationService.SendModerationRequest(trackname, nick);

            if (result.state == OperationResult.State.Success) return Content("Success");
            else return Content("[ERR] " + result.message);
        }

        public async Task<IActionResult> SendResponse(string opJson)
        {
            Console.WriteLine(opJson);

            ModerateOperation op = JsonConvert.DeserializeObject<ModerateOperation>(opJson);

            await moderationService.SendModerationResponse(op);

            return Content("Success");
        }
        public IActionResult DownloadMap(string trackname, string nick)
        {
            trackname = WebUtility.UrlDecode(trackname);
            nick = WebUtility.UrlDecode(nick);

            byte[] map = moderationService.GetModerationMap(trackname, nick);
            if (map == null) return Content("");
            return File(map, System.Net.Mime.MediaTypeNames.Application.Octet, trackname + ".bsz");
        }


        public async Task<IActionResult> ModeratorCheat(string nick, string trackname)
        {
            trackname = WebUtility.UrlDecode(trackname);
            nick = WebUtility.UrlDecode(nick);

            await botService.ModeratorCheat(trackname, nick);

            return Content("Success");
        }

        public string GetApprovedGroups(bool isDb = false)
        {
            List<GroupInfo> groupInfos = moderationService.GetApprovedGroups();
            string json = JsonConvert.SerializeObject(groupInfos);
            return json;
        }
        //public IActionResult ClearMaps(string masterkey)
        //{
        //    if (masterkey != "sosipisun") return Content("Invalid masterkey");

        //    moderationService.ClearApprovedMaps();
        //    return Content("Cleared");
        //}
        public IActionResult ReloadData()
        {
            moderationService.LoadData();
            return Content("Reloaded");
        }
    }
}
