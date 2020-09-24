using BeatSlayerServer.Models.Configuration;
using BeatSlayerServer.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BeatSlayerServer.Services
{
    public class EventService
    {
        private readonly string eventFolder;
        private readonly string multiplayerEventFolder;

        private readonly ServerSettings settings;
        //private readonly MyDbContext ctx;

        public EventService(SettingsWrapper settingsWrapper/*, MyDbContext ctx*/)
        {
            settings = settingsWrapper.settings;
            //this.ctx = ctx;

            eventFolder = settings.SharedFolder + "/Events";
            multiplayerEventFolder = eventFolder + "/Multiplayer";

            Directory.CreateDirectory(eventFolder);
            Directory.CreateDirectory(multiplayerEventFolder);
        }

        /// <summary>Multiplayer event player played a game</summary>
        public void OnPlayerPlay(string nick)
        {
            string filepath = multiplayerEventFolder + "/" + nick + ".txt";

            //// If player has >50k RP
            //if (ctx.Players.Select(c => new { nick = c.Nick, rp = c.RP }).First(c => c.nick == nick).rp >= 50000)
            //    return;

            if (File.Exists(filepath))
            {
                string content = File.ReadAllText(filepath);
                int count = int.Parse(content) + 1;
                File.WriteAllText(filepath, count.ToString());
            }
            else
            {
                File.WriteAllText(filepath, "1");
            }
        }

        public string GetMultiplayerEventResults()
        {
            StringBuilder builder = new StringBuilder();


            List<KeyValuePair<string, int>> results = new List<KeyValuePair<string, int>>();

            foreach (string filepath in Directory.GetFiles(multiplayerEventFolder))
            {
                string nick = Path.GetFileNameWithoutExtension(filepath);
                string content = File.ReadAllText(filepath);

                results.Add(new KeyValuePair<string, int>(nick, int.Parse(content)));
            }



            results = results.OrderByDescending(c => c.Value).ToList();

            foreach (var pair in results)
            {
                builder.AppendLine(pair.Key + ":" + pair.Value);
            }

            return builder.ToString();
        }

        public DateTime[] GetStartAndEndEventTimes()
        {
            return new DateTime[2] { settings.Event.EventStartTime, settings.Event.EventEndTime };
        }
    }
}
