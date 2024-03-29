﻿using BeatSlayerServer.Dtos.Mapping;
using BeatSlayerServer.Enums.Game;
using BeatSlayerServer.Models.Maps;
using BeatSlayerServer.Models.Multiplayer;
using BeatSlayerServer.Models.Multiplayer.Chat;
using BeatSlayerServer.Utils;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BeatSlayerServer.Services.Multiplayer
{
    public class LobbyService
    {
        public Dictionary<int, Lobby> Lobbies { get; set; } = new Dictionary<int, Lobby>();

        private readonly ConnectionService connService;
        private readonly IHubContext<GameHub> hub;
        private readonly ILogger<LobbyService> logger;


        public LobbyService(ILogger<LobbyService> logger, ConnectionService connService, IHubContext<GameHub> hub)
        {
            this.logger = logger;
            this.connService = connService;
            this.hub = hub;
        }

        public void OnPlayerDisconnected(ConnectedPlayer player)
        {
            foreach (Lobby lobby in Lobbies.Values)
            {
                if (lobby.Players.Any(c => c.Value.Player.Nick == player.Nick))
                {
                    LeaveLobby(lobby.LobbyId, player);
                }
            }
        }

        #region Lobbies (get/create/join/leave/rename)

        public List<LobbyDTO> GetLobbies()
        {
            List<LobbyDTO> result = new List<LobbyDTO>();
            foreach (Lobby lobby in Lobbies.Values)
            {
                result.Add(new LobbyDTO(lobby));
            }

            return result;
        }
        public LobbyDTO GetLobby(int lobbyId)
        {
            return new LobbyDTO(Lobbies[lobbyId]);
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
                    logger.LogInformation("[{action}] Lobbies count is {lobbiesCount}", "Lobbies Changed", Lobbies.Values.Count);
                    return lobby;
                }
            }

            throw new Exception("Not found space for new lobby");
        }

        public LobbyDTO JoinLobby(ConnectedPlayer player, int lobbyId)
        {
            if (Lobbies.Any(c => c.Value.Players.Any(c => c.Value.Player == player)))
            {
                return null;
            }

            Lobby lobby = Lobbies[lobbyId];
            LobbyPlayer lobbyPlayer = lobby.Join(player);
            if (lobbyPlayer == null) return null;


            SendLobbyToAllExcept(lobby.LobbyId, player.Nick, "OnLobbyPlayerJoin", new LobbyPlayerDTO(lobbyPlayer));
            SendLobbyToAllExcept(lobby.LobbyId, player.Nick, "OnLobbySystemMessage", new LobbySystemChatMessage()
            {
                MessageType = LobbySystemChatMessage.SystemMessageType.Join,
                PlayerNick = player.Nick
            });

            logger.LogInformation("[{action}] All players in lobbies {playersInLobbies}", "PlayersInLobbies", Lobbies.Values.Sum(c => c.Players.Count));

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
            SendLobbyToAllExcept(lobby.LobbyId, player.Nick, "OnLobbySystemMessage", new LobbySystemChatMessage()
            {
                MessageType = LobbySystemChatMessage.SystemMessageType.Leave,
                PlayerNick = player.Nick
            });


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
                logger.LogInformation("[{action}] Lobbies count is {lobbiesCount}", "Lobbies Changed", Lobbies.Values.Count);
            }

            logger.LogInformation("[{action}] All players in lobbies {playersInLobbies}", "PlayersInLobbies", Lobbies.Values.Sum(c => c.Players.Count));
        }
        public void Rename(int lobbyId, string lobbyName)
        {
            Lobbies[lobbyId].LobbyName = lobbyName;
            SendLobbyToAll(lobbyId, "OnLobbyRename", lobbyName);
        }
        public void ChangePassword(int lobbyId, string password)
        {
            Lobbies[lobbyId].Password = password;
            SendLobbyToAll(lobbyId, "OnLobbyChangePassword", password);
        }

        #endregion

        #region Change map

        public void ChangeMap(int lobbyId, BasicMapData map, DifficultyData diff)
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

        #endregion


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

            SendLobbyToAll(lobbyId, "OnLobbyHostChange", new LobbyPlayerDTO(Lobbies[lobbyId].Players.First(c => c.Value.IsHost).Value));
        }
        public void Kick(int lobbyId, string nick)
        {
            LobbyPlayer player = Lobbies[lobbyId].Players.First(c => c.Value.Player.Nick == nick).Value;

            SendLobbyToAll(lobbyId, "OnLobbyPlayerKick", new LobbyPlayerDTO(player));
            SendLobbyToAllExcept(lobbyId, nick, "OnLobbySystemMessage", new LobbySystemChatMessage()
            {
                MessageType = LobbySystemChatMessage.SystemMessageType.Kick,
                PlayerNick = nick
            });

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
        public void OnLobbyPlayerStartTyping(int lobbyId, string nick)
        {
            SendLobbyToAllExcept(lobbyId, nick, "OnLobbyPlayerStartTyping", nick);
        }
        public void OnLobbyPlayerStopTyping(int lobbyId, string nick)
        {
            SendLobbyToAll(lobbyId, "OnLobbyPlayerStopTyping", nick);
        }


        #endregion



        #region Multiplayer

        public void OnGameStart(int lobbyId)
        {
            Lobbies[lobbyId].IsPlaying = true;
            SendLobbyToAll(lobbyId, "OnMultiplayerGameStart");

            foreach (var player in Lobbies[lobbyId].Players.Values)
            {
                player.IsGameSceneLoaded = false;
                ChangeReadyState(lobbyId, player.Player.Nick, LobbyPlayer.ReadyState.Playing);
            }
        }
        public void OnPlayerLoaded(int lobbyId, string nick)
        {
            Console.WriteLine(nick + " loaded");
            Lobbies[lobbyId].Players.First(c => c.Value.Player.Nick == nick).Value.IsGameSceneLoaded = true;

            CheckIsAllPlayersLoaded(lobbyId);
        }
        public bool AreAllPlayersLoaded(int lobbyId)
        {
            return Lobbies[lobbyId].Players.All(c => c.Value.IsGameSceneLoaded);
        }
        private void CheckIsAllPlayersLoaded(int lobbyId)
        {
            if (!Lobbies[lobbyId].IsPlaying) return;

            if (AreAllPlayersLoaded(lobbyId))
            {
                SendLobbyToAll(lobbyId, "OnMultiplayerPlayersLoaded");
            }
        }
        public void ScoreUpdate(int lobbyId, string nick, float score, int combo)
        {
            LobbyPlayer player = Lobbies[lobbyId].Players.First(c => c.Value.Player.Nick == nick).Value;
            player.Score = score;
            player.Combo = combo;

            SendLobbyToAllExcept(lobbyId, nick, "OnMultiplayerScoreUpdate", nick, score, combo);
        }
        public void OnMultiplayerPlayerAliveChanged(int lobbyId, string nick, bool isAlive)
        {
            SendLobbyToAllExcept(lobbyId, nick, "OnMultiplayerPlayerAliveChanged", nick, isAlive);
        }
        public void PlayerFinished(int lobbyId, string nick, ReplayData replay)
        {
            SendLobbyToAll(lobbyId, "OnMultiplayerPlayerFinished", nick, replay);
            ChangeReadyState(lobbyId, nick, LobbyPlayer.ReadyState.NotReady);

            UpdateLobbyPlayStatus(lobbyId);
        }
        public void PlayerLeft(int lobbyId, string nick)
        {
            SendLobbyToAll(lobbyId, "OnMultiplayerPlayerLeft", nick);
            ChangeReadyState(lobbyId, nick, LobbyPlayer.ReadyState.NotReady);

            UpdateLobbyPlayStatus(lobbyId);
        }

        private void UpdateLobbyPlayStatus(int lobbyId)
        {
            Lobby lobby = Lobbies[lobbyId];

            bool prevStatus = lobby.IsPlaying;

            lobby.IsPlaying = lobby.Players.Any(p => p.Value.State == LobbyPlayer.ReadyState.Playing);

            if (lobby.IsPlaying != prevStatus)
            {
                SendLobbyToAll(lobbyId, "OnLobbyPlayStatusChanged", lobby.IsPlaying);
            }
        }

        #endregion






        private void SendLobbyToAll(int lobbyId, string methodName, params object[] args)
        {
            if (!Lobbies.ContainsKey(lobbyId)) return;

            Console.WriteLine("Send: " + methodName);

            List<string> playersToPing = Lobbies[lobbyId].Players.Values.Select(c => c.Player.ConnectionId).ToList();

            connService.InvokeAsync(playersToPing, methodName, args);
        }
        private void SendLobbyToAllExcept(int lobbyId, string exceptNick, string methodName, params object[] args)
        {
            if (!Lobbies.ContainsKey(lobbyId)) return;

            Console.WriteLine("Send except: " + methodName);

            string connId = Lobbies[lobbyId].Players.First(c => c.Value.Player.Nick == exceptNick).Value.Player.ConnectionId;

            List<string> playersToPing = Lobbies[lobbyId].Players.Values.Where(c => c.Player.Nick != exceptNick).Select(c => c.Player.ConnectionId).ToList();


            connService.InvokeAsync(playersToPing, methodName, args);
        }
    }
}
