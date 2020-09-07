﻿using BeatSlayerServer.Dtos.Mapping;
using BeatSlayerServer.Enums.Game;
using BeatSlayerServer.Models.Database;
using BeatSlayerServer.Models.Multiplayer;
using BeatSlayerServer.Models.Multiplayer.Chat;
using BeatSlayerServer.Utils;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BeatSlayerServer.Services.Multiplayer
{
    public class LobbyService
    {
        public Dictionary<int, Lobby> Lobbies { get;  set; } = new Dictionary<int, Lobby>();

        private readonly ConnectionService connService;
        private readonly IHubContext<GameHub> hub;


        public LobbyService(ConnectionService connService, IHubContext<GameHub> hub)
        {
            this.connService = connService;
            this.hub = hub;
        }
        private void Log(string message)
        {
            Console.WriteLine(message);
        }

        public void OnPlayerDisconnected(ConnectedPlayer player)
        {
            foreach (Lobby lobby in Lobbies.Values)
            {
                if (lobby.Players.Any(c => c.Value.Player == player))
                {
                    LeaveLobby(lobby.LobbyId, player);
                }
            }
        }


        public IEnumerable<LobbyPlayer> GetPlayersInLobby(int lobbyId)
        {
            return Lobbies[lobbyId].Players.Values;
        }


        public List<LobbyDTO> GetLobbies()
        {
            List<LobbyDTO> result = new List<LobbyDTO>();
            foreach (Lobby lobby in Lobbies.Values)
            {
                result.Add(new LobbyDTO(lobby));
            }

            return result;
        }
        public Lobby CreateLobby(ConnectedPlayer player)
        {
            // Search for free space for new lobby
            for (int i = 0; i < Lobby.MAX_PLAYERS; i++)
            {
                // If lobby with id = i have been deleted
                if (!Lobbies.ContainsKey(i))
                {
                    Lobby lobby = new Lobby(i, player);
                    Lobbies[i] = lobby;
                    return lobby;
                }
            }

            throw new System.Exception("Not found space for new lobby");
        }

        public LobbyDTO JoinLobby(ConnectedPlayer player, int lobbyId)
        {
            Lobby lobby = Lobbies[lobbyId];
            LobbyPlayer lobbyPlayer = lobby.Join(player);

            Log("JoinLobby");
            SendLobbyToAllExcept(lobby.LobbyId, player.Nick, "OnLobbyPlayerJoin", new LobbyPlayerDTO(lobbyPlayer));

            return new LobbyDTO(lobby);
        }
        public void LeaveLobby(int lobbyId, ConnectedPlayer player)
        {
            if (!Lobbies.ContainsKey(lobbyId)) return;
            Lobby lobby = Lobbies[lobbyId];

            if (lobby.Players.All(c => c.Value.Player != player)) return;
            LobbyPlayer lobbyPlayer = lobby.Players.First(c => c.Value.Player == player).Value;

            // Ping about player leaving
            SendLobbyToAllExcept(lobby.LobbyId, player.Nick, "OnLobbyPlayerLeave", new LobbyPlayerDTO(lobbyPlayer));

            // If host leaving
            if (lobbyPlayer.IsHost)
            {
                if (lobby.Players.Count > 1)
                {
                    // Get rest players
                    IEnumerable<LobbyPlayer> restPlayers = lobby.Players.Where(c => c.Value.Player != player).Select(c => c.Value);

                    // Sort by slot index
                    restPlayers = restPlayers.OrderByDescending(c => c.SlotIndex);
                    // Rise if c.player was under the leaving host and put underground if upper
                    restPlayers = restPlayers.OrderByDescending(c => c.SlotIndex > lobbyPlayer.SlotIndex);

                    // Get first player, he will be new host
                    ChangeHost(lobbyId, restPlayers.First().Player.Nick);
                }
            }

            // Kick from lobby
            lobby.Leave(player);

            if (lobby.Players.Count == 0)
            {
                Console.WriteLine("Destroy lobby");
                Lobbies.Remove(lobbyId);
            }
        }

        public void ChangeMap(int lobbyId, MapData map, DifficultyData diff)
        {
            Lobbies[lobbyId].ChangeMap(map, diff);

            SendLobbyToAll(lobbyId, "OnLobbyMapChange", map, diff);
        }
        public void HostStartChangingMap(int lobbyId)
        {
            Lobbies[lobbyId].IsHostChangingMap = true;

            SendLobbyToAll(lobbyId, "OnHostStartChangingMap");
        }

        public void HostCancelChangingMap(int lobbyId)
        {
            Lobbies[lobbyId].IsHostChangingMap = false;

            SendLobbyToAll(lobbyId, "OnHostCancelChangingMap");
        }


        public void ChangeMods(int lobbyId, string nick, ModEnum mods)
        {
            Lobbies[lobbyId].ChangeMods(nick, mods);

            SendLobbyToAll(lobbyId, "OnRemotePlayerModsChange", nick, mods);
        }

        public void ChangeReadyState(int lobbyId, string nick, LobbyPlayer.ReadyState state)
        {
            Lobbies[lobbyId].Players.First(c => c.Value.Player.Nick == nick).Value.State = state;
            SendLobbyToAll(lobbyId, "OnRemotePlayerReadyStateChange", nick, state);
        }

        public void ChangeHost(int lobbyId, string nick)
        {
            string prevHost = Lobbies[lobbyId].Players.Values.First(c => c.IsHost).Player.Nick;
            foreach (LobbyPlayer player in Lobbies[lobbyId].Players.Values)
            {
                player.IsHost = player.Player.Nick == nick;
            }

            SendLobbyToAllExcept(lobbyId, prevHost, "OnLobbyHostChange", new LobbyPlayerDTO(Lobbies[lobbyId].Players.First(c => c.Value.IsHost).Value));
        }
        public void Kick(int lobbyId, string nick)
        {
            LobbyPlayer player = Lobbies[lobbyId].Players.First(c => c.Value.Player.Nick == nick).Value;

            SendLobbyToAll(lobbyId, "OnLobbyPlayerKick", new LobbyPlayerDTO(player));

            Lobbies[lobbyId].Leave(player.Player);
        }

        public void OnStartDownloading(int lobbyId, string nick)
        {
            SendLobbyToAllExcept(lobbyId, nick, "OnRemotePlayerStartDownloading", nick);
        }
        public void OnDownloadProgress(int lobbyId, string nick, int percent)
        {
            SendLobbyToAllExcept(lobbyId, nick, "OnRemotePlayerDownloadProgress", nick, percent);
        }
        public void OnDownloaded(int lobbyId, string nick)
        {
            SendLobbyToAllExcept(lobbyId, nick, "OnRemotePlayerDownloaded", nick);
        }



        #region Chat

        public void SendPlayerMessage(int lobbyId, LobbyPlayerChatMessage message)
        {
            SendLobbyToAll(lobbyId, "OnLobbyPlayerMessage", message);
        }


        #endregion











        private void SendLobbyToAll(int lobbyId, string methodName, params object[] args)
        {
            //List<string> playersToPing = new List<string>();
            //playersToPing.AddRange(Lobbies[lobbyId].PlayersIds);

            List<string> playersToPing = Lobbies[lobbyId].Players.Values.Select(c => c.Player.ConnectionId).ToList();

            connService.InvokeAsync(playersToPing, methodName, args);
        }
        private void SendLobbyToAllExcept(int lobbyId, string exceptNick, string methodName, params object[] args)
        {
            string connId = Lobbies[lobbyId].Players.First(c => c.Value.Player.Nick == exceptNick).Value.Player.ConnectionId;

            //List<string> playersToPing = new List<string>();
            //playersToPing.AddRange(Lobbies[lobbyId].PlayersIds);
            //playersToPing.Remove(connId);

            List<string> playersToPing = Lobbies[lobbyId].Players.Values.Where(c => c.Player.Nick != exceptNick).Select(c => c.Player.ConnectionId).ToList();


            connService.InvokeAsync(playersToPing, methodName, args);
        }
    }
}
