using BeatSlayerServer.Models.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace BeatSlayerServer.Services.Messaging.Discord
{
    /// <summary>
    /// Works like web api wrapper for external api application
    /// </summary>
    public class DiscordBotWrapper
    {
        public readonly IHostEnvironment env;
        private readonly ServerSettings settings;
        private readonly ILogger<DiscordBotWrapper> logger;


        private const string url_base = "http://localhost:4020/Bot";
        private const string url_moderation = "/SendModerationRequestMessage?trackname={0}&mapper={1}";
        private const string url_approve = "/SendMapApprovedMessage?trackname={0}&mapper={1}&moderator={2}&comment={3}";
        private const string url_reject = "/SendMapRejectedMessage?trackname={0}&mapper={1}&moderator={2}&comment={3}";
        private const string url_publish = "/SendMapPublished?trackname={0}&mapper={1}";
        private const string url_kill = "/KillDiscord";
        private const string url_build = "/BuildDiscord";

        private const string url_isAlive = "/IsAlive";
        private const string url_isEnabled = "/IsEnabled";



        public DiscordBotWrapper(IHostEnvironment env, SettingsWrapper wrapper, ILogger<DiscordBotWrapper> logger)
        {
            settings = wrapper.settings;
            this.env = env;
            this.logger = logger;
        }
        

        public async Task<string> SendMessage(string action, params string[] args)
        {
            string url = url_base + action;

            for (int i = 0; i < args.Length; i++)
            {
                args[i] = HttpUtility.UrlEncode(args[i]);
            }

            url = string.Format(url, args);

            WebClient c = new WebClient();
            return await c.DownloadStringTaskAsync(url);
        }




        public async Task<bool> IsAlive()
        {
            try
            {
                return bool.Parse(await SendMessage(url_isAlive));
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> IsEnabled()
        {
            try
            {
                return bool.Parse(await SendMessage(url_isEnabled));
            }
            catch
            {
                return false;
            }
        }
        public async Task Kill()
        {
            await SendMessage(url_kill);
        }
        public async Task Build()
        {
            await SendMessage(url_build);
        }
        public async Task SendModerationRequestMessage(string trackname, string mapper)
        {
            //ModerationRequestMessage msg = new ModerationRequestMessage(trackname, mapper, ModeratorRole);
            await SendMessage(url_moderation, trackname, mapper);
        }

        public async Task SendMapPublishedMessage(string trackname, string mapper)
        {
            //MapPublishMessage msg = new MapPublishMessage(trackname, mapper);
            await SendMessage(url_publish, trackname, mapper);
        }

        public async Task SendMapApprovedMessage(string trackname, string mapper, string moderator, string comment)
        {
            //MapApproveMessage msg = new MapApproveMessage(trackname, mapper, moderator, comment);
            await SendMessage(url_approve, trackname, mapper, moderator, comment);
        }

        public async Task SendMapRejectedMessage(string trackname, string mapper, string moderator, string comment)
        {
            //MapRejectMessage msg = new MapRejectMessage(trackname, mapper, moderator, comment);
            await SendMessage(url_reject, trackname, mapper, moderator, comment);
        }
    }
}
