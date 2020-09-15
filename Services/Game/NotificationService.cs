using BeatSlayerServer.Utils;
using BeatSlayerServer.Utils.Database;
using BeatSlayerServer.Utils.Notifications;
using Microsoft.AspNetCore.SignalR;
using System.Linq;

namespace BeatSlayerServer.Services.Game
{
    public class NotificationService
    {
        private readonly MyDbContext ctx;
        private readonly AccountService accountService;
        private readonly ConnectionService connectionService;

        public NotificationService(MyDbContext ctx, AccountService accountService, ConnectionService connectionService)
        {
            this.ctx = ctx;
            this.accountService = accountService;
            this.connectionService = connectionService;
        }


        public void SendFriendInvite(string targetPlayer, string requesterPlayer)
        {
            NotificationInfo notif = new NotificationInfo()
            {
                TargetNick = targetPlayer,
                RequesterNick = requesterPlayer,
                Type = NotificationType.FriendInvite
            };

            Send(notif);
        }


        public void AcceptFriendInvite(string nick, int id)
        {
            if (!accountService.TryFindAccount(nick, out Account acc)) return;

            if (!TryGet(id, out NotificationInfo not)) return;

            if (!accountService.TryFindAccount(not.RequesterNick, out Account requester)) return;



            acc.Friends.Add(requester);
            requester.Friends.Add(acc);
            ctx.SaveChanges();


            Remove(nick, id);
            Send(new NotificationInfo()
            {
                TargetNick = not.RequesterNick,
                RequesterNick = not.TargetNick,
                Type = NotificationType.FriendInviteAccept
            });
        }
        public void RejectFriendInvite(string nick, int id)
        {
            if (!accountService.TryFindAccount(nick, out Account acc)) return;

            if (!TryGet(id, out NotificationInfo not)) return;


            Remove(nick, id);
            Send(new NotificationInfo()
            {
                TargetNick = not.RequesterNick,
                RequesterNick = not.TargetNick,
                Type = NotificationType.FriendInviteReject
            });
        }
        public void RemoveFriend(string fromNick, string nick)
        {
            if (!accountService.TryFindAccount(fromNick, out Account fromPlayer)) return;
            if (!accountService.TryFindAccount(nick, out Account player)) return;

            player.Friends.Remove(fromPlayer);
            fromPlayer.Friends.Remove(player);
            ctx.SaveChanges();
        }





        public void Send(NotificationInfo notification)
        {
            if (!accountService.TryFindAccount(notification.TargetNick, out Account acc)) return;



            ctx.Notifications.Add(notification);
            acc.Notifications.Add(notification);
            ctx.SaveChanges();

            if(connectionService.TryFindPlayer(notification.TargetNick, out ConnectedPlayer conn))
            {
                connectionService.hub.Clients.Client(conn.ConnectionId).SendAsync("Notification_OnSend", notification);
            }
        }
        public void Remove(string nick, int id)
        {
            if (!accountService.TryFindAccount(nick, out Account acc)) return;

            ctx.Notifications.Remove(ctx.Notifications.FirstOrDefault(c => c.Id == id));
            acc.Notifications.RemoveAll(c => c.Id == id);

            ctx.SaveChanges();
        }
        public bool TryGet(int id, out NotificationInfo notification)
        {
            notification = ctx.Notifications.FirstOrDefault(c => c.Id == id);
            return notification != null;
        }
    }
}
