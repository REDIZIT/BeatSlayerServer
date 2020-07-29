using BeatSlayerServer.Services;
using BeatSlayerServer.Services.Game;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeatSlayerServer.Utils
{
    public class EditorHub : Hub
    {
        private readonly ConnectionService connectionService;
        private readonly AccountService accountService;

        public EditorHub(ConnectionService connectionService, AccountService accountService)
        {
            this.connectionService = connectionService;
            this.accountService = accountService;
        }


        public async Task<OperationMessage> LogIn(string nick, string password)
        {
            return await Task.Run(() =>
            {
                string ip = Context.GetHttpContext().Connection.RemoteIpAddress.ToString();

                OperationMessage op = accountService.LogIn(nick, password);

                connectionService.OnPlayerLoggedIn(Context.ConnectionId, nick, true, ip);

                return op;
            });
        }
        public async Task<OperationMessage> GetPublishedMaps(string nick, string password)
        {
            return await Task.Run(() => accountService.GetPublishedMaps(nick, password));
        }
        public async Task<byte[]> GetAvatar(string nick)
        {
            return await Task.Run(() => accountService.GetAvatar(nick));
        }
    }
}
