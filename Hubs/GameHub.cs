using BeatSlayerServer.Utils.Shop;
using BeatSlayerServer.Services;
using BeatSlayerServer.Services.Chat;
using BeatSlayerServer.Services.Game;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AccountData = BeatSlayerServer.Utils.Database.AccountData;
using BeatSlayerServer.Dtos;
using BeatSlayerServer.Dtos.Mapping;
using BeatSlayerServer.Controllers;
using BeatSlayerServer.Hubs;
using BeatSlayerServer.Services.Dashboard;
using BeatSlayerServer.Models.Database;
using BeatSlayerServer.Dtos.Tutorial;

namespace BeatSlayerServer.Utils
{
    public class GameHub : Hub
    {
        private readonly MyDbContext context;
        private readonly ConnectionService connectionService;
        private readonly AccountService accountService;
        private readonly ILogger<GameHub> logger;

        private readonly ChatService chatService;
        private readonly RankingService rankingService;
        private readonly NotificationService notificationService;
        private readonly ShopService shopService;

        private readonly DashboardService dashboardService;

        public GameHub(MyDbContext context, ILogger<GameHub> logger, ConnectionService connectionService, AccountService accountService,
            ChatService chatService, RankingService rankingService, NotificationService notificationService, ShopService shopService,
            DashboardService dashboardService)
        {
            this.context = context;
            this.logger = logger;
            this.accountService = accountService;
            this.connectionService = connectionService;
            this.chatService = chatService;
            this.rankingService = rankingService;
            this.notificationService = notificationService;
            this.shopService = shopService;

            this.dashboardService = dashboardService;
        }

        public override Task OnConnectedAsync()
        {
            connectionService.OnPlayerConnected(Context.ConnectionId, GetIp(Context.GetHttpContext()));
            dashboardService.OnPlayerConnected(Context.ConnectionId, GetIp(Context.GetHttpContext()));
            return base.OnConnectedAsync();
        }
        public override Task OnDisconnectedAsync(Exception err)
        {
            connectionService.OnPlayerDisconnected(Context.ConnectionId);
            dashboardService.OnPlayerDisconnected(Context.ConnectionId, GetIp(Context.GetHttpContext()));
            return base.OnDisconnectedAsync(err);
        }


        // === Modules === //
        #region Chat

        #region Chat Groups
        public void Chat_GetGroups()
        {
            List<ChatGroupData> groups = chatService.GetGroups();
            Clients.Caller.SendAsync("OnGetGroups", groups);
        }
        public void Chat_JoinGroup(string nick, string groupName)
        {
            string json = JsonConvert.SerializeObject(chatService.JoinGroup(nick, groupName));

            Console.WriteLine(nick + " joined to " + groupName);

            Clients.Caller.SendAsync("OnJoinGroup", json);
        }
        public void Chat_LeaveGroup(string nick, string groupName)
        {
            Console.WriteLine(nick + " left " + groupName);

            chatService.LeaveGroup(Context.ConnectionId, groupName);
        }
        #endregion

        public void Chat_SendMessage(string nick, string message, Controllers.AccountRole role, string groupName)
        {
            var msg = chatService.SendMessage(nick, message, role, groupName);

            string json = JsonConvert.SerializeObject(msg);

            Clients.Group(groupName).SendAsync("OnSendChatMessage", json);
            Clients.All.SendAsync("OnSendChatMessage", json);
        }

        #endregion

        #region Accounts

        public void Accounts_GetAvatar(string nick)
        {
            var bytes = accountService.GetAvatar(nick);
            Clients.Caller.SendAsync("Accounts_OnGetAvatar", bytes);
        }
        public void Accounts_SignUp(string nick, string password, string country, string email)
        {
            OperationMessage op = accountService.SignUp(nick, password, country, email);
            Clients.Caller.SendAsync("Accounts_OnSignUp", op);
        }
        public void Accounts_LogIn(string nick, string password)
        {
            string ip = GetIp(Context.GetHttpContext());

            OperationMessage op = accountService.LogIn(nick, password);

            connectionService.OnPlayerLoggedIn(Context.ConnectionId, nick, false, ip);
            dashboardService.OnPlayerLoggedIn(Context.ConnectionId, GetIp(Context.GetHttpContext()), nick);

            Clients.Caller.SendAsync("Accounts_OnLogIn", op);
        }



        public async Task Accounts_Restore(string nick, string password)
        {
            OperationMessage success = await accountService.Restore(nick, password);
            await Clients.Caller.SendAsync("Accounts_OnRestore", success);
        }
        public void Accounts_ConfirmRestore(string code)
        {
            bool success = accountService.ConfirmRestore(code);
            Clients.Caller.SendAsync("Accounts_OnConfirmRestore", success);
        }
        public void Accounts_ChangePassword(string nick, string currentPassword, string newPassword)
        {
            OperationMessage op = accountService.ChangePassword(nick, currentPassword, newPassword);
            Clients.Caller.SendAsync("Accounts_OnChangePassword", op);
        }
        public async Task Accounts_SendChangeEmailCode(string nick, string newEmail)
        {
            await accountService.SendChangeEmailCode(nick, newEmail);
        }
        public void Accounts_ChangeEmail(string nick, string code)
        {
            OperationMessage op = accountService.ChangeEmail(nick, code);
            Clients.Caller.SendAsync("Accounts_OnChangeEmail", op);
        }
        public void Accounts_ChangeEmptyEmail(string nick, string newEmail)
        {
            OperationMessage op = accountService.ChangeEmptyEmail(nick, newEmail);
            Clients.Caller.SendAsync("Accounts_OnChangeEmail", op);
        }



        public void Accounts_UpdateInGameTime(string nick, int seconds)
        {
            accountService.UpdateInGameTime(nick, seconds);
        }
        public void Accounts_View(string nick)
        {
            AccountData acc = accountService.View(nick);
            Clients.Caller.SendAsync("Accounts_OnView", acc);
        }
        public void Accounts_Search(string str)
        {
            List<AccountData> accs = accountService.Find(str);
            Clients.Caller.SendAsync("Accounts_OnSearch", accs);
        }


        public void Accounts_SendReplay(string json)
        {
            Console.WriteLine("!!!!!!!! LEGACY WAY !!!!!!!!!!!");

            ReplaySendData data = rankingService.AddReplay(json);


            rankingService.CalculateLeaderboardPlaces();


            Clients.Caller.SendAsync("Accounts_OnSendReplay", data);
        }
        public async Task<ReplaySendData> SendReplay(ReplayData replay)
        {
            ReplaySendData data = await rankingService.AddReplay(replay);

            rankingService.CalculateLeaderboardPlaces();

            return data;
        }
        public void Accounts_GetBestReplay(string nick, string trackname, string creatornick)
        {
            ReplayInfo r = accountService.GetBestReplay(nick, trackname, creatornick);
            Clients.Caller.SendAsync("Accounts_OnGetBestReplay", r?.CutInfo());
        }
        public ReplayData GetBestReplay(string nick, string trackname, string creatornick)
        {
            return accountService.GetBestReplay(nick, trackname, creatornick)?.CutInfo();
        }

        public void Accounts_GetBestReplays(string nick, int count)
        {
            List<ReplayData> replayInfos = new List<ReplayData>();
            foreach (var r in accountService.GetBestReplays(nick, count))
            {
                replayInfos.Add(r.CutInfo());
            }

            Clients.Caller.SendAsync("Accounts_OnGetBestReplays", replayInfos);
        }

        public async Task<List<LeaderboardItem>> GetMapLeaderboard(string trackname, string nick)
        {
            return await accountService.GetMapLeaderboard(trackname, nick);
        }
        public async Task<List<LeaderboardItem>> GetGlobalLeaderboard()
        {
            return await accountService.GetGlobalLeaderboard();
        }

        public async Task<bool> IsPassed(string nick, string author, string name)
        {
            return await accountService.IsPassed(nick, author, name);
        }

        #endregion


        #region -- Friends --

        public void Friends_InviteFriend(string addToNick, string nick)
        {
            notificationService.InviteFriend(addToNick, nick);
        }
        public void Friends_AcceptInvite(string nick, int id)
        {
            notificationService.AcceptFriendInvite(nick, id);
        }
        public void Friends_RejectInvite(string nick, int id)
        {
            notificationService.RejectFriendInvite(nick, id);
        }
        public void Friends_GetFriends(string nick)
        {
            List<AccountData> friends = accountService.GetFriends(nick);
            Clients.Caller.SendAsync("Friends_OnGetFriends", friends);
        }
        public void Friends_RemoveFriend(string fromNick, string nick)
        {
            notificationService.RemoveFriend(fromNick, nick);
        }


        #endregion

        #region Notification


        public void Notification_Ok(string nick, int id)
        {
            notificationService.Remove(nick, id);
        }


        #endregion


        #region Shop

        public void Shop_SendCoins(string nick, int coins)
        {
            logger.LogInformation("[SHOP] {nick} spent {coins}", nick, -coins);
            shopService.SendCoins(nick, coins);
        }
        public async Task Shop_SyncCoins(string nick, int coins)
        {
            logger.LogInformation("[SHOP] Synced coins for {nick} in {coins}", nick, coins);
            await shopService.SyncCoins(nick, coins);
        }

        public List<PurchaseModel> Shop_UpgradePurchases(string nick, bool[] boughtSabers, bool[] tailsBought, bool[] boughtMaps)
        {
            return shopService.UpgradePurchases(nick, boughtSabers, tailsBought, boughtMaps);
        }
        public bool Shop_IsPurchaseBought(string nick, int shopItemId)
        {
            return shopService.IsPurchaseBought(nick, shopItemId);
        }
        public PurchaseModel Shop_TryBuy(string nick, int purchaseId)
        {
            return shopService.TryBuy(nick, purchaseId);
        }

        #endregion

        #region Tutorial

        public void Tutorial_Played(string json)
        {
            TutorialResult result = JsonConvert.DeserializeObject<TutorialResult>(json);

            string ip = GetIp(Context.GetHttpContext());
            logger.LogInformation("[{action}] {ip} {@result} {allmissed} {allsliced} {accuracy}", "TUTORIAL", ip, result, result.AllMissed, result.AllSliced, result.Accuracy * 100);
        }

        #endregion



        private string GetIp(HttpContext httpContext)
        {
            //string result = "";
            //if (remoteIpAddress != null)
            //{
            //    // If we got an IPV6 address, then we need to ask the network for the IPV4 address 
            //    // This usually only happens when the browser is on the same machine as the server.
            //    if (remoteIpAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
            //    {
            //        remoteIpAddress = System.Net.Dns.GetHostEntry(remoteIpAddress).AddressList
            //.First(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
            //    }
            //    result = remoteIpAddress.ToString();
            //}

            //return result;

            string remoteIpAddress = httpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            bool containsXForwarded = httpContext.Request.Headers.ContainsKey("X-Forwarded-For");
            if (containsXForwarded)
                remoteIpAddress = httpContext.Request.Headers["X-Forwarded-For"];

            return remoteIpAddress;
        }
    }
}


public interface IGameHub
{
    Task OnTest2();
    Task OnTestPar(int i);
}