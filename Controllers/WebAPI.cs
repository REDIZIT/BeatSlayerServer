using BeatSlayerServer.Services.Game;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net.Mime;
using BeatSlayerServer.Utils;

namespace BeatSlayerServer.Controllers
{
    public class WebAPI : Controller
    {
        private readonly AccountService accountService;

        public WebAPI(AccountService accountService)
        {
            this.accountService = accountService;
        }


        [HttpPost]
        public IActionResult UploadAvatar(string nick, IFormFile file)
        {
            int stage = 0;
            try
            {
                Console.WriteLine("Upload avatar for " + nick);

                string extension = Path.GetExtension(file.FileName);

                stage = 1;
                string tmpPath = "UploadData/" + nick + extension;
                using (var stream = System.IO.File.Create(tmpPath))
                {
                    stage = 2;
                    file.CopyTo(stream);
                    stage = 3;
                }
                stage = 4;
                byte[] bytes = System.IO.File.ReadAllBytes(tmpPath);
                stage = 5;
                System.IO.File.Delete(tmpPath);
                stage = 6;


                OperationMessage msg = accountService.ChangeAvatar(nick, bytes, extension);
                stage = 7;
                return Content(JsonConvert.SerializeObject(msg));
            }
            catch (Exception err)
            {
                return Content(JsonConvert.SerializeObject(new OperationMessage(OperationType.Warning, "End with stage " + stage + " and error " + err.Message)));
            }

        }
        public IActionResult GetAvatar(string nick)
        {
            byte[] bytes = accountService.GetAvatar(nick);
            return File(bytes, MediaTypeNames.Image.Jpeg);
        }


        public IActionResult UploadBackground(string nick, IFormFile file)
        {
            Console.WriteLine("Upload background for " + nick);

            string extension = Path.GetExtension(file.FileName);
            Console.WriteLine(extension);

            string tmpPath = "UploadData/" + nick + extension;
            using (var stream = System.IO.File.Create(tmpPath))
            {
                file.CopyTo(stream);
            }
            byte[] bytes = System.IO.File.ReadAllBytes(tmpPath);
            System.IO.File.Delete(tmpPath);


            OperationMessage msg = accountService.ChangeBackground(nick, bytes, extension);
            return Content(JsonConvert.SerializeObject(msg));
        }
        public IActionResult GetBackground(string nick)
        {
            byte[] bytes = accountService.GetBackground(nick);
            return File(bytes, MediaTypeNames.Image.Jpeg);
        }



        /*public IActionResult GetAccount(string nick)
         {
             return Content(JsonConvert.SerializeObject(Core.ctx.Players.FirstOrDefault(c => c.Nick == nick), new JsonSerializerSettings()
             {
                 Formatting = Formatting.Indented,
                 ReferenceLoopHandling = ReferenceLoopHandling.Ignore
             }));
         }*/
    }

    class FileDto
    {
        public string Nick { get; set; }
        public byte[] File { get; set; }
    }
}
