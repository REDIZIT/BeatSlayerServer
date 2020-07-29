using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;

namespace BeatSlayerServer.Hubs
{
    [Authorize(Roles = "Developer")]
    public class DashboardHub : Hub
    {
        public void DoLog()
        {
            Clients.All.SendAsync("OnLog", DateTime.Now.ToLongTimeString());
        }
    }

    public interface IDashboardHub
    {
        void OnPlayerConnected(string connectionId, string ip);
    }
}
