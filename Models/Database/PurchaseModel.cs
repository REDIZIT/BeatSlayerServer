namespace BeatSlayerServer.Models.Database
{
    public class PurchaseModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Cost { get; set; }

        public PurchaseModel(string name, int cost)
        {
            Name = name;
            Cost = cost;
        }
    }
}
