using BeatSlayerServer.Models.Database;
using System.Collections.Generic;

namespace BeatSlayerServer.Models.Configuration.Modules
{
    public class PurchasesSettings
    {
        public List<PurchaseModel> Purchases { get; set; } = new List<PurchaseModel>();
    }
}
