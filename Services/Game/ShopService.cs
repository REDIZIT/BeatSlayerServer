using BeatSlayerServer.Utils.Database;
using BeatSlayerServer.Services.Messaging;
using System;
using System.Linq;
using System.Threading.Tasks;
using BeatSlayerServer.Extensions;
using BeatSlayerServer.Models.Database;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Newtonsoft.Json;
using BeatSlayerServer.Models.Configuration;
using BeatSlayerServer.Services;

namespace BeatSlayerServer.Utils.Shop
{
    public class ShopService
    {
        private readonly MyDbContext ctx;
        private readonly BotService botService;
        private readonly ILogger<ShopService> logger;
        private readonly ServerSettings settings;

        public ShopService(MyDbContext ctx, ILogger<ShopService> logger, SettingsWrapper wrapper, BotService botService)
        {
            this.ctx = ctx;
            this.logger = logger;
            settings = wrapper.settings;
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

            if (acc.Purchases != null && acc.Purchases.Count > 0) return null;

            List<bool> purchases = new List<bool>();
            purchases.AddRange(boughtSabers);
            purchases.AddRange(boughtTails);
            purchases.AddRange(boughtMaps);

            //logger.LogDebug("Debugging");
            logger.LogInformation("Purchases list is => {@purchases}", settings.Shop.Purchases);

            for (int i = 0; i < purchases.Count; i++)
            {
                //logger.LogInformation("Purchase[i] = {isBought}", purchases[i]);
                if (!purchases[i]) continue;

                //logger.LogInformation("|-- Add to acc");
                PurchaseModel item = settings.Shop.Purchases.FirstOrDefault(c => c.ItemId == i);
                acc.Purchases.Add(new PurchaseModel(item.Name, item.Cost, item.ItemId));
                //acc.Purchases.Add(item);
                //logger.LogInformation("||---- Success? {success}", acc.Purchases.Any(c => c.Id == i));
            }

            ctx.SaveChanges();

            logger.LogInformation("Acc purchases now are {@purchases}", acc.Purchases.ToList());

            return acc.Purchases.ToList();

            //return null;
        }
        public bool IsPurchaseBought(string nick, int purchaseId)
        {
            if (!ctx.TryFindAccount(nick, out Account acc)) return false;

            return acc.Purchases.Any(c => c.ItemId == purchaseId);
            //return false;
        }
        public PurchaseModel TryBuy(string nick, int purchaseId)
        {
            if (!ctx.TryFindAccount(nick, out Account acc)) return null;

            // Get PurchaseModel from table
            PurchaseModel purchase = settings.Shop.Purchases.FirstOrDefault(c => c.ItemId == purchaseId);
            if (purchase == null)
            {
                logger.LogError("[{action}] {nick} tried to bought item ({purchaseId}) but it is null", "BUY", nick, purchaseId);
                return null;
            }

            if (acc.Coins < purchase.Cost)
            {
                logger.LogInformation("[{action}] {nick} tried to bought item ({purchaseId}) but he can't pay for it. His balance is {coins}, but item costs {cost}",
                    "BUY", nick, purchaseId, acc.Coins, purchase.Cost);
                return null;
            }
            // Return if player already has this thing bought
            if (acc.Purchases.Any(c => c.ItemId == purchaseId))
            {
                logger.LogError("[{action}] {nick} tried to bought item ({purchaseId}) but he already bought it", "BUY", nick, purchaseId);
                return null;
            }



            acc.Coins -= purchase.Cost;
            acc.Purchases.Add(purchase);

            logger.LogInformation("[{action}] {nick} bought {purchaseId} and now has {coins} coins", "BUY", nick, purchaseId, acc.Coins);

            ctx.SaveChanges();

            return purchase;
        }
        public bool GiveItem(Account acc, int itemId, string masterpass)
        {
            if (!SecurityHelper.CheckMasterpass(masterpass)) return false;

            // Get PurchaseModel from table
            PurchaseModel purchase = settings.Shop.Purchases.FirstOrDefault(c => c.ItemId == itemId);
            if (purchase == null)
            {
                return false;
            }

            acc.Coins -= purchase.Cost;
            acc.Purchases.Add(purchase);

            logger.LogInformation("[{action}] {nick} got a {itemName}({itemId})", "Give item", acc.Nick, purchase.Name, purchase.ItemId);

            ctx.SaveChanges();
            return true;
        }
    }
}
