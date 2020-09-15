using BeatSlayerServer.Models.Configuration;
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

        public EventService(SettingsWrapper settingsWrapper)
        {
            settings = settingsWrapper.settings;

            eventFolder = settings.SharedFolder + "/Events";
            multiplayerEventFolder = eventFolder + "/Multiplayer";

            Directory.CreateDirectory(eventFolder);
            Directory.CreateDirectory(multiplayerEventFolder);
        }

        /// <summary>Multiplayer event player played a game</summary>
        public void OnPlayerPlay(string nick)
        {
            string filepath = multiplayerEventFolder + "/" + nick + ".txt";

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
