using BeatSlayerServer.Models.Configuration;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace BeatSlayerServer.Services.Messaging
{
    /// <summary>
    /// Service managing all bots
    /// </summary>
    public class BotService
    {
        private readonly ServerSettings settings;

        private const string url_base = "http://localhost:4020/Bot";
        private const string url_sendMapPublished = "/SendMapPublished?trackname={0}&mapper={1}";
        private const string url_sendModerationRequest = "/SendModerationRequestMessage?trackname={0}&mapper={1}";
        private const string url_sendApprove = "/SendMapApprovedMessage?trackname={0}&mapper={1}&moderator={2}&comment={3}";
        private const string url_sendApproveMail = "/SendApprove?nick={0}&email={1}&trackname={2}&moderator={3}&reason={4}";
        private const string url_sendReject = "/SendMapRejectedMessage?trackname={0}&mapper={1}&moderator={2}&comment={3}";
        private const string url_sendRejectMail = "/SendReject?nick={0}&email={1}&trackname={2}&moderator={3}&reason={4}";

        private const string url_cheat = "/SendCheat?trackname={0}&moderator={1}";
        private const string url_coinsSyncLimit = "/SendCoinsSyncLimit?nick={0}&coins={1}";

        private const string url_restoreCode = "/SendRestorePasswordCode?nick={0}&email={1}&code={2}";
        private const string url_changeEmail = "/SendEmailChangeCode?nick={0}&email={1}&code={2}";


        private const string url_isAlive = "/Is{0}Alive";
        private const string url_isEnabled = "/Is{0}Enabled";
        private const string url_kill = "/Kill{0}";
        private const string url_build = "/Build{0}";




        public BotService(SettingsWrapper wrapper)
        {
            settings = wrapper.settings;
        }


        public async Task<string> SendMessage(string action, params string[] args)
        {
            try
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
            catch (Exception err)
            {
                Console.WriteLine("[ERROR] Bot send message error: " + err.Message);
                return await Task.Run(() => { return "Error"; });
            }
        }


        public async Task<bool> CheckAlive(string messager)
        {
            try { return bool.Parse(await SendMessage(url_isAlive, messager)); }
            catch { return false; }
        }
        public async Task<bool> CheckEnabled(string messager)
        {
            try { return bool.Parse(await SendMessage(url_isEnabled, messager)); }
            catch { return false; }
        }
        public async Task Kill(string messager)
        {
            await SendMessage(url_kill, messager);
        }
        public async Task Build(string messager)
        {
            await SendMessage(url_build, messager);
        }



        public async Task SendMapPublished(string trackname, string mapper)
        {
            await SendMessage(url_sendMapPublished, trackname, mapper);
        }
        public async Task SendModerationRequest(string trackname, string mapper, bool update)
        {
            await SendMessage(url_sendModerationRequest, trackname, mapper);
        }
        public async Task SendMapApproved(string trackname, string mapper, string moderator, string comment)
        {
            await SendMessage(url_sendApprove, trackname, mapper, moderator, comment);
        }
        public async Task SendMapRejected(string trackname, string mapper, string moderator, string comment)
        {
            await SendMessage(url_sendReject, trackname, mapper, moderator, comment);
        }
        public async Task ModeratorCheat(string trackname, string moderator)
        {
            await SendMessage(url_cheat, trackname, moderator);
        }

        public async Task CoinsSyncLimit(string nick, int coins)
        {
            await SendMessage(url_coinsSyncLimit, nick, coins.ToString());
        }


        public async Task SendMapApprovedMail(string mapper, string email, string trackname, string moderator, string comment)
        {
            await SendMessage(url_sendApproveMail, mapper, email, trackname, moderator, comment);
        }
        public async Task SendMapRejectedMail(string mapper, string email, string trackname, string moderator, string comment)
        {
            await SendMessage(url_sendRejectMail, mapper, email, trackname, moderator, comment);
        }


        public async Task SendRestorePasswordCode(string nick, string email, string code)
        {
            await SendMessage(url_restoreCode, nick, email, code);
        }
        public async Task SendChangeEmailCode(string nick, string email, string code)
        {
            await SendMessage(url_changeEmail, nick, email, code);
        }
    }
}