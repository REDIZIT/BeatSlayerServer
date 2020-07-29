namespace BeatSlayerServer.Utils.Notifications
{
    public class NotificationInfo
    {
        public int Id { get; set; }
        public NotificationType Type { get; set; }

        public string TargetNick { get; set; }

        public string RequesterNick { get; set; }
    }

    public enum NotificationType
    {
        FriendInvite,
        FriendInviteAccept,
        FriendInviteReject,
        MapModeration,
    }
}
