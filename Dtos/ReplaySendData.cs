namespace BeatSlayerServer.Dtos
{
    // This class is response from server on get replay
    // Used when player finished map
    public class ReplaySendData
    {
        public Grade Grade { get; set; }
        public double RP { get; set; }
        public int Coins { get; set; }
    }
    /// <summary>
    /// Replay grade (SS,S,A,B,C,D)
    /// </summary>
    public enum Grade
    {
        SS, S, A, B, C, D, Unknown
    }
}
