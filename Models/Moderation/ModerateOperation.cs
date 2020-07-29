namespace BeatSlayerServer.Models.Moderation
{
    public class ModerateOperation
    {
        public string trackname, nick;

        public enum State
        {
            Waiting = 0, Rejected = 1, Approved = 2
        }
        public State state;

        public enum UploadType
        {
            Requested = 0, Updated = 1
        }
        public UploadType uploadType;


        public string moderatorNick;
        public string moderatorComment;
    }
}
