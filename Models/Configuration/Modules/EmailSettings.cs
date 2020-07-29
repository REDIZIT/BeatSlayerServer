namespace BeatSlayerServer.Models.Configuration.Modules
{
    public class EmailSettings
    {
        public bool IsEnabled { get; set; } = true;

        public string Login { get; set; }
        public string Password { get; set; }

        public string Template_Code { get; set; } = "wwwroot/templates/mail-code-template.html";
        public string Template_Map { get; set; } = "wwwroot/templates/mail-template.html";
    }
}
