using BeatSlayerServer.Hubs;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;

namespace BeatSlayerServer.Services.Dashboard
{
    public class DashboardService
    {
        private readonly IHubContext<DashboardHub> hub;
        private readonly ConnectionService connectionService;

        public DashboardService(IHubContext<DashboardHub> hub, ConnectionService connectionService)
        {
            this.hub = hub;
            this.connectionService = connectionService;
            connectionService.OnOnlineChange += OnOnlineChange;
        }


        public void OnPlayerConnected(string connectionId, string ip)
        {
            hub.Clients.All.SendAsync("OnPlayerConnected", connectionId, ip);
        }
        public void OnPlayerLoggedIn(string connectionId, string ip, string nick)
        {
            hub.Clients.All.SendAsync("OnPlayerLoggedIn", connectionId, ip, nick);
            OnOnlineChange(-1);
        }
        public void OnPlayerDisconnected(string connectionId, string nick)
        {
            hub.Clients.All.SendAsync("OnPlayerDisconnected", connectionId, nick);
        }
        public void OnOnlineChange(int online)
        {
            hub.Clients.All.SendAsync("OnOnlineChange", connectionService.ConnectedPlayers.Count, connectionService.LoggedPlayers.Count());
        }


        public void OnMapPlayed(string trackname, string mapper, string player, float score, float RP, float accuracy)
        {
            score = (int)Math.Floor(score);
            RP = (int)Math.Floor(RP);
            accuracy = (float)Math.Floor(accuracy * 1000) / 10f;
            hub.Clients.All.SendAsync("OnMapPlayed", trackname, mapper, player, score, RP, accuracy);
        }
    }
}
