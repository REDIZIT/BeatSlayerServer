﻿using BeatSlayerServer.Utils;
using BeatSlayerServer.Utils.Database;
using BeatSlayerServer.Utils.Notifications;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

        public void InviteFriend(string targetNick, string requesterNick)
        {
            if (!ctx.Players.Any(c => c.Nick == targetNick)) return;

            if (!ctx.Players.Any(c => c.Nick == requesterNick)) return;

            SendFriendInvite(targetNick, requesterNick);
        }

        /// Really bad, but there is circular dependency
        /// I'm scared..
        public void AcceptFriendInvite(string nick, int id)
        {
            if (!accountService.TryFindAccount(nick, out Account acc)) return;

            if (!TryGet(id, out NotificationInfo not)) return;

            if (!accountService.TryFindAccount(not.RequesterNick, out Account requester)) return;



            acc.Friends.Add(requester);
            requester.Friends.Add(acc);
            ctx.SaveChanges();


            Remove(nick, id);
            Send(not.RequesterNick, new NotificationInfo()
            {
                RequesterNick = not.RequesterNick,
                TargetNick = nick,
                Type = NotificationType.FriendInviteAccept
            });
        }
        public void RejectFriendInvite(string nick, int id)
        {
            if (!accountService.TryFindAccount(nick, out Account acc)) return;

            if (!TryGet(id, out NotificationInfo not)) return;


            Remove(nick, id);
            Send(not.RequesterNick, new NotificationInfo()
            {
                RequesterNick = not.RequesterNick,
                TargetNick = nick,
                Type = NotificationType.FriendInviteReject
            });
        }
        public void RemoveFriend(string fromNick, string nick)
        {
            if (!accountService.TryFindAccount(fromNick, out Account fromPlayer)) return;
            if (!accountService.TryFindAccount(nick, out Account player)) return;

            Console.WriteLine("Remove friend " + nick + " from " + fromNick);

            player.Friends.Remove(fromPlayer);
            fromPlayer.Friends.Remove(player);
            ctx.SaveChanges();
        }





        public void Send(string nick, NotificationInfo notification)
        {
            if (!accountService.TryFindAccount(nick, out Account acc)) return;



            ctx.Notifications.Add(notification);
            ctx.SaveChanges();


            Console.WriteLine("Send notification " + nick + " " + notification.Type.ToString() + " with id " + notification.Id);

            if(connectionService.TryFindPlayer(nick, out ConnectedPlayer conn))
            {
                Console.WriteLine("Conn is not null");
                connectionService.hub.Clients.Client(conn.ConnectionId).SendAsync("Notification_OnSend", notification);
                Console.WriteLine("Sent");
            }
            else Console.WriteLine("Conn is null");

            acc.Notifications.Add(notification);

            Console.WriteLine("Now has " + acc.Notifications.Count);
            Console.WriteLine(JsonConvert.SerializeObject(acc.Notifications, Formatting.Indented));

            ctx.SaveChanges();
        }
        public void Remove(string nick, int id)
        {
            Console.WriteLine("RemoveNotify " + id);

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



        public void SendFriendInvite(string toPlayer, string reqPlayer)
        {
            NotificationInfo notif = new NotificationInfo()
            {
                Type = NotificationType.FriendInvite,
                TargetNick = toPlayer,
                RequesterNick = reqPlayer
            };

            Send(toPlayer, notif);
        }
    }
}
