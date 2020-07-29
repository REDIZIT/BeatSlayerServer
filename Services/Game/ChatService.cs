using BeatSlayerServer.Controllers;
using BeatSlayerServer.Utils;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BeatSlayerServer.Models.Configuration;

namespace BeatSlayerServer.Services.Chat
{
    public class ChatService
    {
        public List<ChatGroup> groups = new List<ChatGroup>();

        private ServerSettings settings;
        private readonly IHubContext<GameHub> hub;


        public ChatService(IOptionsMonitor<ServerSettings> mon, IHubContext<GameHub> hub)
        {
            settings = mon.CurrentValue;
            this.hub = hub;
            
            LoadHistory();

            mon.OnChange((settings) =>
            {
                this.settings = settings;
            });
        }

        public List<ChatMessage> JoinGroup(string connectionId, string groupName)
        {
            //groups.Find(c => c.name == groupName).users.Add(nick);
            hub.Groups.AddToGroupAsync(connectionId, groupName);

            return GetChatHistory(groupName);
        }
        public void LeaveGroup(string connectionId, string groupName)
        {
            //groups.Find(c => c.name == groupName).users.Remove(nick);
            hub.Groups.RemoveFromGroupAsync(connectionId, groupName);
        }


        public ChatMessage SendMessage(string nick, string message, AccountRole role, string groupName)
        {
            ChatMessage msg = new ChatMessage(nick, message, role, groupName);
            Console.WriteLine($"SendMessage: ({groupName}) {nick}: " + message);

            ChatGroup group = groups.Find(c => c.name == groupName);
            group.history.Add(msg);

            SaveHistory();

            return msg;
        }

        public List<ChatMessage> GetChatHistory(string groupName)
        {
            return groups.Find(c => c.name == groupName).history.TakeLast(30).ToList();
        }

        public List<ChatGroupData> GetGroups()
        {
            List<ChatGroupData> ls = new List<ChatGroupData>();

            foreach (var item in groups)
            {
                ls.Add(new ChatGroupData()
                {
                    Name = item.name
                });
            }
            return ls;
        }



        void SaveHistory()
        {
            string json = JsonConvert.SerializeObject(groups);

            Console.WriteLine("Save history to " + settings.Chat.HistoryFilePath);
            File.WriteAllText(settings.Chat.HistoryFilePath, json);
        }

        void LoadHistory()
        {
            if (!File.Exists(settings.Chat.HistoryFilePath))
            {
                groups = new List<ChatGroup>()
                {
                    new ChatGroup("English"),
                    new ChatGroup("Russian"),
                    new ChatGroup("French")
                };
            }
            else
            {
                string json = File.ReadAllText(settings.Chat.HistoryFilePath);
                groups = JsonConvert.DeserializeObject<List<ChatGroup>>(json);
            }
        }
    }

    public class ChatMessage
    {
        public string nick, message;
        public AccountRole role;
        public string groupName;

        public ChatMessage() { }
        public ChatMessage(string nick, string msg, AccountRole role, string groupName)
        {
            this.nick = nick;
            this.message = msg;
            this.role = role;
            this.groupName = groupName;
        }
    }
    public class ChatGroup
    {
        public string name;
        public List<ChatMessage> history = new List<ChatMessage>();

        public ChatGroup(string name)
        {
            this.name = name;
        }
    }
    public class ChatGroupData
    {
        public string Name { get; set; }
    }
}
