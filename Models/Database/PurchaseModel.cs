namespace BeatSlayerServer.Models.Database
{
    public class PurchaseModel
    {
        /// <summary>
        /// Id in database
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Item id in config
        /// </summary>
        public int ItemId { get; set; }
        public string Name { get; set; }
        public int Cost { get; set; }

        public PurchaseModel() { }
        public PurchaseModel(string name, int cost, int id)
        {
            Name = name;
            Cost = cost;
            ItemId = id;
        }
    }
}
