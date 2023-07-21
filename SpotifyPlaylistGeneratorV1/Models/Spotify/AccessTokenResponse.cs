namespace SpotifyPlaylistGeneratorV1.Models.Spotify
{
    public class AccessTokenResponseModel
    {
        public string access_token { get; set; } = default!;
        public string token_type { get; set; } = default!;
        public string scope { get; set; } = default!;
        public int expires_in { get; set; } = default!;
        public string refresh_token { get; set; } = default!;
    }
}
