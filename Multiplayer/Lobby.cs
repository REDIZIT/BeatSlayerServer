using BeatSlayerServer.Enums.Game;
using BeatSlayerServer.Extensions;
using BeatSlayerServer.Models.Database;
using BeatSlayerServer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BeatSlayerServer.Models.Multiplayer
{
    public class Lobby
    {
        public string LobbyName { get; set; }
        public int LobbyId { get; set; }

        public Dictionary<int, LobbyPlayer> Players { get; set; } = new Dictionary<int, LobbyPlayer>();

        public MapData SelectedMap { get; set; }
        public bool IsHostChangingMap { get; set; }


        public const int MAX_PLAYERS = 10;


        public Lobby(int lobbyId, ConnectedPlayer firstPlayer)
        {
            LobbyId = lobbyId;

            //Players[0] = new LobbyPlayer(firstPlayer, 0, true);
            LobbyName = firstPlayer.Nick + "'s lobby";

            SelectedMap = new MapData()
            {
                Group = new Utils.Database.GroupData()
                {
                    Author = "Nightcore",
                    Name = "IZY"
                },
                Nick = "REDIZIT"
            };
        }

        public LobbyPlayer Join(ConnectedPlayer player)
        {
            //if (Players.Values.Any(c => c.Player == player && c.IsHost)) return;

            // Searching for free slots
            for (int i = 0; i < MAX_PLAYERS; i++)
            {
                // If slot is empty
                if (!Players.ContainsKey(i))
                {
                    Players[i] = new LobbyPlayer(player, i, Players.Count == 0);
                    // TODO: Send info to players
                    return Players[i];
                }
            }

            throw new NotImplementedException("Lobby is full, new player can't join it");
        }

        public void Leave(ConnectedPlayer player)
        {
            // Idk how it will work tbh xD
            Players.RemoveAll(c => c.Value.Player == player);
            // TODO: Send info to players
        }

        public void ChangeMap(MapData map)
        {
            SelectedMap = map;
            // TODO: Notify lobby players
        }

        public void ChangeMods(string nick, ModEnum mods)
        {
            Players.First(c => c.Value.Player.Nick == nick).Value.Mods = mods;
        }
    }
}
