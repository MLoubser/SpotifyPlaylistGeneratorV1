namespace SpotifyPlaylistGeneratorV1.Models.Config
{
    public class SpotifyConfigModel
    {
        public string BaseUrl { get; set; } = default!;
        public int RefreshTokenExpireMinutes { get; set; } = default!;
        public string ClientId { get; set; } = default!;
        public string ClientSecret { get; set; } = default!;
        public string AppName { get; set; } = default!;
        public string RedirectUrl { get; set; } = "https://localhost:7000/callback/spotify";
        public string RequiredScope { get; set; } = "playlist-modify-private";

    }
}
