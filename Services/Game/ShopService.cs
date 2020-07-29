using BeatSlayerServer.Utils.Database;
using BeatSlayerServer.Services.Messaging;
using System;
using System.Linq;
using System.Threading.Tasks;
using BeatSlayerServer.Extensions;
using BeatSlayerServer.Models.Database;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace BeatSlayerServer.Utils.Shop
{
    public class ShopService
    {
        private readonly MyDbContext ctx;
        private readonly BotService botService;
        private readonly ILogger<ShopService> logger;

        public ShopService(MyDbContext ctx, ILogger<ShopService> logger, BotService botService)
        {
            this.ctx = ctx;
            this.logger = logger;
            this.botService = botService;
        }

        public void SendCoins(string nick, int coins)
        {
            Account acc = ctx.Players.FirstOrDefault(c => c.Nick == nick);
            if (acc == null) return;

            Console.WriteLine(string.Format("Add {0} coins to {1}", coins, nick));

            acc.Coins += coins;
            ctx.SaveChanges();
        }

        public async Task SyncCoins(string nick, int coins)
        {
            Account acc = ctx.Players.FirstOrDefault(c => c.Nick == nick);
            if (acc == null) return;

            if (acc.Coins != -1) return;

            if (coins > 200000)
            {
                await botService.CoinsSyncLimit(nick, coins);
            }

            acc.Coins = coins;
            ctx.SaveChanges();
        }



        public List<PurchaseModel> UpgradePurchases(string nick, bool[] boughtSabers, bool[] boughtTails, bool[] boughtMaps)
        {
            logger.LogInformation("[{action} {subaction}] {nick} upgraded with {@boughtSabers} {@boughtTails} {@boughtMaps}", "UPGRADE", "PURCHASES", nick, boughtSabers, boughtTails, boughtMaps);

            var acc = ctx.Players.FirstOrDefault(c => c.Nick == nick);
            if (acc == null) return null;

            for (int i = 0; i < boughtSabers.Length; i++)
            {
                var item = ctx.Purchases.FirstOrDefault(c => c.Id == i);
                acc.Purchases.Add(item);
            }
            for (int i = 7; i < boughtTails.Length; i++)
            {
                var item = ctx.Purchases.FirstOrDefault(c => c.Id == i);
                acc.Purchases.Add(item);
            }
            for (int i = 10; i < boughtMaps.Length; i++)
            {
                var item = ctx.Purchases.FirstOrDefault(c => c.Id == i);
                acc.Purchases.Add(item);
            }

            ctx.SaveChanges();

            return acc.Purchases;
        }
        public bool IsPurchaseBought(string nick, int purchaseId)
        {
            if (!ctx.TryFindAccount(nick, out Account acc)) return false;

            return acc.Purchases.Any(c => c.Id == purchaseId);
        }
        public PurchaseModel TryBuy(string nick, int purchaseId)
        {
            if (!ctx.TryFindAccount(nick, out Account acc)) return null;

            // Get PurchaseModel from table
            PurchaseModel purchase = ctx.Purchases.FirstOrDefault(c => c.Id == purchaseId);
            if (purchase == null)
            {
                logger.LogError("[{action}] {nick} tryied to bought item ({purchaseId}) but it is null", "BUY", nick, purchaseId);
                return null;
            }

            if (acc.Coins < purchase.Cost)
            {
                logger.LogError("[{action}] {nick} tryied to bought item ({purchaseId}) but he can't pay for it. His balance is {coins}, but item costs {cost}",
                    "BUY", nick, purchaseId, acc.Coins, purchase.Cost);
                return null;
            }
            // Return if player already has this thing bought
            if (acc.Purchases.Any(c => c.Id == purchaseId))
            {
                logger.LogError("[{action}] {nick} tryied to bought item ({purchaseId}) but he already bought it", "BUY", nick, purchaseId);
                return null;
            }



            acc.Coins -= purchase.Cost;
            acc.Purchases.Add(purchase);

            logger.LogInformation("[{action}] {nick} bought {purchaseId} and now has {coins} coins", "BUY", nick, purchaseId, acc.Coins);

            ctx.SaveChanges();

            return purchase;
        }
    }
}
