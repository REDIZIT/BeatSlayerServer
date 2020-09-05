using System;
using System.Net;
using BeatSlayerServer.Models.Builds;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace BeatSlayerServer.Controllers
{
    public class BuildsController : Controller
    {
        public IActionResult Index()
        {
            BuildsViewModel model = new BuildsViewModel()
            {
                EditorVersion = GetEditorVersionStatic()
            };
            return View(model);
        }

        public IActionResult GetGameVersion()
        {
            string infoTablePath = "/srv/prod/Data/InfoTable.json";
            InfoTable table;
            if (System.IO.File.Exists(infoTablePath))
            {
                table = JsonConvert.DeserializeObject<InfoTable>(System.IO.File.ReadAllText(infoTablePath));
            }
            else
            {
                return Content("1.0.0");
            }


            DateTime t1 = DateTime.Now;
            TimeSpan deltaTime = t1.Subtract(table.LastGameVersionCheck);

            if (deltaTime >= new TimeSpan(0,0,10))
            {
                string page = new WebClient().DownloadString("https://play.google.com/store/apps/details?id=com.REDIZIT.BeatSlayer&hl=ru");


                string pattern = "Текущая версия</div><span class=\"htlgb\"><div class=\"IQ1z0d\"><span class=\"htlgb\">";
                int index = page.IndexOf(pattern);

                string result = page.Substring(index + pattern.Length, 20);
                result = result.Substring(0, result.IndexOf('<'));

                table.GameVersion = result;
                table.LastGameVersionCheck = DateTime.Now;
                System.IO.File.WriteAllText("/srv/prod/Data/InfoTable.json", JsonConvert.SerializeObject(table, Formatting.Indented));

                return Content(result);
            }
            else
            {
                return Content(table.GameVersion);
            }
        }

        public IActionResult GetEditorVersion()
        {
            return Content(GetEditorVersionStatic());
        }

        public static string GetEditorVersionStatic()
        {
            if (!System.IO.File.Exists("/srv/prod/Data/InfoTable.json")) return "-1";

            string file = System.IO.File.ReadAllText("/srv/prod/Data/InfoTable.json");
            InfoTable table = JsonConvert.DeserializeObject<InfoTable>(file);

            return table.EditorVersion;
        }


        public IActionResult DownloadEditorApk()
        {
            return File(System.IO.File.ReadAllBytes("Data/bseditor.apk"), System.Net.Mime.MediaTypeNames.Application.Octet, "bseditor.apk");
        }
        public IActionResult DownloadGameApk()
        {
            return File(System.IO.File.ReadAllBytes("Data/BeatSlayer.apk"), System.Net.Mime.MediaTypeNames.Application.Octet, "BeatSlayer.apk");
        }
    }
    
    public class InfoTable
    {
        public string EditorVersion;
        public string GameVersion;
        public DateTime LastGameVersionCheck;

        public string GameMessage;
    }
}