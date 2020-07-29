using System;

namespace BeatSlayerServer.Utils.Statistics
{
    /// <summary>
    /// Model describes statistics value in <see cref="HeartbeatFrame"/>
    /// </summary>
    public interface IHeartbeatData
    {
        void Apply(HeartbeatFrame frame);
    }









    public class HeartbeatDataOnline : IHeartbeatData
    {
        public readonly int currentOnline;
        
        public HeartbeatDataOnline(int currentOnline)
        {
            this.currentOnline = currentOnline;
        }

        public void Apply(HeartbeatFrame frame)
        {
            frame.SignalrOnline = Math.Max(currentOnline, frame.SignalrOnline);
        }
    }



    public class HeartbeatDataMapPlayed : IHeartbeatData
    {
        public readonly bool isApproved;

        public HeartbeatDataMapPlayed(bool isApproved)
        {
            this.isApproved = isApproved;
        }

        public void Apply(HeartbeatFrame frame)
        {
            if (isApproved) frame.GamesApprovedCount++;
            else frame.GamesCount++;
        }
    }



    public class HeartbeatDataGameLaunch : IHeartbeatData
    {
        /// <summary>
        /// Is player without account
        /// </summary>
        public readonly bool isAnonymous;

        public HeartbeatDataGameLaunch(bool isAnonymous)
        {
            this.isAnonymous = isAnonymous;
        }

        public void Apply(HeartbeatFrame frame)
        {
            if (isAnonymous) frame.GameLaunchAnonimCount++;
            else frame.GameLaunchCount++;
        }
    }
}
