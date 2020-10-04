using BeatSlayerServer.Enums;
using BeatSlayerServer.Utils;
using BeatSlayerServer.ProjectManagement;
using BeatSlayerServer.Services.Messaging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using BeatSlayerServer.Models.Configuration;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace BeatSlayerServer.Services.MapsManagement
{
    public class PublishService
    {
        private readonly MyDbContext ctx;
        private ServerSettings settings;
        private readonly BotService botService;
        private readonly ModerationService moderationService;
        private readonly MapsService mapsService;

        public PublishService(MyDbContext ctx, IOptionsMonitor<ServerSettings> mon, BotService botService, ModerationService moderationService, MapsService mapsService)
        {
            this.ctx = ctx;
            this.botService = botService;
            this.moderationService = moderationService;
            this.mapsService = mapsService;

            settings = mon.CurrentValue;
            mon.OnChange((sets) => settings = sets);
        }



        /// <summary>
        /// Saving file on disk and invoking <see cref="PublishProject(string)"/>
        /// </summary>
        public async Task<OperationResult> PublishProject(IFormFile file)
        {
            string trackname = Path.GetFileNameWithoutExtension(file.FileName);
            string folder = settings.Publishing.TempFolder;
            string tempfilepath = folder + "/" + trackname + ".bsz";
            Directory.CreateDirectory(folder);

            using(Stream stream = File.Create(tempfilepath))
            {
                file.CopyTo(stream);
            }

            return await PublishProject(tempfilepath);
        }

        /// <summary>
        /// Unpack project, create groupinfo, mapinfo files and move temp file to new folder
        /// </summary>
        public async Task<OperationResult> PublishProject(string bszFilePath)
        {
            try
            {
                Project compressedProject = ProjectManager.LoadProject(bszFilePath);

                string trackname = compressedProject.author + "-" + compressedProject.name;
                string mapFolder = settings.TracksFolder + "/" + trackname + "/" + compressedProject.creatorNick;

                if (IsTracknameBad(trackname))
                {
                    File.Delete(bszFilePath);
                    throw new Exception("Project author and name can't be accepted. Use English, Russian alphabets, digits and special symbols");
                }
                if (IsTracknameBad(compressedProject.creatorNick))
                {
                    File.Delete(bszFilePath);
                    throw new Exception("Your nick can't be accepted. Use English, Russian alphabets, digits and special symbols");
                }


                Directory.CreateDirectory(mapFolder);

                File.Move(bszFilePath, mapFolder + "/" + trackname + ".bsz", true);



                // If first upload add to mapper account publish maps statistics
                var mapInfo = ProjectManager.GetMapInfo(trackname, compressedProject.creatorNick);
                if(mapInfo == null)
                {
                    var mapperAcc = ctx.Players.FirstOrDefault(c => c.Nick == compressedProject.creatorNick);
                    mapperAcc.MapsPublished++;
                    ctx.SaveChanges();
                }


                ProjectManager.UnpackProject(compressedProject, mapFolder);
                ProjectManager.CheckInfoFiles(compressedProject);

                AddMapToDatabase(compressedProject);

                mapsService.CreateCovers(trackname, compressedProject.creatorNick);



                await botService.SendMapPublished(compressedProject.author + " - " + compressedProject.name, compressedProject.creatorNick);
                return new OperationResult(OperationResult.State.Success);
            }
            catch (Exception err)
            {
                OperationResult result = new OperationResult(OperationResult.State.Fail, err.Message + "-srv");
                return result;
            }
        }

        /// <summary>
        /// Is trackname bad with the requirements for storaging as file (Eng, Rus, Digits, Spec. symbols)
        /// </summary>
        public bool IsTracknameBad(string str)
        {
            return Regex.IsMatch(str, @"[^a-zA-Z0-9_?а-яёА-Я- '()\[\]\{\}+*~!@#$%^&\/\\`>=<.,;]");
        }






        private async Task<OperationResult> PublishUpdateForModeration(string bszFilePath)
        {
            return await moderationService.OnApprovedMapUpdate(bszFilePath);
        }



        private void AddMapToDatabase(Project proj)
        {
            //Console.WriteLine("AddMapToDatabase isn't implemented");
            var db_group = ctx.Groups.FirstOrDefault(c => c.Author == proj.author && c.Name == proj.name);
            if (db_group == null)
            {
                db_group = new Utils.Database.GroupInfo()
                {
                    Author = proj.author,
                    Name = proj.name
                };
                ctx.Groups.Add(db_group);
            }

            var db_map = db_group.Maps.FirstOrDefault(c => c.Nick == proj.creatorNick);
            if(db_map == null)
            {
                db_group.Maps.Add(new Models.Database.MapInfo()
                {
                    Group = db_group,
                    Nick = proj.creatorNick
                });
            }
            
            ctx.SaveChanges();
        }
    }
}
