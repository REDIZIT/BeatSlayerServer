using System.ComponentModel.DataAnnotations;

namespace BeatSlayerServer.Models.Dashboard
{
    public class LoginViewModel
    {
        [Required]
        public string Nick { get; set; }

        [Required]
        public string Password { get; set; }

        public string ReturnUrl { get; set; }
    }
}
