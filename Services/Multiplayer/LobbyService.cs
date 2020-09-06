using BeatSlayerServer.Enums.Game;
using BeatSlayerServer.Extensions;
using BeatSlayerServer.Models.Database;
using BeatSlayerServer.Models.Multiplayer;
using BeatSlayerServer.Utils;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Linq;

namespace BeatSlayerServer.Services.Multiplayer
{
    public class LobbyService
    {
        public Dictionary<int, Lobby> Lobbies { get; set; } = new Dictionary<int, Lobby>();

        private readonly IHubContext<GameHub> hub;


        public LobbyService(IHubContext<GameHub> hub)
        {
            this.hub = hub;
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
            return JoinLobby(player, Lobbies[lobbyId]);
        }
        public LobbyDTO JoinLobby(ConnectedPlayer player, Lobby lobby)
        {
            LobbyPlayer lobbyPlayer = lobby.Join(player);

            List<string> playersToPing = lobby.Players.Values.Where(c => c.Player != player).Select(c => c.Player.ConnectionId).ToList();
            hub.Clients.Clients(playersToPing).SendAsync("OnLobbyPlayerJoin", new LobbyPlayerDTO(lobbyPlayer));

            return new LobbyDTO(lobby);
        }
        public void LeaveLobby(int lobbyId, ConnectedPlayer player)
        {
            if (!Lobbies.ContainsKey(lobbyId)) return;
            Lobby lobby = Lobbies[lobbyId];

            if (lobby.Players.All(c => c.Value.Player != player)) return;
            LobbyPlayer lobbyPlayer = lobby.Players.First(c => c.Value.Player == player).Value;

            // Ping about player leaving
            List<string> playersToPing = lobby.Players.Values.Where(c => c.Player != player).Select(c => c.Player.ConnectionId).ToList();
            hub.Clients.Clients(playersToPing).SendAsync("OnLobbyPlayerLeave", new LobbyPlayerDTO(lobbyPlayer));


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
                Lobbies.Remove(lobbyId);
            }
        }

        public void ChangeMap(int lobbyId, MapData map)
        {
            Lobbies[lobbyId].ChangeMap(map);

            List<string> playersToPing = Lobbies[lobbyId].Players.Values.Select(c => c.Player.ConnectionId).ToList();
            hub.Clients.Clients(playersToPing).SendAsync("OnLobbyMapChange", map);
        }

        public void ChangeMods(int lobbyId, string nick, ModEnum mods)
        {
            Lobbies[lobbyId].ChangeMods(nick, mods);

            List<string> playersToPing = Lobbies[lobbyId].Players.Values.Select(c => c.Player.ConnectionId).ToList();
            hub.Clients.Clients(playersToPing).SendAsync("OnRemotePlayerModsChange", nick, mods);
        }

        public void ChangeReadyState(int lobbyId, string nick, LobbyPlayer.ReadyState state)
        {
            Lobbies[lobbyId].Players.First(c => c.Value.Player.Nick == nick).Value.State = state;

            //List<string> playersToPing = Lobbies[lobbyId].Players.Values.Where(c => c.Player.Nick != nick).Select(c => c.Player.ConnectionId).ToList();
            //hub.Clients.Clients(playersToPing).SendAsync("OnRemotePlayerReadyStateChange", new LobbyPlayerDTO(lobbyPlayer));
        }

        public void ChangeHost(int lobbyId, string nick)
        {
            foreach (LobbyPlayer player in Lobbies[lobbyId].Players.Values)
            {
                player.IsHost = player.Player.Nick == nick;
            }

            List<string> playersToPing = Lobbies[lobbyId].Players.Values.Select(c => c.Player.ConnectionId).ToList();
            hub.Clients.Clients(playersToPing).SendAsync("OnLobbyHostChange", new LobbyPlayerDTO(Lobbies[lobbyId].Players.First(c => c.Value.IsHost).Value));
        }
        public void Kick(int lobbyId, string nick)
        {
            LobbyPlayer player = Lobbies[lobbyId].Players.First(c => c.Value.Player.Nick == nick).Value;

            List<string> playersToPing = Lobbies[lobbyId].Players.Values.Select(c => c.Player.ConnectionId).ToList();
            hub.Clients.Clients(playersToPing).SendAsync("OnLobbyPlayerKick", new LobbyPlayerDTO(player));

            Lobbies[lobbyId].Leave(player.Player);
        }

        public void OnStartDownloading(int lobbyId, string nick)
        {
            Lobby lobby = Lobbies[lobbyId];

            List<string> playersToPing = lobby.Players.Values.Where(c => c.Player.Nick != nick).Select(c => c.Player.ConnectionId).ToList();
            hub.Clients.Clients(playersToPing).SendAsync("OnRemotePlayerStartDownloading", nick);
        }
        public void OnDownloadProgress(int lobbyId, string nick, int percent)
        {
            Lobby lobby = Lobbies[lobbyId];

            List<string> playersToPing = lobby.Players.Values.Where(c => c.Player.Nick != nick).Select(c => c.Player.ConnectionId).ToList();
            hub.Clients.Clients(playersToPing).SendAsync("OnRemotePlayerDownloadProgress", nick, percent);
        }
        public void OnDownloaded(int lobbyId, string nick)
        {
            Lobby lobby = Lobbies[lobbyId];

            List<string> playersToPing = lobby.Players.Values.Where(c => c.Player.Nick != nick).Select(c => c.Player.ConnectionId).ToList();
            hub.Clients.Clients(playersToPing).SendAsync("OnRemotePlayerDownloaded", nick);
        }
    }
}
