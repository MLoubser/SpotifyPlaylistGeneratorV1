using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SpotifyPlaylistGeneratorV1.Authentication
{
    public class ApplicationUser : IdentityUser
    {
        [MaxLength(200)]
        public string? FullNameX { get; set; }
        [MaxLength(200)]
        public string? LastNameX { get; set; }

        public string? RefreshTokenX { get; set; } = null;
        public string? ApiTokenX { get; set; } = null;

        public DateTime? LastRefreshX { get; set; } = null;
    }
}
