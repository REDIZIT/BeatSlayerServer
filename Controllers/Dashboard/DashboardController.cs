using BeatSlayerServer.Models.Dashboard;
using BeatSlayerServer.Services;
using BeatSlayerServer.Services.Messaging;
using BeatSlayerServer.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BeatSlayerServer.Controllers.Dashboard
{
    
    public class DashboardController : Controller
    {
        private readonly MyDbContext ctx;
        private readonly ConnectionService connectionService;
        private readonly BotService botService;

        public DashboardController(MyDbContext ctx, ConnectionService connectionService, BotService botService)
        {
            this.ctx = ctx;
            this.connectionService = connectionService;
            this.botService = botService;
        }

        [Authorize(Roles = "Developer, Moderator")]
        public async Task<IActionResult> Index()
        {
            IndexViewModel model = new IndexViewModel()
            {
                Nick = User.Identity.Name,
                Role = User.FindFirst(c => c.Type == ClaimsIdentity.DefaultRoleClaimType).Value,
                CurrentOnline = connectionService.ConnectedPlayers.Count,
                SessionsCount = connectionService.ConnectedPlayers.Where(c => !string.IsNullOrWhiteSpace(c.Nick)).Count(),
                IsDiscordBotAlive = await botService.CheckAlive("Discord"),
                IsDiscordBotEnabled = await botService.CheckEnabled("Discord"),
                IsVkBotAlive = await botService.CheckAlive("Vk"),
                IsVkBotEnabled = await botService.CheckEnabled("Vk"),
                IsEmailServiceAlive = await botService.CheckAlive("Email"),
                IsEmailServiceEnabled = await botService.CheckEnabled("Email")
            };

            return View(model);
        }













        [HttpGet]
        public IActionResult Login(string returnUrl = "")
        {
            LoginViewModel model = new LoginViewModel()
            {
                ReturnUrl = returnUrl
            };
            return View(model);
        }



        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await SignOut();

            return RedirectToAction("Index", "Home");
        }


        public async Task<IActionResult> Auth(string nick, string password, string masterkey)
        {
            if (masterkey != "sosipisun") return Content("Invalid masterkey");

            await Login(new LoginViewModel()
            {
                Nick = nick,
                Password = password
            });

            return Content("Done");
        }


        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var account = ctx.Players.FirstOrDefault(c => c.Nick == model.Nick);

                if (account != null)
                {
                    if (account.Password == SecurityHelper.GetMd5Hash(model.Password))
                    {
                        await Authenticate(account.Nick, account.Role.ToString());

                        if (string.IsNullOrWhiteSpace(model.ReturnUrl))
                        {
                            return RedirectToAction("Index", "Dashboard");
                        }
                        else
                        {
                            return Redirect(model.ReturnUrl);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("Password", "Неверный пароль");
                    }
                }
                else
                {
                    ModelState.AddModelError("Nick", "Нет такого аккаунта");
                }

            }

            return View(model);
        }





        private async Task Authenticate(string nick, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, nick),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, role)
            };

            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);


            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }
        private async Task SignOut()
        {
            await HttpContext.SignOutAsync();
        }
    }
}
