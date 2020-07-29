namespace BeatSlayerServer.Models.Database
{
    public class CodeVerificationRequest
    {
        public int Id { get; set; }
        public string Nick { get; set; }
        public string Value { get; set; }
        public string Code { get; set; }

        public CodeVerificationRequest(string Nick, string Value, string Code)
        {
            this.Nick = Nick;
            this.Value = Value;
            this.Code = Code;
        }
    }
}