using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using BeatSlayerServer.Controllers;
using Newtonsoft.Json;
using BeatSlayerServer.Utils;
using BeatSlayerServer.Services.Statistics;
using BeatSlayerServer.Models.Configuration;
using BeatSlayerServer.Services;
using Microsoft.Extensions.Hosting;
using BeatSlayerServer.Models.Configuration.Modules;

namespace BSServer.Controllers
{
    public class HomeController : Controller
    {
        private readonly HeartbeatService heartbeatService;
        private readonly ServerSettings settings;


        public HomeController(SettingsWrapper wrapper, HeartbeatService heartbeatService)
        {
            settings = wrapper.settings;
            this.heartbeatService = heartbeatService;
        }



        public IActionResult Test()
        {
            return Content("Да бля, это капуста");
        }


        public IActionResult Index()
        {
            HomeModel model = new HomeModel()
            {
                ServerType = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production" ? ServerType.Production : ServerType.Development
            };
            return View(model);
        }

        public IActionResult GetMapStatistics(int offset)
        {
            HomeModel model = new HomeModel();

            model.chartInfo = heartbeatService.GetChartMap(offset);

            return Content(JsonConvert.SerializeObject(model));
        }

        

        public IActionResult Download()
        {
            ViewBag.EditorApkPath = "Data/bseditor.apk";
            ViewBag.EditorVersion = BuildsController.GetEditorVersionStatic();
            return View();
        }

        #region Requesting project item

        // Ахуеть, это реально используется, капеееццц...
        // На сколько это старая херата то, а?..
        // На 21.06.2020 4:22 все ещё нужна (ссылка из игры: http://176.107.160.146/Home/DownloadProject)
        // На 21.06.2020 20:32 уже накой ненужна ))
        [Obsolete("Last used in Game 1.59.0. Use /Maps/Upload instead")]
        [HttpGet]
        public FileResult DownloadProject(string trackname, string nickname)
        {
            if (trackname == null || trackname == "") return null;

            trackname = trackname.Replace("%amp%", "&");
            nickname = nickname.Replace("%amp%", "&");

            string filepath = settings.TracksFolder + "/" + trackname + "/" + nickname + "/" + trackname + ".bsz";
            if (!System.IO.File.Exists(filepath)) return null;

            byte[] arr = System.IO.File.ReadAllBytes(filepath);
            return File(arr, System.Net.Mime.MediaTypeNames.Application.Octet, trackname + ".bsz");
        }

        #endregion


        #region Publish code (From editor app)

        // Бляяяя, а это тоже используется...
        // Или уже нееет... крч, вот ссылка из редактора http://176.107.160.146/Maps/UploadProject
        //[HttpPost]
        //public async Task<IActionResult> Upd(IFormFile arr, string nickname, string email, string audioTime)
        //{
        //    if (arr == null) return Content("[ERR] File is NULL");
        //    if (nickname == null || nickname == "") return Content("[ERR] Nickname is empty");
        //    if (email == null || !email.Contains("@")) return Content("[ERR] Email isn't correct");
        //    if (audioTime == null || audioTime == "") return Content("[ERR] Audio time is empty");

        //    //OperationResult onUpload = Publisher.OnUpload(arr, nickname, email);

        //    //if (onUpload.state == OperationResult.State.Success) return Content("success");
        //    //else return Content("[ERR] " + onUpload.message);

        //}

        #endregion



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}