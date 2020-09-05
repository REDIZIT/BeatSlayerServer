using BeatSlayerServer.Utils;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BeatSlayerServer.Services
{
    /// <summary>
    /// SignalR connection service
    /// </summary>
    public class ConnectionService
    {
        public List<ConnectedPlayer> ConnectedPlayers { get; set; } = new List<ConnectedPlayer>();
        public IEnumerable<ConnectedPlayer> LoggedPlayers => ConnectedPlayers.Where(c => !string.IsNullOrWhiteSpace(c.Nick));

        public Action<int> OnOnlineChange;

        public readonly IHubContext<GameHub> hub;
        private readonly ILogger<ConnectionService> logger;





        public ConnectionService(ILogger<ConnectionService> logger, IHubContext<GameHub> hub)
        {
            this.logger = logger;
            this.hub = hub;
            OnOnlineChange += NotifyOnlineChange;
        }


        public void OnPlayerConnected(string connectionId, string ip)
        {
            ConnectedPlayer player = new ConnectedPlayer(connectionId, ip);
            ConnectedPlayers.Add(player);

            logger.LogInformation("[{action}] {nick} {connectionId} from {ip}", "CONNECTED", player.Nick, connectionId, ip);

            OnOnlineChange(ConnectedPlayers.Count);
        }
        public void OnPlayerDisconnected(string connectionId)
        {
            ConnectedPlayer player = ConnectedPlayers.Find(c => c.ConnectionId == connectionId);
            ConnectedPlayers.RemoveAll(c => c.ConnectionId == connectionId);

            logger.LogInformation("[{action}] {nick} {connectionId} {sessionSeconds}", "DISCONNECTED", player.Nick, connectionId, player.SessionTime.TotalSeconds);

            OnOnlineChange(ConnectedPlayers.Count);
        }






        public void OnPlayerLoggedIn(string connectionId, string nick, bool isEditor, string ip)
        {
            ConnectedPlayer player = ConnectedPlayers.Find(c => c.ConnectionId == connectionId);
            if (player == null) return;

            player.Nick = nick;


            logger.LogInformation((isEditor ? "[EDITOR] " : "") + "[LOGGED IN] {nick} {connectionId} from {ip}", player.Nick, connectionId, ip);

            logger.LogInformation("[ONLINE] Current online is {online} and authed {authed}", ConnectedPlayers.Count, LoggedPlayers.Count());
        }


        public void NotifyOnlineChange(int count)
        {
            hub.Clients.All.SendAsync("OnOnlineChange", count);
            logger.LogInformation("[ONLINE] Current online is {online} and authed {authed}", count, LoggedPlayers.Count());
        }



        public bool TryFindPlayer(string nick, out ConnectedPlayer player)
        {
            player = ConnectedPlayers.Find(c => c.Nick == nick);

            return player != null;
        }
        public ConnectedPlayer FindPlayer(string nick)
        {
            if(!TryFindPlayer(nick, out ConnectedPlayer player)) throw new Exception($"Player with nick {nick} not found in connected");
            return player;
        }
    }
}
