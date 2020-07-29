namespace BeatSlayerServer.Models.Dashboard
{
    public class IndexViewModel
    {
        public string Nick { get; set; }
        public string Role { get; set; }
        public int CurrentOnline { get; set; }
        public int SessionsCount { get; set; }

        public bool IsDiscordBotAlive { get; set; }
        public bool IsDiscordBotEnabled { get; set; }

        public bool IsVkBotAlive { get; set; }
        public bool IsVkBotEnabled { get; set; }

        public bool IsEmailServiceAlive { get; set; }
        public bool IsEmailServiceEnabled { get; set; }
    }
}
