using System.ComponentModel.DataAnnotations;

namespace SpotifyPlaylistGeneratorV1.Models.Views
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Email address")]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string? Password { get; set; }
    }
}
