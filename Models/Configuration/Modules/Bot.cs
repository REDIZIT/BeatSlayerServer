namespace BeatSlayerServer.Models.Configuration.Modules
{
    public class BotSettings
    {

        public long Vk_UserId { get; set; }
        public string Vk_AcessToken { get; set; }


        public bool IsDiscordBotEnabled { get; set; } = true;
        public string Discord_Token { get; set; }

        public ulong Discord_ModerationChannelId { get; set; } = 701781287064698961;
        public ulong Discord_PublicChannelId { get; set; } = 702088186779664425;
        public ulong Discord_ModerationRoleId { get; set; } = 694150045854990417;
        public ulong Discord_DeveloperRoleId { get; set; } = 692845394257903637;

        /// <summary>
        /// Prefixes on which bot will be triggered
        /// </summary>
        public string[] Discord_OtherPrefixes { get; set; } = new string[] { "!" };
        public ulong[] Discord_BotChannels { get; set; }
        public ulong Discord_SuggestionChannelId { get; set; } = 729017598473404499;
        public ulong Discord_BugsChannelId { get; set; } = 729017598473404499;
    }
}
