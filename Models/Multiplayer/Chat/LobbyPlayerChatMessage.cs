namespace BeatSlayerServer.Models.Multiplayer.Chat
{
    public abstract class LobbyChatMessage
    {
        public string PlayerNick { get; set; }
        public abstract string GetMessage();
    }
    public class LobbyPlayerChatMessage : LobbyChatMessage
    {
        public string Message { get; set; }

        public override string GetMessage()
        {
            return Message;
        }
    }
}
