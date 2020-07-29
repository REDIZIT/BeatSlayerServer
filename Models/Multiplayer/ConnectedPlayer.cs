using System;

namespace BeatSlayerServer.Utils
{
    public class ConnectedPlayer
    {
        public string ConnectionId { get; }
        public string Nick { get; set; }
        public string Ip { get; }
        public TimeSpan SessionTime => DateTime.Now - connectTime;

        private readonly DateTime connectTime;


        public ConnectedPlayer(string id, string ip)
        {
            ConnectionId = id;
            Ip = ip;
            connectTime = DateTime.Now;
        }
    }
}
