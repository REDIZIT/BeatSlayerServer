using BeatSlayerServer.Dtos.Mapping;
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

        public Dictionary<int, LobbyPlayer> Players { get; set; }
        public List<string> PlayersIds { get; set; }

        public MapData SelectedMap { get; set; }
        public DifficultyData SelectedDifficulty { get; set; }
        public bool IsHostChangingMap { get; set; }


        public const int MAX_PLAYERS = 10;


        public Lobby(int lobbyId, ConnectedPlayer firstPlayer)
        {
            LobbyId = lobbyId;
            LobbyName = firstPlayer.Nick + "'s lobby";
            Players = new Dictionary<int, LobbyPlayer>();
            PlayersIds = new List<string>();
        }

        public LobbyPlayer Join(ConnectedPlayer player)
        {
            // Searching for free slots
            for (int i = 0; i < MAX_PLAYERS; i++)
            {
                // If slot is empty
                if (!Players.ContainsKey(i))
                {
                    Players[i] = new LobbyPlayer(player, i, Players.Count == 0);
                    PlayersIds.Add(player.ConnectionId);
                    Console.WriteLine("Add player id : " + player.ConnectionId);
                    return Players[i];
                }
            }

            throw new NotImplementedException("Lobby is full, new player can't join it");
        }

        public void Leave(ConnectedPlayer player)
        {
            Players.RemoveAll(c => c.Value.Player == player);
            Console.WriteLine("Remove " + player.ConnectionId + ". Count was " + PlayersIds.Count);
            PlayersIds.RemoveAll(c => c == player.ConnectionId);
            Console.WriteLine("Count become " + PlayersIds.Count);
        }

        public void ChangeMap(MapData map, DifficultyData diff)
        {
            IsHostChangingMap = false;
            SelectedMap = map;
            SelectedDifficulty = diff;
        }

        public void ChangeMods(string nick, ModEnum mods)
        {
            Players.First(c => c.Value.Player.Nick == nick).Value.Mods = mods;
        }
    }
}
