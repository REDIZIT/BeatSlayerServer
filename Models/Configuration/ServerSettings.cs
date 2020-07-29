using BeatSlayerServer.Models.Configuration.Modules;

namespace BeatSlayerServer.Models.Configuration
{
    public class ServerSettings
    {
        public string TracksFolder { get; set; }
        public string SharedFolder { get; set; }
        public string TutorialTrackname { get; set; }
        public string DefaultAvatarPath { get; set; }
        public string AccountsDataPath { get; set; }
        public string DefaultMapIcon { get; set; }

        public string StartingUrl { get; set; }

        public string ConnectionString { get; set; }

        public PublishingSettings Publishing { get; set; }
        public ModerationSettings Moderation { get; set; }

        public BotSettings Bot { get; set; }
        public EmailSettings Email { get; set; }

        public HeartbeatSettings Heartbeat { get; set; }
        public ChatSettings Chat { get; set; }
    }
}
