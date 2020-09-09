namespace BeatSlayerServer.Models.Multiplayer.Chat
{
    public class LobbySystemChatMessage : LobbyChatMessage
    {
        public SystemMessageType MessageType { get; set; }
        public override string GetMessage()
        {
            return string.Format(MessageType+ " {0}", PlayerNick);
        }

        public enum SystemMessageType
        {
            Join, Leave, Kick
        }
    }
}
