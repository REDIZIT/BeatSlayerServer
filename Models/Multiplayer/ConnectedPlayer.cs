using BeatSlayerServer.Enums.Game;
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
    public class LobbyPlayer
    {
        public ConnectedPlayer Player { get; set; }
        public int SlotIndex { get; set; }
        public bool IsHost { get; set; }
        public ReadyState State { get; set; }
        public ModEnum Mods { get; set; }


        public bool IsGameSceneLoaded { get; set; }
        public float Score { get; set; }
        public int Combo { get; set; }


        public enum ReadyState
        {
            NotReady, Ready, Downloading
        }


        public LobbyPlayer(ConnectedPlayer player, int slotIndex, bool isHost)
        {
            Player = player;
            SlotIndex = slotIndex;
            IsHost = isHost;
            State = ReadyState.NotReady;
        }
    }

    public class ConnectedPlayerDTO
    {
        public string Nick { get; set; }

        public ConnectedPlayerDTO(ConnectedPlayer player)
        {
            Nick = player.Nick;
        }
    }
    public class LobbyPlayerDTO
    {
        public ConnectedPlayerDTO Player { get; set; }
        public int SlotIndex { get; set; }
        public bool IsHost { get; set; }
        public LobbyPlayer.ReadyState State { get; set; }
        public ModEnum Mods { get; set; }

        public LobbyPlayerDTO(LobbyPlayer player)
        {
            Player = new ConnectedPlayerDTO(player.Player);
            SlotIndex = player.SlotIndex;
            IsHost = player.IsHost;
            State = player.State;
            Mods = player.Mods;
        }
    }
}
