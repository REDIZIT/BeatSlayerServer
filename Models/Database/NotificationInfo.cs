namespace BeatSlayerServer.Utils.Notifications
{
    public class NotificationInfo
    {
        public int Id { get; set; }
        public NotificationType Type { get; set; }

        public string TargetNick { get; set; }

        public string RequesterNick { get; set; }

        /// <summary>String representation of object to save</summary>
        //public string ExtendedValue { get; set; }
        


        //public static NotificationInfo CoinsTransfer(string targetNick, string payerNick, int count)
        //{
        //    return new NotificationInfo()
        //    {
        //        TargetNick = targetNick,
        //        RequesterNick = payerNick,
        //        Type = NotificationType.CoinsTransfer/*,*/
        //        //ExtendedValue = count.ToString()
        //    };
        //}
    }

    public enum NotificationType
    {
        FriendInvite,
        FriendInviteAccept,
        FriendInviteReject,
        MapModeration,
        CoinsTransfer
    }
}
