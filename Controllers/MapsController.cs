using BeatSlayerServer.Enums;
using BeatSlayerServer.Models.Configuration;
using BeatSlayerServer.ProjectManagement;
using BeatSlayerServer.Services;
using BeatSlayerServer.Services.MapsManagement;
using BeatSlayerServer.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace BeatSlayerServer.Controllers.Wrappers
{
    public class MapsController : Controller
    {
        private readonly ServerSettings settings;
        private readonly MapsService mapsService;
        private readonly PublishService publishService;

        private readonly ILogger<MapsController> logger;
        private readonly MyDbContext ctx;


        public MapsController(ILogger<MapsController> logger, MyDbContext ctx, SettingsWrapper wrapper, MapsService mapsService, PublishService publishService)
        {
            this.ctx = ctx;
            this.logger = logger;

            settings = wrapper.settings;
            this.mapsService = mapsService;
            this.publishService = publishService;
        }


        [HttpPost]
        [RequestSizeLimit(64 * 1024 * 1024)]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            OperationResult op = await publishService.PublishProject(file);

            //logger.LogError("[PUBLISH MAP] {result}", op);

            return Content(JsonConvert.SerializeObject(op));
        }

        public IActionResult Download(string trackname, string nick)
        {
            if (trackname == null || trackname == "") return null;

            trackname = trackname.Replace("%amp%", "&");
            nick = nick.Replace("%amp%", "&");


            string filepath = settings.TracksFolder + "/" + trackname + "/" + nick + "/" + trackname + ".bsz";
            if (!System.IO.File.Exists(filepath))
            {
                logger.LogError("[DOWNLOAD MAP] {trackname} by {nick} does not exist", trackname, nick);
                return null;
            }

            logger.LogInformation("[DOWNLOAD MAP] {trackname} by {nick}", trackname, nick);

            // Set statistics
            var mapInfo = ProjectManager.GetMapInfo(trackname, nick);
            mapInfo.downloads++;
            ProjectManager.SetMapInfo(mapInfo);
            // Set db statistics
            //foreach (var diff in ctx.Groups.First(c => c.Author == author && c.Name == name).Maps.First(c => c.Nick == nick).Difficulties)
            //{
            //    diff.Downloads++;
            //}
            //ctx.SaveChanges();
            

            byte[] arr = System.IO.File.ReadAllBytes(filepath);
            return File(arr, System.Net.Mime.MediaTypeNames.Application.Octet, trackname + ".bsz");
        }





        public IActionResult DeleteProject(string trackname, string nick, string password = "", string masterkey = "")
        {
            trackname = UrlHelper.Decode(trackname);
            nick = UrlHelper.Decode(nick);

            var op = mapsService.DeleteProject(trackname, nick, password, masterkey);
            logger.LogInformation("[DELETE PROJECT] {nick} removed {trackname}", nick, trackname);

            return Content(op.Type == Utils.OperationType.Success ? "Success" : "[ERR] " + op.Message);
        }
        public IActionResult RenameProject(string trackname, string nick, string newtrackname, string masterkey = "")
        {
            var op = mapsService.RenameProject(trackname, nick, newtrackname, masterkey);
            logger.LogInformation("[RENAME PROJECT] Project by {nick} was renamed from {trackname} to {new trackname}", nick, trackname, newtrackname);

            return Content(op.Type == Utils.OperationType.Success ? "Success" : "[ERR] " + op.Message);
        }
        public IActionResult RemoveMapFromApproved(string trackname, string nick, string masterkey = "")
        {
            trackname = UrlHelper.Decode(trackname);
            nick = UrlHelper.Decode(nick);

            var op = mapsService.RemoveMapFromApproved(trackname, nick, masterkey);
            logger.LogInformation("[UNAPPROVE MAP] {trackname} by {nick} was unapproved", trackname, nick);

            return Content(op.Type == Utils.OperationType.Success ? "Success" : "[ERR] " + op.Message);
        }


        public IActionResult GetCoverPicture(string trackname, string mapper)
        {
            byte[] cover = mapsService.GetCover(trackname, mapper, ImageSize._128x128);

            return File(cover, "image/jpeg");
        }

        public string CreateCovers(string trackname, string mapper)
        {
            Stopwatch w = Stopwatch.StartNew();

            mapsService.CreateCovers(trackname, mapper);

            return $"Done in {w.ElapsedMilliseconds}ms";
        }
    }
}
