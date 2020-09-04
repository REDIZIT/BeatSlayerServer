using BeatSlayerServer.Enums;
using BeatSlayerServer.Utils.Moderation;
using BeatSlayerServer.ProjectManagement;
using BeatSlayerServer.Services.Messaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using BeatSlayerServer.Models.Configuration;
using BeatSlayerServer.Models.Moderation;
using BeatSlayerServer.Utils;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json;
using BeatSlayerServer.Models.Database.Maps;
using System.Diagnostics;

namespace BeatSlayerServer.Services.MapsManagement
{
    public class ModerationService
    {
        private readonly string TracksFolder;
        private readonly string ModerationFolder, ModerationTrackFolder;

        private readonly MyDbContext ctx;
        private readonly ServerSettings settings;
        private readonly BotService botService;

        public ModerationService(MyDbContext ctx, SettingsWrapper wrapper, BotService botService)
        {
            this.ctx = ctx;
            settings = wrapper.settings;
            this.botService = botService;

            TracksFolder = settings.TracksFolder;
            ModerationFolder = settings.Moderation.MainFolder;
            ModerationTrackFolder = settings.Moderation.MainFolder + "/Tracks";

            LoadData();
        }

        public ModerationData data;




        /// <summary>
        /// Deprecated, use db version. Getting groups from <see cref="ModerationData"/> (file system).
        /// </summary>
        [Obsolete("Use GetApprovedGroups()")]
        public List<GroupInfo> GetApprovedGroupsLegacy()
        {
            var infos = data.approvedMaps.GroupBy(c => c.group).ToList();
            List<GroupInfo> groupInfos = new List<GroupInfo>();
            foreach (var item in infos)
            {
                groupInfos.Add(item.Key);
            }
            return groupInfos;
        }
        public List<GroupInfo> GetApprovedGroups()
        {
            Stopwatch w = Stopwatch.StartNew();

            var approvedGroups = ctx.Groups.Where(g => g.Maps.Any(m => m.PublishStatus == MapPublishStatus.Approved)).Select(c => new
            {
                c.Author,
                c.Name,
                MapsCount = c.Maps.Count
            });

            List<GroupInfo> result = new List<GroupInfo>();
            foreach (var group in approvedGroups)
            {
                GroupInfo info = new GroupInfo()
                {
                    author = group.Author,
                    name = group.Name,
                    mapsCount = group.MapsCount
                };

                result.Add(info);
            }

            w.Stop();
            Console.WriteLine("GetApprovedGroups done in " + w.ElapsedMilliseconds + "ms");

            return result;
        }


        public bool IsMapApproved(string trackname, string nick)
        {
            return data.approvedMaps.Any(c => c.nick == nick && c.group.author + "-" + c.group.name == trackname);
        }

        //public void ClearApprovedMaps()
        //{
        //    string trackFolder = TracksFolder;
        //    string[] groupsFolders = Directory.GetDirectories(trackFolder);
        //    foreach (string groupFolder in groupsFolders)
        //    {
        //        string[] maps = Directory.GetDirectories(groupFolder);

        //        foreach (string map in maps)
        //        {
        //            MapInfo mapInfo = ProjectManager.GetMapInfo(map, true);
        //            mapInfo.approved = false;
        //            ProjectManager.SetMapInfo(mapInfo);
        //        }
        //    }
        //}


        public List<ModerateOperation> GetModerationMaps()
        {
            string[] operationsFiles = Directory.GetFiles(ModerationTrackFolder, "*.xml", SearchOption.AllDirectories);

            List<ModerateOperation> operations = new List<ModerateOperation>();
            for (int i = 0; i < operationsFiles.Length; i++)
            {
                ModerateOperation op = LoadModerateOperation(operationsFiles[i]);
                operations.Add(op);
            }

            return operations;
        }
        public byte[] GetModerationMap(string trackname, string nick)
        {
            string filepath = ModerationTrackFolder + "/" + trackname + "/" + nick + "/" + trackname + ".bsz";
            string defaultpath = TracksFolder + "/" + trackname + "/" + nick + "/" + trackname + ".bsz";
            // If has not update file
            if (!File.Exists(filepath))
            {
                if (!File.Exists(defaultpath))
                {
                    return null;
                }
                else
                {
                    return File.ReadAllBytes(defaultpath);
                }
            }
            else
            {
                return File.ReadAllBytes(filepath);
            }
        }

        public async Task<OperationResult> SendModerationRequest(string trackname, string nick)
        {
            trackname = WebUtility.UrlDecode(trackname);
            nick = WebUtility.UrlDecode(nick);

            string mapModerationFolder = TracksFolder + "/" + trackname + "/" + nick;

            if (!Directory.Exists(mapModerationFolder)) return new OperationResult(OperationResult.State.Fail, "No such published map");

            string moderationMapFolder = ModerationTrackFolder + "/" + trackname + "/" + nick;
            if (!Directory.Exists(moderationMapFolder)) Directory.CreateDirectory(moderationMapFolder);


            ModerateOperation request = LoadModerateOperation(trackname, nick);
            if (request != null) return new OperationResult(OperationResult.State.Fail, "Your request is in handling, please wait");

            if (settings.Bans.ApproveBans != null && settings.Bans.ApproveBans.Contains(nick)) 
                return new OperationResult(OperationResult.State.Fail, "You have been banned and can't approve your maps");



            MapInfo info = ProjectManager.GetMapInfo(trackname, nick);

            ModerateOperation op = new ModerateOperation()
            {
                trackname = trackname,
                nick = nick,
                state = ModerateOperation.State.Waiting,
                uploadType = info.approved ? ModerateOperation.UploadType.Updated : ModerateOperation.UploadType.Requested
            };

            SaveModerateOperation(op);


            await botService.SendModerationRequest(op.trackname, op.nick, false);

            return new OperationResult(OperationResult.State.Success);
        }
        public async Task SendModerationResponse(ModerateOperation op)
        {
            Console.WriteLine(JsonConvert.SerializeObject(op));
            try
            {
                MapInfo info = ProjectManager.GetMapInfo(op.trackname, op.nick);

                if (info.approved)
                {
                    if (op.state == ModerateOperation.State.Approved)
                    {
                        // That means that map has been approved and now player want to update it
                        string bszFilePath = settings.TracksFolder + "/" + op.trackname + "/" + op.nick + "/" + op.trackname + ".bsz";

                        Project proj = ProjectManager.LoadProject(bszFilePath);

                        string mapFolder = settings.TracksFolder + "/" + op.trackname + "/" + op.nick;


                        ProjectManager.UnpackProject(proj, mapFolder);

                        File.Move(bszFilePath, mapFolder + "/" + op.trackname + ".bsz", true);

                        ProjectManager.CheckInfoFiles(proj);

                        GiveGranted(op.trackname, op.nick);

                        data.approvedMaps.Remove(data.approvedMaps.Find(c => c.group.author + "-" + c.group.name == op.trackname && c.nick == op.nick));
                        data.approvedMaps.Add(ProjectManager.GetMapInfo(op.trackname, op.nick));
                    }
                }
                else
                {
                    if (op.state == ModerateOperation.State.Approved)
                    {
                        GiveGranted(op.trackname, op.nick);
                        data.approvedMaps.Add(ProjectManager.GetMapInfo(op.trackname, op.nick));
                    }
                }

                await NotifyMessagers(op);
                

                SaveModerateOperation(op);
                SaveData();
            }
            catch (Exception err)
            {
                Console.WriteLine("Moderation error: " + err);
            }

        }

        /// <summary>
        /// Send message to bots and email
        /// </summary>
        private async Task NotifyMessagers(ModerateOperation op)
        {
            // Send email if player set it
            string email = ctx.Players.FirstOrDefault(c => c.Nick == op.nick).Email;
            if (!string.IsNullOrWhiteSpace(email) && email.Contains("@"))
            {
                if (op.state == ModerateOperation.State.Approved)
                {
                    await botService.SendMapApprovedMail(op.nick, email, op.trackname, op.moderatorNick, op.moderatorComment);
                }
                else
                {
                    await botService.SendMapRejectedMail(op.nick, email, op.trackname, op.moderatorNick, op.moderatorComment);
                }
            }


            // Send message to vk
            if(op.state == ModerateOperation.State.Approved)
            {
                await botService.SendMapApproved(op.trackname, op.nick, op.moderatorNick, op.moderatorComment);
            }
            else
            {
                await botService.SendMapRejected(op.trackname, op.nick, op.moderatorNick, op.moderatorComment);
            }
        }



        #region Save/Load ModerateOperation
        public void SaveModerateOperation(ModerateOperation op)
        {
            XmlSerializer xml = new XmlSerializer(typeof(ModerateOperation));
            using (var s = File.Create(ModerationTrackFolder + "/" + op.trackname + "/" + op.nick + "/Operation.xml"))
            {
                xml.Serialize(s, op);
            }
        }

        public ModerateOperation LoadModerateOperation(string path)
        {
            if (!File.Exists(path)) return null;

            XmlSerializer xml = new XmlSerializer(typeof(ModerateOperation));
            using (var s = File.OpenRead(path))
            {
                return (ModerateOperation)xml.Deserialize(s);
            }
        }
        public ModerateOperation LoadModerateOperation(string trackname, string nick)
        {
            return LoadModerateOperation(ModerationTrackFolder + "/" + trackname + "/" + nick + "/Operation.xml");
        }
        #endregion

        #region Save/Load ModerationData
        public void SaveData()
        {
            XmlSerializer xml = new XmlSerializer(typeof(ModerationData));
            using var s = File.Create(ModerationFolder + "/moderationData.xml");
            xml.Serialize(s, data);
        }
        public void LoadData()
        {
            XmlSerializer xml = new XmlSerializer(typeof(ModerationData));
            using var s = File.OpenRead(ModerationFolder + "/moderationData.xml");
            data = (ModerationData)xml.Deserialize(s);
        }
        #endregion




        private void GiveGranted(string trackname, string nick)
        {
            MapInfo info = ProjectManager.GetMapInfo(trackname, nick);

            info.approved = true;
            info.grantedTime = DateTime.Now;

            string author = trackname.Split('-')[0];
            string name = trackname.Split('-')[1];

            ProjectManager.SetMapInfo(trackname, nick, info);


            var map = ctx.Groups.FirstOrDefault(c => c.Author == author && c.Name == name).Maps.FirstOrDefault(c => c.Nick == nick);
            map.PublishStatus = Models.Database.Maps.MapPublishStatus.Approved;
            ctx.SaveChanges();
        }




        public async Task<OperationResult> OnApprovedMapUpdate(string bszFilePath)
        {
            Project proj = ProjectManager.LoadProject(bszFilePath);
            string trackname = proj.author + "-" + proj.name;
            string nick = proj.creatorNick;

            string mapFolder = ModerationTrackFolder + "/" + trackname + "/" + nick;
            string bszNewFilePath = mapFolder + "/" + trackname + ".bsz";


            // Move bsz from UploadData to Moderation folder
            File.Move(bszFilePath, bszNewFilePath, true);

            ModerateOperation op = new ModerateOperation()
            {
                trackname = trackname,
                nick = nick,
                state = ModerateOperation.State.Waiting,
                uploadType = ModerateOperation.UploadType.Updated
            };
            SaveModerateOperation(op);


            string botMsg = $"🤖 Игрок обновил approved карту 🤖\n{nick} обновил карту {trackname} и её нужно повторно проверить";

            //botService.SendMessageToVk(botMsg);
            await botService.SendModerationRequest(op.trackname, op.nick, true);

            return new OperationResult(OperationResult.State.Success);
        }
    }
}
