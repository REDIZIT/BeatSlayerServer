namespace InEditor.Analyze
{
    public class AnalyzeResult
    {
        public string DifficultyName { get; set; }
        public int DifficultyStars { get; set; }
        public int DifficultyId { get; set; }


        public float MaxScore { get; set; }
        public float MaxRP { get; set; }
        public int CubesCount { get; set; }
        public int LinesCount { get; set; }
        public float ScorePerBlock { get; set; }
        public float RPPerBlock { get; set; }
    }
}
